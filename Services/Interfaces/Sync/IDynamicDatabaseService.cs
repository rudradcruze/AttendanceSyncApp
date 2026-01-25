using System;
using System.Collections.Generic;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Sync;

namespace AttandanceSyncApp.Services.Interfaces.Sync
{
    public interface IDynamicDatabaseService
    {
        ServiceResult<bool> TestConnection(DatabaseConnectionDto config);
        ServiceResult<IEnumerable<AttandanceSynchronization>> GetAttendanceData(int requestId, DateTime? fromDate, DateTime? toDate);
        string BuildConnectionString(DatabaseConnectionDto config);
    }
}
