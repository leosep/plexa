using System.Net;
using FluentAssertions;

namespace Plexa.IntegrationTests;

/// <summary>
/// Pruebas de integración de las rutas y de las cifras de aceptación de la spec.
/// Se omiten (verde) cuando no hay <c>Tests__ConnectionString</c> configurada.
/// </summary>
public class WebAppTests : IClassFixture<PlexaWebFactory>
{
    private readonly PlexaWebFactory _factory;

    public WebAppTests(PlexaWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_IsRedirectedToLogin()
    {
        if (!_factory.Enabled) return;

        using var client = _factory.CreateAnonymousClient();
        var response = await client.GetAsync("/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().ContainEquivalentOf("/Account/Login");
    }

    [Fact]
    public async Task Health_Endpoint_IsHealthy()
    {
        if (!_factory.Enabled) return;

        using var client = _factory.CreateAnonymousClient();
        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Healthy");
    }

    [Fact]
    public async Task UnknownRoute_Returns404Page()
    {
        if (!_factory.Enabled) return;

        using var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/this-route-does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Member_CanOpenEveryMainScreen()
    {
        if (!_factory.Enabled) return;

        using var client = await _factory.CreateAuthenticatedClientAsync(allowAutoRedirect: true);

        foreach (var path in new[]
                 {
                     "/", "/dashboard", "/board", "/projects", "/team",
                     "/reports", "/analytics", "/calendar", "/messages", "/settings"
                 })
        {
            var response = await client.GetAsync(path);
            response.StatusCode.Should().Be(HttpStatusCode.OK, $"{path} debe responder 200");
        }
    }

    [Fact]
    public async Task Member_CannotOpenAdminArea()
    {
        if (!_factory.Enabled) return;

        using var client = await _factory.CreateAuthenticatedClientAsync(allowAutoRedirect: true);
        var response = await client.GetAsync("/admin/members");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_CanOpenAdminArea()
    {
        if (!_factory.Enabled) return;

        using var client = await _factory.CreateAuthenticatedClientAsync("alex.rivera@dwn.com.do");

        foreach (var path in new[] { "/admin/members", "/admin/projects", "/admin/sprints" })
        {
            var response = await client.GetAsync(path);
            response.StatusCode.Should().Be(HttpStatusCode.OK, $"{path} debe responder 200 para Admin");
        }
    }

    [Fact]
    public async Task Culture_Switch_PersistsSpanishCulture()
    {
        if (!_factory.Enabled) return;

        using var client = _factory.CreateAnonymousClient();
        var loginHtml = await client.GetStringAsync("/login");
        var token = System.Text.RegularExpressions.Regex
            .Match(loginHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")
            .Groups[1].Value;

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["culture"] = "es",
            ["returnUrl"] = "/login",
            ["__RequestVerificationToken"] = token
        });
        var response = await client.PostAsync("/culture", form);
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var spanish = await client.GetStringAsync("/login");
        spanish.Should().Contain("lang=\"es\"");
        spanish.Should().Contain("Correo", "la cookie plexa.culture debe aplicar la cultura española");
    }

    [Fact]
    public async Task Reports_MatchSpecAcceptanceNumbers()
    {
        if (!_factory.Enabled) return;

        using var client = await _factory.CreateAuthenticatedClientAsync();
        var html = await client.GetStringAsync("/reports");

        html.Should().Contain("42h 00m");
        html.Should().Contain("-41h 59m");
        html.Should().Contain("Showing 12 tasks");
        html.Should().Contain("1 with time logged");
    }

    [Fact]
    public async Task Analytics_MatchesSpecAcceptanceNumbers()
    {
        if (!_factory.Enabled) return;

        using var client = await _factory.CreateAuthenticatedClientAsync();
        var html = await client.GetStringAsync("/analytics");

        html.Should().Contain("Share of the 13 tracked tasks by state, last 8 weeks");
        html.Should().Contain("77%");
        html.Should().Contain("23%");
        html.Should().Contain("28.0d");
        html.Should().Contain("0 tasks reopened this quarter");
    }

    [Fact]
    public async Task Dashboard_MatchesSpecAcceptanceNumbers()
    {
        if (!_factory.Enabled) return;

        using var client = await _factory.CreateAuthenticatedClientAsync();
        var html = await client.GetStringAsync("/dashboard");

        html.Should().Contain("13 tasks in the backlog");
        html.Should().Contain("3 / 13");
        html.Should().Contain("23% of sprint");
    }

    [Fact]
    public async Task Board_ShowsFourteenTasks()
    {
        if (!_factory.Enabled) return;

        using var client = await _factory.CreateAuthenticatedClientAsync();
        var html = await client.GetStringAsync("/board");

        html.Should().Contain("14 tasks");
    }
}
