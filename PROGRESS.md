# PROGRESS — Plexa

> Status of the functional clone described in `spec/`. Last verification: 2026-11-04.

| Phase | Prompt | Status | Notes |
|---|---|---|---|
| 00 | .NET scaffold and infrastructure | ✅ | 4-layer solution + 3 test projects, `Directory.Build.props` with `TreatWarningsAsErrors`, Tailwind v4, Dockerfile and compose (only `web`) |
| 01 | Data model (EF Core), migrations and seed | ✅ | Entities + configurations, `InitialCreate` applied, idempotent seed with 1 workspace, 3 users, 1 project, 14 tasks, 1 time entry and 4 notifications |
| 02 | Authentication, invitations and roles | ✅ | Identity with the `plexa.auth` cookie, `Admin`/`Member` roles, invitations, password recovery, project-membership policies |
| 03 | Razor shell (sidebar, topbar, i18n, components) | ✅ | Layout, sidebar/topbar, toasts, `<dialog>` modals, shortcuts (`/`, `N`, `Esc`), EN/ES `IStringLocalizer` with every EN key present in ES |
| 04 | Task services + task board | ✅ | 4 columns, combinable filters, search including subtasks, status drag, approval, `HX-Trigger` |
| 05 | Create/edit task and detail | ✅ | htmx modal with inline validation, two-level subtasks, checklist, dependencies with cycle detection and blocking |
| 06 | Timer and time tracking | ✅ | One timer per user (filtered unique index), start/stop, manual entries, daily history |
| 07 | Dashboard | ✅ | KPIs, status/priority cards with htmx skeletons, activity, personal time, due-date warnings |
| 08 | Projects (list + 7 tabs + documents) | ✅ | Sequential `PRY-NN` code with `WorkspaceCounter`, tabs, team, documents via `IFileStorage`, download through `/media/{id}` |
| 09 | Team workload and capacity | ✅ | Weekly capacity, open hours, `On track`/`At risk`/`Overloaded`/`Available` signals |
| 10 | Reports and exports | ✅ | Estimated vs logged time, filters (sprint, member, tag, time, variance) and CSV/XLSX/PDF export |
| 11 | Analytics | ✅ | Throughput, cycle time, rework, delivery/plan, reliability, weekly cumulative flow |
| 12 | Calendar | ✅ | Month view, event types, create/detail, JSON at `/api/v1/calendar/events` |
| 13 | Messages and notifications | ✅ | Project/team threads, mentions, notification panel, JSON at `/api/v1/notifications` |
| 14 | Settings, SMTP mailbox and administration | ✅ | Profile, avatar, language, mailbox with encrypted password, member/project/sprint admin |
| 15 | Hardening, E2E and final audit | ✅ | 403/404/500 pages, `ProblemDetails`, Serilog, health check, 54 unit + 12 integration + 3 E2E (skippable), Docker |

Stack: .NET 9 MVC + Razor + Tailwind v4 + htmx/Alpine · EF Core/SQL Server · Identity

Legend: ⬜ pending · 🟡 in progress · ✅ done and verified · ⚠️ done with debt (note which)

## Verification (2026-11-04)

| Check | Result |
|---|---|
| `dotnet build Plexa.sln` | 0 warnings, 0 errors |
| `dotnet test Plexa.sln` | 54 unit + 12 integration + 3 E2E = **69 passing** |
| Tailwind (`tools/tailwindcss.exe`) | compiles, `wwwroot/css/site.css` ≈ 52 KB |
| `.resx` EN/ES | 480 EN keys, 665 ES; **0 EN keys without a translation** |
| UI and functional QA (Playwright against the running app) | **229 checks PASS, 0 FAIL, 0 WARN** |
| Main routes with a Member session | `/`, `/dashboard`, `/board`, `/projects`, `/team`, `/reports`, `/analytics`, `/calendar`, `/messages`, `/settings`, `/health` → 200 |
| `/admin/*` area with a Member | 403 |
| Responsive 1280 / 768 / 390 px | no horizontal overflow on member, admin, ES or modals |
| Acceptance figures | board `14 tasks`; dashboard `13 tasks in the backlog`, `3 / 13`, `23% of sprint`; analytics `13 tracked`, `77/0/0/23`, `28.0d`; reports `42h 00m · 1m · -41h 59m` |

### UI and functional QA (2026-11-04)

Automated Playwright walkthrough of the running app, with seed data and in EN/ES: anonymous and authenticated routes,
board (creation, column change, checklist, timer), search, project tabs, CSV/XLSX/PDF exports, analytics empty states,
calendar, messages (member and admin), settings, notifications, shortcuts, 403/404, ES audit and responsive at 768/390 px.
Result **229 PASS / 0 FAIL / 0 WARN**.

Defects found and fixed:

1. `Htmx.Toast` overwrote the existing `HX-Trigger`; it now merges events.
2. `_AuthLayout` did not load Alpine or `app.js`, and used a decorative `<h1>` for the logo (now `<p>`).
3. Wrong script order; unified to `htmx → chart → app.js → alpine` in both layouts.
4. Invalid SVG with `<template x-if>` inside a `<path>`; replaced with `<path x-show>`.
5. The `_TaskCard` move form fired with `change from:select`; now `hx-trigger="change"`.
6. The topbar live search was not wired; it now queries `/search/suggest` with `_SearchSuggestions` and `[x-cloak]`.
7. `/error/{code:int}` only accepted GET (405 on POST); it now uses `[Route]`.
8. **Culture cookie**: it was written as plain `es` and `CookieRequestCultureProvider` ignored it; it now uses
   `MakeCookieValue("es|uic=es")`. Regression covered by `Culture_Switch_PersistsSpanishCulture`.
9. `/messages/{id}` returned a partial on full navigation; it now returns the `Index` view unless htmx targets `thread-panel`.
10. Creating a channel did not navigate to the new thread; it now responds with `HX-Redirect` to `/messages/{id}?created=1` and an inline banner.
11. Horizontal overflow in tables and messages at 390 px; `[contain:paint]` on table wrappers, `min-w-0` on grid/thread/composer and `break-words` on bubbles.
12. Hardcoded placeholders (`user@company.com`, `smtp.office365.com`) moved to EN/ES `.resx`.
13. Board, projects, timer, calendar, settings and administration toasts and `TempData` notices were fixed in English;
    they now go through `ITranslator` and there is an ES regression test (`Settings_SuccessMessage_IsLocalized_InSpanish`).
14. The tag option list was hardcoded in Spanish (`Base de datos`, `Diseño`, `Investigación`, `Rendimiento`); it is now
    English (`Database`, `Design`, `Research`, `Performance`) to match the English-default UI.

Demo data: the workspace is called **Plexa Studio** and the seed project **Internal Training Platform** (the UI never uses the
original app's name). The development database is clean of QA artifacts after the audit: 14 tasks, 1 event, 0 sprints,
1 project, 0 threads, 1 time entry and 4 seed notifications.

## Decisions and assumptions

- **[ASSUMPTION] `net9.0`**: the environment only has SDK 9.0.102, so the solution compiles for `net9.0` instead of `net10.0`.
  Changing `TargetFramework` is enough once the .NET 10 SDK is available.
- **[ASSUMPTION] Standalone Tailwind CLI**: Node 18 does not support `@tailwindcss/cli` 4.x, so `tools/tailwindcss.exe`
  is used (v4.1.11, outside git). `npm run build:css` works with Node 20+.
- **[ASSUMPTION] SQL Server 2012 (compat 110)**: `UseCompatibilityLevel(110)` to avoid `OPENJSON` in `Contains`;
  `STRING_AGG`, `TRIM` and `CONCAT_WS` are forbidden in hand-written SQL.
- **[ASSUMPTION] `Thread` → `ChatThread`**: the entity was renamed to avoid clashing with `System.Threading.Thread` (DbSet `ChatThreads`).
- **[ASSUMPTION] `InBacklog`**: marks backlog tasks. The board counts all 14 nodes (3 parents + 11 subtasks) while the
  dashboard/analytics count 13 tracked items (the `Backend núcleo [1]` parent falls outside the backlog).
  This reproduces the spec's `14 tasks` and `13 tracked tasks` at the same time.
- **[ASSUMPTION] Seed dates**: completed tasks use `CompletedAt = ApprovedAt = CreatedAt + 28 days` so the average
  cycle time is `28.0d` and the comparison shows `+0.0d` when there is no previous period.
- **[ASSUMPTION] `--seed`**: `dotnet run --project src/Plexa.Web -- --seed` migrates, seeds and exits without starting the server.
- **[ASSUMPTION] Migrations in other environments**: in `Development` the app migrates and seeds on startup; elsewhere you must
  enable `Database__MigrateOnStartup` / `Database__SeedOnStartup` (Docker enables the former).
- **[ASSUMPTION] Data Protection keys**: persisted in `storage/keys` (configurable with `DataProtection__KeysPath`) so that
  cookies and encrypted SMTP passwords survive restarts.
- **[ASSUMPTION] Administration**: the spec does not show the member/project/sprint creation screens or billing;
  they were implemented reasonably under `/admin` with the `AdminOnly` policy.
- **[ASSUMPTION] Skippable tests**: integration tests require `Tests__ConnectionString` (temporary `Plexa_Test_<guid>` database);
  E2E requires `E2E__BaseUrl`. Without those variables the tests pass without running, so `dotnet test` works locally/CI.
- **[ASSUMPTION] Time zone**: everything is stored in UTC and displayed in the server's local zone through `IClock`.
- **[ASSUMPTION] Persisted text**: activity and notifications are written in English in the database (the default language)
  so the history does not change when switching EN/ES; the UI that displays them is localized.
- **[ASSUMPTION] Seed names**: workspace **Plexa Studio** and project **Internal Training Platform**, to avoid reusing the
  original app's identity. `spec/` is kept as the reference manual (now in English, with its original data and proper
  names preserved).
- **Brand**: the product is **Plexa** in the UI, emails, titles and metadata; support `support@plexa.app`.
