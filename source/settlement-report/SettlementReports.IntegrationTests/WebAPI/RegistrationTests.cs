using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Energinet.DataHub.Reports.WebAPI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.Reports.IntegrationTests.WebAPI;

public class RegistrationTests
{
    [Fact]
    public void All_dependencies_can_be_resolved_in_webAPI()
    {
        var testConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    // [$"{ServiceBusNamespaceOptions.SectionName}__{nameof(ServiceBusNamespaceOptions.FullyQualifiedNamespace)}"] = "Fake",
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.MitIdExternalMetadataAddress)}"] = "NotEmpty",
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.ExternalMetadataAddress)}"] = "NotEmpty",
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.BackendBffAppId)}"] = "NotEmpty",
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.InternalMetadataAddress)}"] = "NotEmpty",
                })
            .Build();

        using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                webBuilder =>
                {
                    webBuilder.UseConfiguration(testConfiguration);
                    webBuilder.UseDefaultServiceProvider(
                        (_, options) =>
                        {
                            // See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-7.0#scope-validation
                            options.ValidateScopes = true;
                            // Validate the service provider during build
                            options.ValidateOnBuild = true;
                        })
                        // Add controllers as services to enable validation of controller dependencies
                        // See https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/#1-controller-constructor-dependencies-aren-t-checked
                        .ConfigureServices(services =>
                        {
                            services.AddControllers().AddControllersAsServices();
                        });
                })
            .CreateClient(); // This will resolve the dependency injections, hence the test
    }
}
