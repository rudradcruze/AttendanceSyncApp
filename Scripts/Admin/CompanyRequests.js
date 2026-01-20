/* ============================
   Company Requests Management
============================ */
var companyCurrentPage = 1;
var companyPageSize = 20;
var lastKnownRequestId = 0;
var pollingInterval = null;

$(function () {
    loadCompanyRequests(1);

    // Start polling for updates every 2 seconds
    startPolling();

    // Event handlers
    $('#assignDatabaseBtn').on('click', assignDatabase);
});

// ===== Polling for Updates =====
function startPolling() {
    // Poll every 2 seconds to refresh data
    pollingInterval = setInterval(function() {
        // Silent reload (keep current page)
        loadCompanyRequests(companyCurrentPage, true);
    }, 2000);
}

function loadCompanyRequests(page, isPolling) {
    // If manually called (not polling), update current page
    if (!isPolling) {
        companyCurrentPage = page;
    }

    $.get(APP.baseUrl + 'AdminCompanyRequests/GetAllCompanyRequests', {
        page: companyCurrentPage,
        pageSize: companyPageSize
    }, function (res) {
        var tbody = $('#companyRequestsTable tbody');
        
        // If not polling, we empty first to show loading potentially, but for polling we just replace content
        if (!isPolling) {
            tbody.empty();
            $('#newRequestsBadge').hide();
        }

        if (res.Errors && res.Errors.length > 0) {
            if (!isPolling) tbody.append('<tr><td colspan="10" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        
        // Rebuild table content
        var rows = '';
        if (!data.Data || !data.Data.length) {
            rows = '<tr><td colspan="10">No company requests found</td></tr>';
        } else {
            // Update lastKnownRequestId if needed
            if (data.Data.length > 0 && data.Data[0].Id > lastKnownRequestId) {
                lastKnownRequestId = data.Data[0].Id;
            }

            $.each(data.Data, function (_, item) {
                var statusBadge = getCompanyStatusBadge(item);
                //var cancelledBadge = item.IsCancelled
                //    ? '<span class="badge bg-secondary">Yes</span>'
                //    : '<span class="badge bg-light text-dark">No</span>';

                var actionBtns = buildActionButtons(item);

                rows += '<tr>' +
                    '<td>' + item.Id + '</td>' +
                    '<td>' + item.UserName + '</td>' +
                    '<td>' + item.EmployeeName + '</td>' +
                    '<td>' + item.CompanyName + '</td>' +
                    '<td>' + item.ToolName + '</td>' +
                    '<td>' + statusBadge + '</td>' +
                    //'<td>' + cancelledBadge + '</td>' +
                    '<td>' + formatDateTime(item.CreatedAt) + '</td>' +
                    '<td>' + formatDateTime(item.UpdatedAt) + '</td>' +
                    '<td>' + actionBtns + '</td>' +
                    '</tr>';
            });
        }
        
        tbody.html(rows);

        if (!isPolling) {
            renderCompanyPagination(data.TotalRecords, data.Page, data.PageSize);
        }
    });
}

function buildActionButtons(item) {
    // If cancelled - show locked
    if (item.IsCancelled) {
        return '<span class="text-muted">-</span>';
    }

    // If rejected or completed - no actions
    if (item.Status === 'RR' || item.Status === 'CP') {
        return '<span class="text-muted">-</span>';
    }

    var buttons = '';

    // New Request (NR) - show Accept and Reject buttons
    if (item.Status === 'NR') {
        buttons += '<button class="btn btn-sm btn-success me-1" onclick="acceptRequest(' + item.Id + ')">Accept</button>';
        buttons += '<button class="btn btn-sm btn-danger" onclick="rejectRequest(' + item.Id + ')">Reject</button>';
    }

    // In Progress (IP) - show Reject and Assign Database buttons
    if (item.Status === 'IP') {
        buttons += '<button class="btn btn-sm btn-danger me-1" onclick="rejectRequest(' + item.Id + ')">Reject</button>';
        buttons += '<button class="btn btn-sm btn-primary" onclick="openAssignDatabaseModal(' + item.Id + ')">Assign DB</button>';
    }

    return buttons || '<span class="text-muted">-</span>';
}

function getCompanyStatusBadge(item) {
    if (item.IsCancelled) {
        return '<span class="badge bg-secondary">Cancelled</span>';
    }

    var statusMap = {
        'NR': '<span class="badge bg-warning text-dark">New Request</span>',
        'IP': '<span class="badge bg-info">In Progress</span>',
        'CP': '<span class="badge bg-success">Completed</span>',
        'RR': '<span class="badge bg-danger">Rejected</span>'
    };
    return statusMap[item.Status] || '<span class="badge bg-secondary">' + item.Status + '</span>';
}

// ===== Accept/Reject Actions =====
function acceptRequest(requestId) {
    Swal.fire({
        title: 'Accept Request?',
        text: 'This will set the request status to In Progress.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, Accept'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminCompanyRequests/AcceptRequest',
                type: 'POST',
                data: { requestId: requestId },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Success', res.Message, 'success');
                        loadCompanyRequests(companyCurrentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to accept request', 'error');
                }
            });
        }
    });
}

function rejectRequest(requestId) {
    Swal.fire({
        title: 'Reject Request?',
        text: 'This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, Reject'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminCompanyRequests/RejectRequest',
                type: 'POST',
                data: { requestId: requestId },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Rejected', res.Message, 'success');
                        loadCompanyRequests(companyCurrentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to reject request', 'error');
                }
            });
        }
    });
}

// ===== Database Assignment =====
function openAssignDatabaseModal(requestId) {
    $('#adRequestId').val(requestId);

    // Reset sections
    $('#dbConfigSection').show();
    $('#noConfigSection').hide();
    $('#assignDatabaseBtn').prop('disabled', false);

    // Load request info
    $.get(APP.baseUrl + 'AdminCompanyRequests/GetCompanyRequest', { id: requestId }, function (res) {
        if (res.Data) {
            var req = res.Data;
            $('#adUserName').text(req.UserName);
            $('#adEmployeeName').text(req.EmployeeName);
            $('#adCompanyName').text(req.CompanyName);
            $('#adToolName').text(req.ToolName);
        }
    });

    // Load database configuration for this request's company
    $.get(APP.baseUrl + 'AdminCompanyRequests/GetDatabaseConfigForRequest', { requestId: requestId }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            // No config found
            $('#dbConfigSection').hide();
            $('#noConfigSection').show();
            $('#noConfigMessage').text(res.Message);
            $('#assignDatabaseBtn').prop('disabled', true);
        } else if (res.Data) {
            var config = res.Data;
            $('#adDatabaseIP').text(config.DatabaseIP);
            $('#adDatabaseName').text(config.DatabaseName);
            $('#adDatabaseUserId').text(config.DatabaseUserId);
        }
    });

    var modal = new bootstrap.Modal(document.getElementById('assignDatabaseModal'));
    modal.show();
}

function assignDatabase() {
    var requestId = parseInt($('#adRequestId').val());

    Swal.fire({
        title: 'Confirm Assignment',
        text: 'Are you sure you want to assign this database configuration? The request will be marked as completed.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, Assign'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminCompanyRequests/AssignDatabase',
                type: 'POST',
                data: { requestId: requestId },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Success', res.Message, 'success');
                        bootstrap.Modal.getInstance(document.getElementById('assignDatabaseModal')).hide();
                        loadCompanyRequests(companyCurrentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to assign database', 'error');
                }
            });
        }
    });
}

function renderCompanyPagination(totalRecords, page, pageSize) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#companyPagination').empty();

    if (totalPages <= 1) return;

    for (var i = 1; i <= totalPages; i++) {
        var activeClass = i === page ? 'active' : '';
        pagination.append(
            '<button class="btn btn-sm btn-outline-primary me-1 ' + activeClass +
            '" onclick="loadCompanyRequests(' + i + ')">' + i + '</button>'
        );
    }
}

// Cleanup on page unload
$(window).on('beforeunload', function () {
    if (pollingInterval) {
        clearInterval(pollingInterval);
    }
});
