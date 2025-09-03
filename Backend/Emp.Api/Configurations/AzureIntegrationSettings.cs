namespace Emp.Api.Configurations;

public class AzureIntegrationSettings
{
    public required string PublishToServiceBusAzureServiceFnUrl { get; set; }               // This function will accept HTTP Post numbers, they will be published to service bus topic
}
