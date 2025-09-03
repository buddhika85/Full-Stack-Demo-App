
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace Emp.Core.DTOs.AzureIntegration;

public class AzNumItemDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }


    [JsonPropertyName("number")]
    [Required]
    public int Number { get; set; }


    [JsonPropertyName("createdTime")]
    public string? CreatedTime { get; set; }


    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
