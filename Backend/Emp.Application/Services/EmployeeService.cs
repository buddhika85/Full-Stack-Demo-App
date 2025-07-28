using Emp.Core;
using Emp.Core.DTOs;
using Emp.Core.Extensions;
using Emp.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Emp.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<EmployeeService> logger;

    public EmployeeService(IUnitOfWork unitOfWork, ILogger<EmployeeService> logger)
    {
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        logger.LogInformation("Attempting to get all employees");
        try
        {
            var entities = await unitOfWork.EmployeeRepository.GetAllAsync();
            logger.LogInformation("Successfully retrieved {Count} employees.", entities.Count());
            return entities.ToDtos();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all employees.");
            throw; // Re-throw to propagate the error up;
        }
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        logger.LogInformation("Atempting to get an employee with ID {id}", id);
        try
        {
            var entity = await unitOfWork.EmployeeRepository.GetByIdAsync(id);
            if (entity == null)
            {
                logger.LogWarning("Employee with id {id} unavailable", id);
                return null;
            }
            logger.LogInformation("Employee with id {id} retrieved", id);
            return entity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in retrieving an employee with id {id}", id);
            throw;
        }
    }

    public async Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto employeeDto)
    {
        logger.LogInformation("Attempting to create employee with email: {Email}", employeeDto.Email);
        try
        {
            var entity = employeeDto.ToEntity();
            await unitOfWork.EmployeeRepository.AddAsync(entity);
            if (await unitOfWork.CompleteAsync() > 0)
            {
                logger.LogInformation("Employee {FirstName} {LastName} (ID: {EmployeeId}) created successfully.", entity.FirstName, entity.LastName, entity.Id);

                if (entity.Department == null)
                {
                    entity.Department = await unitOfWork.DepartmentRepository.GetByIdAsync(entity.DepartmentId);
                }
                return entity.ToDto();
            }

            logger.LogError("Employee {FirstName} {LastName} (ID: {EmployeeId}) creation unsuccessful.", entity.FirstName, entity.LastName, entity.Id);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating employee with email: {Email}", employeeDto.Email);
            throw;
        }
    }

    public async Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto employeeDto)
    {
        logger.LogInformation("Atempting to update an employee with ID {id}", id);
        try
        {
            var entity = await unitOfWork.EmployeeRepository.GetByIdAsync(id);
            if (entity == null)
            {
                logger.LogWarning("Updated Failed: Employee with id {id} unavailable", id);
                return false;
            }
            employeeDto.MapToEntity(entity);
            unitOfWork.EmployeeRepository.Update(entity);
            if (await unitOfWork.CompleteAsync() > 0)
            {
                logger.LogInformation("Employee with ID {id} updated successfully.", id);
                return true;
            }

            logger.LogError("Employee with ID {id} update unsuccessful.", id);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in updating an employee with id {id}", id);
            throw;
        }
    }


    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        logger.LogInformation("Attempting to delete employee with ID: {EmployeeId}", id);
        try
        {
            var entity = await unitOfWork.EmployeeRepository.GetByIdAsync(id);
            if (entity == null)
            {
                logger.LogWarning("Delete failed: Employee with ID {EmployeeId} not found.", id);
                return false;
            }

            unitOfWork.EmployeeRepository.Delete(entity);
            await unitOfWork.CompleteAsync();
            logger.LogInformation("Employee with ID {EmployeeId} deleted successfully.", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting employee with ID: {EmployeeId}", id);
            throw;
        }
    }


}


