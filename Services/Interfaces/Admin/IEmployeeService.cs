using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IEmployeeService
    {
        ServiceResult<PagedResultDto<EmployeeDto>> GetEmployeesPaged(int page, int pageSize);
        ServiceResult<EmployeeDto> GetEmployeeById(int id);
        ServiceResult CreateEmployee(EmployeeCreateDto dto);
        ServiceResult UpdateEmployee(EmployeeUpdateDto dto);
        ServiceResult DeleteEmployee(int id);
        ServiceResult ToggleEmployeeStatus(int id);
    }
}
