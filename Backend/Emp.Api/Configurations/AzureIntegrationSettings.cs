namespace Emp.Api.Configurations;

public class AzureIntegrationSettings
{
    public required string PublishToServiceBusAzureServiceFnUrl { get; set; }               // This function will accept HTTP Post numbers, they will be published to service bus topic

    public required string ConsumeAzureAppServiceApiAppUrl { get; set; }                    // Consumes Azure Service App API - for Even numbers
    public required string ConsumeAzureContainerInstanceApiUrl { get; set; }                // Consumes Azure Container Instance API - for Odd numbers
}
