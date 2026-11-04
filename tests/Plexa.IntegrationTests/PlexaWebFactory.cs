using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;

namespace Plexa.IntegrationTests;

/// <summary>
/// Arranca la aplicación contra una base de datos temporal <c>Plexa_Test_&lt;guid&gt;</c>
/// en el servidor indicado por <c>Tests__ConnectionString</c>. La base se crea y se
/// elimina por la propia factoría; nunca toca la base de desarrollo.
/// </summary>
public sealed class PlexaWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string? _baseConnectionString =
        Environment.GetEnvironmentVariable("Tests__ConnectionString");

    private bool _databaseCreated;

    public PlexaWebFactory()
    {
        DatabaseName = "Plexa_Test_" + Guid.NewGuid().ToString("N")[..10];
    }

    public string DatabaseName { get; }

    public bool Enabled => !string.IsNullOrWhiteSpace(_baseConnectionString);

    public string? SkipReason => Enabled
        ? null
        : "Define Tests__ConnectionString para ejecutar las pruebas de integración.";

    public string ConnectionString { get; private set; } = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (!Enabled)
        {
            return;
        }

        var baseBuilder = new SqlConnectionStringBuilder(_baseConnectionString!)
        {
            InitialCatalog = "master"
        };

        using (var connection = new SqlConnection(baseBuilder.ConnectionString))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [{DatabaseName}]";
            command.ExecuteNonQuery();
        }

        _databaseCreated = true;

        ConnectionString = new SqlConnectionStringBuilder(_baseConnectionString)
        {
            InitialCatalog = DatabaseName
        }.ConnectionString;

        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
    }

    public HttpClient CreateAnonymousClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    public async Task<HttpClient> CreateAuthenticatedClientAsync(
        string email = "lsepulveda@dwn.com.do",
        bool allowAutoRedirect = false)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = allowAutoRedirect
        });

        var login = await client.GetAsync("/login");
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await login.Content.ReadAsStringAsync();
        var token = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;

        token.Should().NotBeNullOrWhiteSpace("el formulario de login debe incluir el token antiforgery");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = "Plexa123$",
            ["__RequestVerificationToken"] = token
        });

        var response = await client.PostAsync("/login", form);
        if (allowAutoRedirect)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.OriginalString.Should().StartWith("/dashboard");
        }

        return client;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();

        if (!_databaseCreated)
        {
            return;
        }

        SqlConnection.ClearAllPools();

        var master = new SqlConnectionStringBuilder(_baseConnectionString!)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        using var connection = new SqlConnection(master);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            $"IF DB_ID(N'{DatabaseName}') IS NOT NULL " +
            $"BEGIN ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{DatabaseName}]; END";
        command.ExecuteNonQuery();
    }
}
