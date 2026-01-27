/* ============================
   Admin Login Sessions Management
============================ */
var currentPage = 1;
var pageSize = 20;

$(function () {
    loadSessions(1);

    // Auto-refresh every 2 seconds
    setInterval(function() {
        loadSessions(currentPage);
    }, 2000);

    // Filter Buttons
    $('#applyFiltersBtn').on('click', function() {
        loadSessions(1);
    });

    $('#resetFiltersBtn').on('click', function() {
        $('#filterUser').val('');
        $('#filterStatus').val('');
        loadSessions(1);
    });
});

function loadSessions(page) {
    currentPage = page;

    var filter = {
        page: page,
        pageSize: pageSize,
        userSearch: $('#filterUser').val(),
        isActive: $('#filterStatus').val() === '' ? null : $('#filterStatus').val() === 'true'
    };

    $.get(APP.baseUrl + 'AdminSessions/GetAllSessions', filter, function (res) {
        var tbody = $('#sessionsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="9" class="text-danger text-center">' + res.Message + '</td></tr>');
            $('#totalSessionsCount').text('0 Sessions');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="9" class="text-center">No sessions found</td></tr>');
            $('#totalSessionsCount').text('0 Sessions');
            $('#pagination').empty();
            return;
        }

        // Update total count
        $('#totalSessionsCount').text(data.TotalRecords + ' Session' + (data.TotalRecords !== 1 ? 's' : ''));

        $.each(data.Data, function (_, session) {
            var statusBadge = session.IsActive
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-secondary">Inactive</span>';

            var logoutDisplay = session.LogoutTime ? formatDateTime(session.LogoutTime) : 'N/A';

            tbody.append(
                '<tr>' +
                '<td>' + session.Id + '</td>' +
                '<td>' + escapeHtml(session.UserName) + '<br><small class="text-muted">' + escapeHtml(session.UserEmail) + '</small></td>' +
                '<td>' + escapeHtml(session.Device || 'N/A') + '</td>' +
                '<td>' + escapeHtml(session.Browser || 'N/A') + '</td>' +
                '<td>' + escapeHtml(session.IpAddress || 'N/A') + '</td>' +
                '<td>' + formatDateTime(session.LoginTime) + '</td>' +
                '<td>' + logoutDisplay + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td><button class="btn btn-sm btn-info" onclick="viewDetails(' + session.Id + ')"><i class="bi bi-eye"></i> View</button></td>' +
                '</tr>'
            );
        });

        renderPagination('#pagination', data.TotalRecords, data.Page, data.PageSize, loadSessions);
    }).fail(function(xhr) {
        var tbody = $('#sessionsTable tbody');
        if (xhr.status === 401) {
            tbody.html('<tr><td colspan="9" class="text-center text-danger">Session expired. Please login again.</td></tr>');
        } else {
            tbody.html('<tr><td colspan="9" class="text-center text-danger">Failed to load sessions</td></tr>');
        }
        $('#totalSessionsCount').text('0 Sessions');
    });
}

function viewDetails(sessionId) {
    $.get(APP.baseUrl + 'AdminSessions/GetSession?id=' + sessionId, function(res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
            return;
        }

        var session = res.Data;

        // Populate modal with session data
        $('#detailSessionId').text(session.Id);
        $('#detailId').text(session.Id);
        $('#detailStatus').html(session.IsActive
            ? '<span class="badge bg-success">Active</span>'
            : '<span class="badge bg-secondary">Inactive</span>');
        $('#detailLoginTime').text(formatDateTime(session.LoginTime));
        $('#detailLogoutTime').text(formatDateTime(session.LogoutTime));
        $('#detailIpAddress').text(session.IpAddress || 'N/A');
        $('#detailDevice').text(session.Device || 'N/A');
        $('#detailBrowser').text(session.Browser || 'N/A');

        // User info
        $('#detailUserId').text(session.UserId);
        $('#detailUserName').text(session.UserName);
        $('#detailUserEmail').text(session.UserEmail);

        // Show modal
        var modal = new bootstrap.Modal(document.getElementById('sessionDetailsModal'));
        modal.show();
    }).fail(function() {
        Swal.fire('Error', 'Failed to load session details', 'error');
    });
}
