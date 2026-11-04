using Microsoft.Playwright;
using Xunit;

namespace Plexa.E2E;

/// <summary>
/// Pruebas E2E con Playwright .NET. Requieren una instancia de Plexa en ejecución
/// y la variable <c>E2E__BaseUrl</c> (por ejemplo <c>http://localhost:5199</c>).
/// Sin esa variable las pruebas se omiten para no romper <c>dotnet test</c> en local/CI.
/// </summary>
public sealed class PlaywrightSmokeTests : IAsyncLifetime
{
    private const string DemoEmail = "lsepulveda@dwn.com.do";
    private const string DemoPassword = "Plexa123$";

    private readonly string? _baseUrl = Environment.GetEnvironmentVariable("E2E__BaseUrl");
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public bool Enabled => !string.IsNullOrWhiteSpace(_baseUrl);

    public async Task InitializeAsync()
    {
        if (!Enabled)
        {
            return;
        }

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
    }

    [Fact]
    public async Task Login_LeadsToDashboard()
    {
        if (!Enabled)
        {
            return;
        }

        var page = await NewPageAsync();
        await LoginAsync(page);

        await page.WaitForURLAsync("**/dashboard");
        Assert.Contains("Dashboard", await page.TitleAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task Board_ShowsFourColumnsAndSpecTotal()
    {
        if (!Enabled)
        {
            return;
        }

        var page = await NewPageAsync();
        await LoginAsync(page);

        await page.GotoAsync($"{_baseUrl}/board");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var columns = page.Locator("#board-results > section");
        Assert.Equal(4, await columns.CountAsync());
        Assert.Contains("14 tasks", await page.Locator("header p").First.InnerTextAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task NewTaskModal_OpensAndClosesWithEscape()
    {
        if (!Enabled)
        {
            return;
        }

        var page = await NewPageAsync();
        await LoginAsync(page);

        await page.GotoAsync($"{_baseUrl}/board");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "New task" }).First.ClickAsync();
        await page.Locator("#modal-root dialog").WaitForAsync();

        await page.Keyboard.PressAsync("Escape");
        await page.Locator("#modal-root dialog").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Detached
        });

        await page.CloseAsync();
    }

    private async Task<IPage> NewPageAsync()
    {
        Assert.NotNull(_browser);
        var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = _baseUrl,
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
        });

        return await context.NewPageAsync();
    }

    private async Task LoginAsync(IPage page)
    {
        await page.GotoAsync("/login");
        await page.GetByLabel("Email").FillAsync(DemoEmail);
        await page.GetByLabel("Password").FillAsync(DemoPassword);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in" }).ClickAsync();
    }
}
