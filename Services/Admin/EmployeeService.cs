using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    /// <summary>
    /// Service for managing employee records from an administrative perspective.
    /// Handles CRUD operations for employees and status management.
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new EmployeeService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public EmployeeService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all employees with pagination support.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of employees with their details.</returns>
        public ServiceResult<PagedResultDto<EmployeeDto>> GetEmployeesPaged(int page, int pageSize)
        {
            try
            {
                // Get total count for pagination
                var totalCount = _unitOfWork.Employees.Count();
                var employees = _unitOfWork.Employees.GetAll()
                    .OrderByDescending(e => e.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        IsActive = e.IsActive,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    })
                    .ToList();

                var result = new PagedResultDto<EmployeeDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = employees
                };

                return ServiceResult<PagedResultDto<EmployeeDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<EmployeeDto>>.FailureResult($"Failed to retrieve employees: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific employee by ID.
        /// </summary>
        /// <param name="id">The employee ID.</param>
        /// <returns>Employee details including name and status.</returns>
        public ServiceResult<EmployeeDto> GetEmployeeById(int id)
        {
            try
            {
                // Fetch employee by ID
                var employee = _unitOfWork.Employees.GetById(id);
                if (employee == null)
                {
                    return ServiceResult<EmployeeDto>.FailureResult("Employee not found");
                }

                var dto = new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    IsActive = employee.IsActive,
                    CreatedAt = employee.CreatedAt,
                    UpdatedAt = employee.UpdatedAt
                };

                return ServiceResult<EmployeeDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<EmployeeDto>.FailureResult($"Failed to retrieve employee: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new employee record.
        /// </summary>
        /// <param name="dto">The employee data to create.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult CreateEmployee(EmployeeCreateDto dto)
        {
            try
            {
                // Validate employee name
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult.FailureResult("Employee name is required");
                }

                var employee = new Employee
                {
                    Name = dto.Name.Trim(),
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.Employees.Add(employee);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Employee created");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to create employee: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing employee's information.
        /// </summary>
        /// <param name="dto">The updated employee data.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult UpdateEmployee(EmployeeUpdateDto dto)
        {
            try
            {
                // Retrieve existing employee
                var employee = _unitOfWork.Employees.GetById(dto.Id);
                if (employee == null)
                {
                    return ServiceResult.FailureResult("Employee not found");
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult.FailureResult("Employee name is required");
                }

                employee.Name = dto.Name.Trim();
                employee.IsActive = dto.IsActive;
                employee.UpdatedAt = DateTime.Now;

                _unitOfWork.Employees.Update(employee);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Employee updated");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to update employee: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an employee record if they have no associated requests.
        /// </summary>
        /// <param name="id">The employee ID to delete.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult DeleteEmployee(int id)
        {
            try
            {
                // Retrieve employee to delete
                var employee = _unitOfWork.Employees.GetById(id);
                if (employee == null)
                {
                    return ServiceResult.FailureResult("Employee not found");
                }

                // Check if employee has sync requests
                var hasRequests = _unitOfWork.AttandanceSyncRequests.Count(r => r.EmployeeId == id) > 0;
                if (hasRequests)
                {
                    return ServiceResult.FailureResult("Cannot delete employee with existing requests");
                }

                _unitOfWork.Employees.Remove(employee);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Employee deleted");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to delete employee: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles the status of an employee between active and inactive.
        /// </summary>
        /// <param name="id">The employee ID to toggle.</param>
        /// <returns>Success or failure result with new status.</returns>
        public ServiceResult ToggleEmployeeStatus(int id)
        {
            try
            {
                // Retrieve the employee
                var employee = _unitOfWork.Employees.GetById(id);
                if (employee == null)
                {
                    return ServiceResult.FailureResult("Employee not found");
                }

                employee.IsActive = !employee.IsActive;
                employee.UpdatedAt = DateTime.Now;

                _unitOfWork.Employees.Update(employee);
                _unitOfWork.SaveChanges();

                var status = employee.IsActive ? "activated" : "deactivated";
                return ServiceResult.SuccessResult($"Employee {status}");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to toggle employee status: {ex.Message}");
            }
        }
    }
}
