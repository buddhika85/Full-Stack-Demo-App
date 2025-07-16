
using Emp.Api.Controllers;
using Emp.Core.DTOs;
using Emp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Emp.Api.Middleware;

public class EmployeeController : BaseController
{
    private readonly IEmployeeService employeeService;
    private readonly ILogger<EmployeeController> logger;
    private readonly IOutputCacheStore outputCacheStore;
    private const string cacheTag = "employees";

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger,
            IOutputCacheStore outputCacheStore)
    {
        this.employeeService = employeeService;
        this.logger = logger;
        this.outputCacheStore = outputCacheStore;
    }

    [EndpointSummary("Gets all employees")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    [HttpGet]
    [OutputCache(Tags = [cacheTag])]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        try
        {
            return Ok(await employeeService.GetAllEmployeesAsync());
        }
        catch (Exception ex)
        {
            const string error = "Error occured while retreiving employees";
            logger.LogError(ex, error);
            return InternalServerError(error);
        }
    }

    [EndpointSummary("Get employee by ID")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee([FromRoute] int id)
    {
        try
        {
            var employee = await employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                logger.LogWarning("Employee with ID {id} does not exist.", id);
                return NotFoundError($"Employee with ID {id} does not exist.");
            }
            return Ok(employee);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finding employee with ID {id}.", id);
            return InternalServerError($"Error finding employee with ID {id}.");
        }
    }

    [EndpointSummary("Create a new employee")]
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto createEmployeeDto)
    {
        try
        {
            var employee = await employeeService.CreateEmployeeAsync(createEmployeeDto);
            if (employee == null)
            {
                logger.LogError("Error creating an employee with email {email}.", createEmployeeDto.Email);
                return InternalServerError($"Error creating an employee with email {createEmployeeDto.Email}.");
            }
            await outputCacheStore.EvictByTagAsync(cacheTag, default);
            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating an employee with email {email}.", createEmployeeDto.Email);
            return InternalServerError($"Error creating an employee with email {createEmployeeDto.Email}.");
        }
    }

    [EndpointSummary("Updates an employee")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee([FromRoute] int id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
    {
        try
        {
            if (id != updateEmployeeDto.Id)
            {
                logger.LogError("Unable to update. Employee Id passed from route {id} does not match with body employee Id {dtoId}", id, updateEmployeeDto.Id);
                return ValidationError("id", $"Unable to update. Employee Id passed from route {id} does not match with body employee Id {updateEmployeeDto.Id}");
            }

            if (await employeeService.GetEmployeeByIdAsync(id) == null)
            {
                logger.LogError("Unable to update. Employee with ID {id} does not exist.", id);
                return NotFoundError($"Unable to update. Employee with ID {id} does not exist.");
            }

            if (!await employeeService.UpdateEmployeeAsync(id, updateEmployeeDto))
            {
                logger.LogError("Error occured while updating employee with ID {id} and email {email}", id, updateEmployeeDto.Email);
                return InternalServerError($"Error occured while updating employee with ID {id} and email {updateEmployeeDto.Email}");
            }

            await outputCacheStore.EvictByTagAsync(cacheTag, default);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in updating employee with ID {id} and email {email}", id, updateEmployeeDto.Email);
            return InternalServerError($"Error in updating employee with ID {id} and email {updateEmployeeDto.Email}");
        }
    }

    [EndpointSummary("Deletes an employee")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployees(int id)
    {
        try
        {
            if (await employeeService.GetEmployeeByIdAsync(id) == null)
            {
                logger.LogError("Unable to delete. Employee with ID {id} does not exist.", id);
                return NotFoundError($"Unable to delete. Employee with ID {id} does not exist.");
            }

            if (!await employeeService.DeleteEmployeeAsync(id))
            {
                logger.LogError("Error occured while deleting employee with ID {id}", id);
                return InternalServerError($"Error occured while deleting employee with ID {id}");
            }

            await outputCacheStore.EvictByTagAsync(cacheTag, default);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in deleting employee with ID {id}}", id);
            return InternalServerError($"Error in deleting employee with ID {id}");
        }
    }
}
