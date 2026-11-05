using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Enums;
using Plexa.Domain.Rules;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Plexa.Infrastructure.Services;

public sealed class ReportService : ServiceBase, IReportService
{
    private readonly ITranslator _t;

    public ReportService(PlexaDbContext db, IUserContext user, IClock clock, ITranslator translator) : base(db, user, clock)
    {
        _t = translator;
    }

    public async Task<ReportPageDto> GetAsync(ReportFilters filters, CancellationToken ct = default)
    {
        var users = await LoadUsersAsync(ct);
        var sprints = await Db.Sprints.AsNoTracking()
            .Where(s => s.WorkspaceId == WorkspaceId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(ct);

        var query = Db.Tasks.AsNoTracking()
            .Where(t => t.WorkspaceId == WorkspaceId)
            .Include(t => t.TimeEntries)
            .Include(t => t.Project)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Sprint) && filters.Sprint != "active" && filters.Sprint != "backlog" && Guid.TryParse(filters.Sprint, out var sprintId))
        {
            query = query.Where(t => t.SprintId == sprintId);
        }
        else if (filters.Sprint == "active")
        {
            var activeId = sprints.FirstOrDefault(s => s.Status == SprintStatus.Active)?.Id;
            if (activeId.HasValue)
            {
                query = query.Where(t => t.SprintId == activeId.Value);
            }
        }

        if (filters.MemberId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == filters.MemberId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Tag))
        {
            query = query.Where(t => t.Tag == filters.Tag);
        }

        var tasks = await query.ToListAsync(ct);

        var rows = tasks
            .Select(t => new
            {
                Task = t,
                Logged = TimeSpan.FromSeconds(t.TimeEntries.Sum(e => e.Seconds))
            })
            .Where(x => x.Task.EstimateHours > 0 || x.Logged > TimeSpan.Zero)
            .Where(x => !filters.TimeLogged || x.Logged > TimeSpan.Zero)
            .Where(x => !filters.VarianceGt0 || x.Logged > TimeSpan.FromHours((double)x.Task.EstimateHours))
            .OrderBy(x => x.Task.Title)
            .Select(x => new HoursRowDto(
                x.Task.Id,
                x.Task.Title,
                PersonOf(users, x.Task.AssigneeId),
                x.Task.Tag,
                x.Task.EstimateHours,
                x.Logged,
                x.Logged - TimeSpan.FromHours((double)x.Task.EstimateHours),
                x.Logged > TimeSpan.Zero,
                x.Task.DueDate.HasValue && x.Task.DueDate.Value < Clock.Today && x.Task.Status != WorkTaskStatus.Completed))
            .ToList();

        var totalHours = rows.Sum(r => r.TaskHours);
        var totalLogged = TimeSpan.FromSeconds(rows.Sum(r => r.Logged.TotalSeconds));
        var totalVariance = totalLogged - TimeSpan.FromHours((double)totalHours);
        var withTime = rows.Count(r => r.Logged > TimeSpan.Zero);

        var scope = string.IsNullOrWhiteSpace(filters.Sprint) || filters.Sprint == "backlog"
            ? _t["Backlog"]
            : filters.Sprint == "active" ? _t["Active sprint"] : _t["Sprint"];

        var hours = new HoursReportDto(
            filters,
            BuildSprintOptions(sprints, filters.Sprint),
            users.Values.OrderBy(u => u.FullName).Select(u => u.ToPerson()).ToList(),
            rows,
            totalHours,
            totalLogged,
            totalVariance,
            _t.Format("Showing {0} tasks · {1} · {2} with time logged", rows.Count, scope, withTime));

        return new ReportPageDto(hours, await GetDepartmentRowsAsync(ct));
    }

    public async Task<byte[]> ExportAsync(ReportFilters filters, string format, CancellationToken ct = default)
    {
        var page = await GetAsync(filters, ct);
        return format.ToLowerInvariant() switch
        {
            "csv" => BuildCsv(page.Hours),
            "xlsx" => BuildXlsx(page),
            "pdf" => BuildPdf(page),
            _ => []
        };
    }

    public async Task<byte[]> ExportDepartmentAsync(string format, CancellationToken ct = default)
    {
        var page = await GetAsync(ReportFilters.Empty, ct);
        return format.ToLowerInvariant() switch
        {
            "csv" => BuildDepartmentCsv(page.Departments),
            "xlsx" => BuildXlsx(page),
            "pdf" => BuildPdf(page),
            _ => []
        };
    }

    private async Task<IReadOnlyList<DepartmentRowDto>> GetDepartmentRowsAsync(CancellationToken ct)
    {
        var departments = await Db.Departments.AsNoTracking()
            .Where(d => d.WorkspaceId == WorkspaceId)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        var users = await LoadUsersAsync(ct);
        var since = Clock.UtcNow.AddDays(-30);
        var rows = new List<DepartmentRowDto>();

        foreach (var department in departments)
        {
            var projects = await Db.Projects.AsNoTracking()
                .Where(p => p.DepartmentId == department.Id && p.WorkspaceId == WorkspaceId)
                .Include(p => p.Collaborators)
                .ToListAsync(ct);

            var projectIds = projects.Select(p => p.Id).ToList();
            var tasks = await Db.Tasks.AsNoTracking()
                .Where(t => t.ProjectId != null && projectIds.Contains(t.ProjectId.Value))
                .ToListAsync(ct);

            var completed = tasks.Where(t => t.Status == WorkTaskStatus.Completed && t.ApprovedAt != null && t.CompletedAt >= since).ToList();
            var awaiting = tasks.Count(t => t.Status == WorkTaskStatus.Completed && t.ApprovedAt == null);

            var memberIds = projects.Select(p => p.LeadUserId)
                .Concat(projects.SelectMany(p => p.Collaborators.Select(c => (Guid?)c.UserId)))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Append(department.LeadUserId ?? Guid.Empty)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            rows.Add(new DepartmentRowDto(
                department.Name,
                users.TryGetValue(department.LeadUserId ?? Guid.Empty, out var lead) ? lead.FullName : null,
                projects.Count(p => p.Status is not (ProjectStatus.Completed or ProjectStatus.Archived)),
                memberIds.Count,
                completed.Count,
                awaiting,
                completed.Sum(t => t.EstimateHours)));
        }

        return rows;
    }

    private IReadOnlyList<SelectOption> BuildSprintOptions(IEnumerable<Domain.Entities.Sprint> sprints, string? selected)
    {
        var options = new List<SelectOption>
        {
            new("active", _t["Active sprint"], string.IsNullOrEmpty(selected) || selected == "active"),
            new("backlog", _t["Backlog"], selected == "backlog")
        };

        options.AddRange(sprints.Select(s => new SelectOption(s.Id.ToString(), s.Name, selected == s.Id.ToString())));
        return options;
    }

    private byte[] BuildCsv(HoursReportDto hours)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteField(_t["Task"]);
        csv.WriteField(_t["Owner"]);
        csv.WriteField(_t["Tag"]);
        csv.WriteField(_t["Task time (h)"]);
        csv.WriteField(_t["Logged"]);
        csv.WriteField(_t["Variance"]);
        csv.NextRecord();

        foreach (var row in hours.Rows)
        {
            csv.WriteField(row.Task);
            csv.WriteField(row.Owner?.FullName ?? _t["Unassigned"]);
            csv.WriteField(row.Tag ?? string.Empty);
            csv.WriteField(row.TaskHours.ToString("0.##", CultureInfo.InvariantCulture));
            csv.WriteField(DurationFormatter.Format(row.Logged));
            csv.WriteField(row.HasVariance ? FormatVariance(row.Variance) : string.Empty);
            csv.NextRecord();
        }

        csv.WriteField(_t["Total"]);
        csv.WriteField(string.Empty);
        csv.WriteField(string.Empty);
        csv.WriteField(hours.TotalTaskHours.ToString("0.##", CultureInfo.InvariantCulture));
        csv.WriteField(DurationFormatter.Format(hours.TotalLogged));
        csv.WriteField(FormatVariance(hours.TotalVariance));
        csv.NextRecord();

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(writer.ToString())).ToArray();
    }

    private byte[] BuildDepartmentCsv(IReadOnlyList<DepartmentRowDto> departments)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteField(_t["Department"]);
        csv.WriteField(_t["Lead"]);
        csv.WriteField(_t["Active projects"]);
        csv.WriteField(_t["Members"]);
        csv.WriteField(_t["Completed tasks"]);
        csv.WriteField(_t["Awaiting approval"]);
        csv.WriteField(_t["Hours"]);
        csv.NextRecord();

        foreach (var row in departments)
        {
            csv.WriteField(row.Department);
            csv.WriteField(row.LeadName ?? string.Empty);
            csv.WriteField(row.ActiveProjects);
            csv.WriteField(row.Members);
            csv.WriteField(row.CompletedTasks);
            csv.WriteField(row.AwaitingApproval);
            csv.WriteField(row.Hours.ToString("0.0", CultureInfo.InvariantCulture));
            csv.NextRecord();
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(writer.ToString())).ToArray();
    }

    private byte[] BuildXlsx(ReportPageDto page)
    {
        using var workbook = new XLWorkbook();
        var hours = workbook.Worksheets.Add(_t["Hours report"]);
        hours.Cell(1, 1).Value = _t["Task"];
        hours.Cell(1, 2).Value = _t["Owner"];
        hours.Cell(1, 3).Value = _t["Tag"];
        hours.Cell(1, 4).Value = _t["Task time"];
        hours.Cell(1, 5).Value = _t["Logged"];
        hours.Cell(1, 6).Value = _t["Variance"];

        var row = 2;
        foreach (var item in page.Hours.Rows)
        {
            hours.Cell(row, 1).Value = item.Task;
            hours.Cell(row, 2).Value = item.Owner?.FullName ?? _t["Unassigned"];
            hours.Cell(row, 3).Value = item.Tag ?? string.Empty;
            hours.Cell(row, 4).Value = DurationFormatter.Format(item.TaskHours);
            hours.Cell(row, 5).Value = DurationFormatter.Format(item.Logged);
            hours.Cell(row, 6).Value = item.HasVariance ? FormatVariance(item.Variance) : "—";
            row++;
        }

        hours.Cell(row, 1).Value = _t["Total"];
        hours.Cell(row, 4).Value = DurationFormatter.Format(page.Hours.TotalTaskHours);
        hours.Cell(row, 5).Value = DurationFormatter.Format(page.Hours.TotalLogged);
        hours.Cell(row, 6).Value = FormatVariance(page.Hours.TotalVariance);
        hours.Row(1).Style.Font.Bold = true;
        hours.Columns().AdjustToContents();

        var department = workbook.Worksheets.Add(_t["By department"]);
        var headers = new[]
        {
            _t["Department"], _t["Lead"], _t["Active projects"], _t["Members"],
            _t["Completed tasks"], _t["Awaiting approval"], _t["Hours"]
        };
        for (var i = 0; i < headers.Length; i++)
        {
            department.Cell(1, i + 1).Value = headers[i];
        }

        row = 2;
        foreach (var item in page.Departments)
        {
            department.Cell(row, 1).Value = item.Department;
            department.Cell(row, 2).Value = item.LeadName ?? string.Empty;
            department.Cell(row, 3).Value = item.ActiveProjects;
            department.Cell(row, 4).Value = item.Members;
            department.Cell(row, 5).Value = item.CompletedTasks;
            department.Cell(row, 6).Value = item.AwaitingApproval;
            department.Cell(row, 7).Value = item.Hours;
            row++;
        }

        department.Row(1).Style.Font.Bold = true;
        department.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private byte[] BuildPdf(ReportPageDto page)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(pdf =>
            {
                pdf.Size(PageSizes.A4);
                pdf.Margin(1.8f, Unit.Centimetre);
                pdf.DefaultTextStyle(style => style.FontSize(10).FontColor(Colors.Grey.Darken3));

                pdf.Header().Column(header =>
                {
                    header.Item().Text(_t["Plexa · Hours report"]).FontSize(18).SemiBold().FontColor(Colors.Teal.Darken3);
                    header.Item().Text(page.Hours.FooterNote).FontSize(9).FontColor(Colors.Grey.Medium);
                    header.Item().PaddingBottom(8);
                });

                pdf.Content().Column(content =>
                {
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            foreach (var title in new[] { _t["Task"], _t["Owner"], _t["Tag"], _t["Task time"], _t["Logged"], _t["Variance"] })
                            {
                                header.Cell().Background(Colors.Teal.Lighten4).Padding(4).Text(title).SemiBold();
                            }
                        });

                        foreach (var row in page.Hours.Rows)
                        {
                            table.Cell().Padding(4).Text(row.Task);
                            table.Cell().Padding(4).Text(row.Owner?.FullName ?? _t["Unassigned"]);
                            table.Cell().Padding(4).Text(row.Tag ?? "—");
                            table.Cell().Padding(4).Text(DurationFormatter.Format(row.TaskHours));
                            table.Cell().Padding(4).Text(DurationFormatter.Format(row.Logged));
                            table.Cell().Padding(4).Text(row.HasVariance ? FormatVariance(row.Variance) : "—");
                        }

                        table.Cell().Padding(4).Text(_t["Total"]).SemiBold();
                        table.Cell().Padding(4).Text(string.Empty);
                        table.Cell().Padding(4).Text(string.Empty);
                        table.Cell().Padding(4).Text(DurationFormatter.Format(page.Hours.TotalTaskHours)).SemiBold();
                        table.Cell().Padding(4).Text(DurationFormatter.Format(page.Hours.TotalLogged)).SemiBold();
                        table.Cell().Padding(4).Text(FormatVariance(page.Hours.TotalVariance)).SemiBold();
                    });

                    content.Item().PaddingTop(18).Text(_t["By department (last 30 days)"]).FontSize(13).SemiBold();
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1f);
                        });

                        table.Header(header =>
                        {
                            foreach (var title in new[] { _t["Department"], _t["Active projects"], _t["Members"], _t["Completed"], _t["Awaiting"], _t["Hours"] })
                            {
                                header.Cell().Background(Colors.Teal.Lighten4).Padding(4).Text(title).SemiBold();
                            }
                        });

                        foreach (var row in page.Departments)
                        {
                            table.Cell().Padding(4).Text(row.LeadName is null ? row.Department : _t.Format("{0} (Lead: {1})", row.Department, row.LeadName));
                            table.Cell().Padding(4).Text(row.ActiveProjects.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Padding(4).Text(row.Members.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Padding(4).Text(row.CompletedTasks.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Padding(4).Text(row.AwaitingApproval.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Padding(4).Text(row.Hours.ToString("0.0", CultureInfo.InvariantCulture));
                        }
                    });
                });

                pdf.Footer().AlignRight().Text(text =>
                {
                    text.Span(_t["Plexa · generated "]);
                    text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture));
                });
            });
        }).GeneratePdf();
    }

    private static string FormatVariance(TimeSpan variance)
    {
        var sign = variance < TimeSpan.Zero ? "-" : "+";
        return sign + DurationFormatter.Format(variance.Duration());
    }
}
