using Emp.Core.DTOs;
using Emp.Core.Enums;
using Emp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;


namespace Emp.Api.Controllers;

[Authorize(Roles = $"{nameof(UserRoles.Admin)},{nameof(UserRoles.Staff)}")]
public class DepartmentController : BaseController
{
    private readonly IDepartmentService departmentService;
    private readonly ILogger<DepartmentController> logger;
    private readonly IOutputCacheStore outputCacheStore;
    private const string cacheTag = "departments";

    public DepartmentController(IDepartmentService departmentService, ILogger<DepartmentController> logger,
            IOutputCacheStore outputCacheStore)
    {
        this.departmentService = departmentService;
        this.logger = logger;
        this.outputCacheStore = outputCacheStore;
    }


    [EndpointSummary("Gets all departments")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    [HttpGet]
    [OutputCache(Tags = [cacheTag])]       // this is configured to keep 15 seconds in Program.cs
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
    {
        try
        {
            var result = await departmentService.GetAllDepartmentsAsync();
            logger.Log(LogLevel.Information, "API: GetDepartments endpoint called (cached).");
            return Ok(result);
        }
        catch (Exception ex)
        {
            const string error = "Error occured while retreiving departments";
            logger.LogError(ex, error);
            return InternalServerError(error);
        }
    }


    [EndpointSummary("Get department by ID")]
    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment([FromRoute] int id)
    {
        try
        {
            var department = await departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                logger.LogWarning("Department with ID {id} not found", id);
                return NotFoundError($"Department with ID {id} not found");
            }
            return Ok(department);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while retreiving a department by Id {id}", id);
            return InternalServerError($"An error occurred while retrieving a department by ID {id}.");
        }
    }


    [EndpointSummary("Creates a new department")]
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto createDepartmentDto)
    {
        try
        {
            var dto = await departmentService.CreateDepartmentAsync(createDepartmentDto);
            if (dto == null)
            {
                logger.LogError("Error occured while creating a department with name {name}", createDepartmentDto.Name);
                return InternalServerError($"Error occured while creating a department with name {createDepartmentDto.Name}");

            }

            await outputCacheStore.EvictByTagAsync(cacheTag, default);
            logger.Log(LogLevel.Information, $"API: CreateDepartment endpoint called (evicted cache on cache tag {cacheTag}).");
            return CreatedAtAction(nameof(GetDepartment), new { id = dto.Id }, dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while creating a department with name {name}", createDepartmentDto.Name);
            return InternalServerError($"Error occured while creating a department with name {createDepartmentDto.Name}");
        }
    }

    [EndpointSummary("Updates a department")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDepartment([FromRoute] int id, [FromBody] UpdateDepartmentDto updateDepartmentDto)
    {
        try
        {
            if (id != updateDepartmentDto.Id)
            {
                logger.LogWarning("UpdateDepartment BadRequest - ID mismatch. Route ID: {RouteId}, DTO ID: {DtoId}.", id, updateDepartmentDto.Id);
                return ValidationError("id", $"UpdateDepartment BadRequest - ID mismatch. Route ID: {id}, DTO ID: {updateDepartmentDto.Id}");
            }

            if (await departmentService.GetDepartmentByIdAsync(id) == null)
            {
                logger.LogWarning("Department with ID {id} not found for updates.", id);
                return NotFoundError($"Department with ID {id} not found for updates");
            }

            if (!await departmentService.UpdateDepartmentAsync(id, updateDepartmentDto))
            {
                logger.LogError("Error occured while updating a department with id {id}", id);
                return InternalServerError($"Error occured while updating a department with id {id}");
            }
            await outputCacheStore.EvictByTagAsync(cacheTag, default);
            logger.Log(LogLevel.Information, $"API: UpdateDepartment endpoint called (evicted cache on cache tag {cacheTag}).");
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while updating a department with id {id}", id);
            return InternalServerError($"Error occured while updating a department with id {id}");
        }
    }

    [EndpointSummary("Deletes a department by ID")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDepartment([FromRoute] int id)
    {
        try
        {
            if (await departmentService.GetDepartmentByIdAsync(id) == null)
            {
                logger.LogWarning("Department with ID {id} not found for deleting.", id);
                return NotFoundError($"Department with ID {id} not found for deleting");
            }

            if (!await departmentService.DeleteDepartmentAsync(id))
            {
                logger.LogWarning("Delete failed: Department with ID {Id} has associated employees.", id);
                return ConflictError("Cannot delete department as it has associated employees");
            }

            await outputCacheStore.EvictByTagAsync(cacheTag, default);
            logger.Log(LogLevel.Information, $"API: DeleteDepartment endpoint called (evicted cache on cache tag {cacheTag}).");
            return NoContent();     // Ok($"Department with id {id} deleted.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while deleting a department with id {id}", id);
            return InternalServerError($"Error occured while deleting a department with id {id}");
        }
    }
}
