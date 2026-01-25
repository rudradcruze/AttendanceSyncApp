using System;
using System.Collections.Generic;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;

namespace AttandanceSyncApp.Services.Interfaces.AttandanceSync
{
    public interface IDynamicDatabaseService
    {
        ServiceResult<bool> TestConnection(DatabaseConnectionDto config);
        ServiceResult<IEnumerable<AttandanceSynchronization>> GetAttendanceData(int requestId, DateTime? fromDate, DateTime? toDate);
        string BuildConnectionString(DatabaseConnectionDto config);
    }
}
