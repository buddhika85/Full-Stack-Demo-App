using Emp.Api.Configurations;
using Emp.Core.DTOs.AzureIntegration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;


namespace Emp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AzureIntegrationController : BaseController
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<AzureIntegrationController> logger;
    private readonly AzureIntegrationSettings settings;


    public AzureIntegrationController(IHttpClientFactory httpClientFactory,
        IOptions<AzureIntegrationSettings> options,
        ILogger<AzureIntegrationController> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        settings = options.Value;
    }



    /// <summary>
    /// Calls an Azure Function with a payload
    /// This azure publishes the payload to a Azure Service Bus Topic
    /// </summary>
    /// <returns>AzPayloadReceivedDto</returns>  
    [HttpPost("CallPublishToAzureService_Fn")]
    public async Task<ActionResult<AzPayloadReceivedDto>> CallPublishToAzureServiceBusFn(AzPostToAzureFuncDto azPostToAzureFuncDto)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("ExponetialBackOffForPost");

            var json = JsonSerializer.Serialize(azPostToAzureFuncDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(settings.PublishToServiceBusAzureServiceFnUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                logger.LogError("Azure Function call failed. Status: {status}, Body: {body}", response.StatusCode, responseBody);
                return StatusCode((int)response.StatusCode, "Failed to post to Azure Function");
            }

            return Ok(new AzPayloadReceivedDto
            {
                IsSuccess = true,
                Message = $"{azPostToAzureFuncDto.Number} was posted Azure Function - PublishToAzureServiceBus"
            });
        }
        catch (Exception ex)
        {
            const string error = "Exception occurred in CallPublishToAzureServiceFn - May not have posted number to Azure Function - PublishToAzureService";
            logger.LogError(ex, error);
            return InternalServerError(error);
        }
    }
}
