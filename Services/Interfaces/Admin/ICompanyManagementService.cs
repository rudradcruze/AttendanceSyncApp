using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface ICompanyManagementService
    {
        ServiceResult<PagedResultDto<CompanyManagementDto>> GetCompaniesPaged(int page, int pageSize);
        ServiceResult<CompanyManagementDto> GetCompanyById(int id);
        ServiceResult CreateCompany(CompanyCreateDto dto);
        ServiceResult UpdateCompany(CompanyUpdateDto dto);
        ServiceResult DeleteCompany(int id);
        ServiceResult ToggleCompanyStatus(int id);
    }
}
