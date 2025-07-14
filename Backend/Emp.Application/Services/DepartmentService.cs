using Emp.Core;
using Emp.Core.DTOs;
using Emp.Core.Extensions;
using Emp.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Emp.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<DepartmentService> logger;

    public DepartmentService(IUnitOfWork unitOfWork, ILogger<DepartmentService> logger)
    {
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }


    public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
    {
        logger.LogInformation("Attempting to get all departments");
        try
        {
            var entities = await unitOfWork.DepartmentRepository.GetAllAsync();
            logger.LogInformation("Retrived {count} departments", entities.Count());
            return entities.ToDtos();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all departments.");
            throw;
        }
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
    {
        logger.LogInformation("Atempting to get a department with ID {id}", id);
        try
        {
            var entity = await unitOfWork.DepartmentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                logger.LogWarning("Department with id {id} unavailable", id);
                return null;
            }
            logger.LogInformation("Department with id {id} retrieved", id);
            return entity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in retrieving a department with id {id}", id);
            throw;
        }
    }

    public async Task<DepartmentDto?> CreateDepartmentAsync(CreateDepartmentDto departmentDto)
    {
        logger.LogInformation("Attempting to create department with name: {name}", departmentDto.Name);
        try
        {
            var entity = departmentDto.ToEntity();
            await unitOfWork.DepartmentRepository.AddAsync(entity);
            if (await unitOfWork.CompleteAsync() > 0)
            {
                logger.LogInformation("Department '{DepartmentName}' (ID: {DepartmentId}) created successfully.", entity.Name, entity.Id);
                return entity.ToDto();
            }

            logger.LogError("Department '{DepartmentName}' (ID: {DepartmentId}) creation unsuccessful.", entity.Name, entity.Id);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating department with name: {DepartmentName}", departmentDto.Name);
            throw;
        }
    }
    public async Task<bool> UpdateDepartmentAsync(int id, UpdateDepartmentDto departmentDto)
    {
        logger.LogInformation("Atempting to update an department with ID {id}", id);
        try
        {
            var entity = await unitOfWork.DepartmentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                logger.LogWarning("Updated Failed: Department with id {id} unavailable", id);
                return false;
            }

            departmentDto.MapToEntity(entity);
            unitOfWork.DepartmentRepository.Update(entity);
            if (await unitOfWork.CompleteAsync() > 0)
            {
                logger.LogInformation("Department with ID {id} updated successfully.", id);
                return true;
            }

            logger.LogError("Department with ID {id} update unsuccessful.", id);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in updating a department with id {id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        logger.LogInformation("Attempting to delete department with ID: {id}", id);
        try
        {
            var entity = await unitOfWork.DepartmentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                logger.LogWarning("Delete failed: Department with ID {id} not found.", id);
                return false;
            }

            unitOfWork.DepartmentRepository.Delete(entity);
            await unitOfWork.CompleteAsync();
            logger.LogInformation("Department with ID {id} deleted successfully.", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting department with ID: {id}", id);
            throw;
        }
    }
}
