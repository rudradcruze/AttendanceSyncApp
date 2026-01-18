/* ============================
   Admin Dashboard
============================ */
$(function () {
    loadStats();
    loadRecentRequests();
    loadRecentUsers();
});

function loadStats() {
    $.get(APP.baseUrl + 'AdminDashboard/GetStats', function (res) {
        if (res.Data) {
            $('#totalUsers').text(res.Data.TotalUsers);
            $('#totalRequests').text(res.Data.TotalRequests);
            $('#pendingRequests').text(res.Data.PendingRequests);
            $('#completedRequests').text(res.Data.CompletedRequests);
        }
    });
}

function loadRecentRequests() {
    $.get(APP.baseUrl + 'AdminRequests/GetAllRequests', { page: 1, pageSize: 5 }, function (res) {
        var tbody = $('#recentRequestsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="4" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        if (!res.Data || !res.Data.Data || !res.Data.Data.length) {
            tbody.append('<tr><td colspan="4">No requests found</td></tr>');
            return;
        }

        $.each(res.Data.Data, function (_, item) {
            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.UserName + '</td>' +
                '<td>' + item.CompanyName + '</td>' +
                '<td>' + getStatusBadge(item.Status) + '</td>' +
                '</tr>'
            );
        });
    });
}

function loadRecentUsers() {
    $.get(APP.baseUrl + 'AdminUsers/GetUsers', { page: 1, pageSize: 5 }, function (res) {
        var tbody = $('#recentUsersTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="3" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        if (!res.Data || !res.Data.Data || !res.Data.Data.length) {
            tbody.append('<tr><td colspan="3">No users found</td></tr>');
            return;
        }

        $.each(res.Data.Data, function (_, item) {
            tbody.append(
                '<tr>' +
                '<td>' + item.Name + '</td>' +
                '<td>' + item.Email + '</td>' +
                '<td><span class="badge ' + (item.Role === 'ADMIN' ? 'bg-danger' : 'bg-secondary') + '">' + item.Role + '</span></td>' +
                '</tr>'
            );
        });
    });
}

function getStatusBadge(status) {
    var statusMap = {
        'NR': '<span class="badge bg-warning">New Request</span>',
        'IP': '<span class="badge bg-info">In Progress</span>',
        'CP': '<span class="badge bg-success">Completed</span>'
    };
    return statusMap[status] || status;
}
