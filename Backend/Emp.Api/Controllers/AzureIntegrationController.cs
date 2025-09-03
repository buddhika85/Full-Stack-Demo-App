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
    /// Then this azure functionpublishes the payload to a Azure Service Bus Topic
    /// </summary>
    /// <returns>AzPayloadReceivedDto</returns>     
    [EndpointName("CallPublishToAzureService_Fn")]
    [EndpointSummary("Calls an Azure Function with a payload. Then this azure functionpublishes the payload to a Azure Service Bus topic ODD EVEN.")]
    [ProducesResponseType(typeof(AzPayloadReceivedDto), StatusCodes.Status200OK)]
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


    /// <summary>
    /// Calls an Azure App Service API to retrieve a list of all EVEN numbers posted to Azure Service Bus topic ODD EVEN
    /// </summary>
    /// <returns>AzNumberListDto</returns>
    [EndpointName("GetAllFromAzureApiAppService")]
    [EndpointSummary("Calls an Azure App Service API to retrieve a list of all EVEN numbers posted to Azure Service Bus topic ODD EVEN")]
    [ProducesResponseType(typeof(AzNumberListDto), StatusCodes.Status200OK)]
    [HttpGet("GetAllFromAzureApiAppService")]
    public async Task<ActionResult<AzNumberListDto>> GetAllFromAzureApiAppService()
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("ExponetialBackOffForPost");

            var response = await httpClient.GetAsync(settings.ConsumeAzureAppServiceApiAppUrl);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Azure API app service call to get even numbers Failed. Status: {status}, Body: {body}", response.StatusCode, responseBody);
                return StatusCode((int)response.StatusCode, "Azure API app service call to get even numbers Failed");
            }

            var list = DeseializeJson(responseBody);


            return Ok(new AzNumberListDto
            {
                IsSuccess = true,
                Items = list
            });
        }
        catch (Exception ex)
        {
            const string error = "Exception occurred in API app service call to get even numbers";
            logger.LogError(ex, error);
            return InternalServerError(error);
        }
    }


    /// <summary>
    /// Calls an Azure Container Instance API to retrieve a list of all ODD numbers posted to Azure Service Bus topic ODD EVEN
    /// </summary>
    /// <returns>AzNumberListDto</returns>
    [EndpointName("GetAllFromAzureContainerInstanceApi")]
    [EndpointSummary("Calls an Azure Container Instance API to retrieve a list of all ODD numbers posted to Azure Service Bus topic ODD EVEN")]
    [ProducesResponseType(typeof(AzNumberListDto), StatusCodes.Status200OK)]
    [HttpGet("GetAllFromAzureContainerInstanceApi")]
    public async Task<ActionResult<AzNumberListDto>> GetAllFromAzureContainerInstanceApi()
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("ExponetialBackOffForPost");

            var response = await httpClient.GetAsync(settings.ConsumeAzureContainerInstanceApiUrl);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Azure Container Instance API app call to get odd numbers Failed. Status: {status}, Body: {body}", response.StatusCode, responseBody);
                return StatusCode((int)response.StatusCode, "Azure Container Instance API App call to get odd numbers Failed");
            }

            var list = DeseializeJson(responseBody);


            return Ok(new AzNumberListDto
            {
                IsSuccess = true,
                Items = list
            });
        }
        catch (Exception ex)
        {
            const string error = "Exception occurred in Azure Container Instance API app call to get odd numbers";
            logger.LogError(ex, error);
            return InternalServerError(error);
        }
    }

    private static IEnumerable<AzNumItemDto> DeseializeJson(string responseBody)
    {
        return JsonSerializer.Deserialize<IEnumerable<AzNumItemDto>>(
                            responseBody,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            })
            ?? Enumerable.Empty<AzNumItemDto>();
    }
}
