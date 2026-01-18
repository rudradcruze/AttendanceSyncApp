using System;
using System.Collections.Generic;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IAdminDatabaseConfigService
    {
        ServiceResult<PagedResultDto<DatabaseConfigDto>> GetAllConfigsPaged(int page, int pageSize);
        ServiceResult<DatabaseConfigDto> GetConfigById(int id);
        ServiceResult<string> GetDatabasePassword(int id);
        ServiceResult<string> CreateConfig(DatabaseConfigCreateDto dto);
        ServiceResult<string> UpdateConfig(DatabaseConfigUpdateDto dto);
        ServiceResult<string> DeleteConfig(int id);
        ServiceResult<List<CompanyDto>> GetAvailableCompanies();
    }
}
