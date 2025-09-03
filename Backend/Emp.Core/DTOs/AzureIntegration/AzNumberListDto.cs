namespace Emp.Core.DTOs.AzureIntegration;

public class AzNumberListDto
{
    public IEnumerable<AzNumItemDto>? Items { get; set; }
    public bool IsSuccess { get; set; }
}
