/* ============================
   Admin Sync Requests Management
============================ */
var currentPage = 1;
var pageSize = 20;

$(function () {
    loadRequests(1);

    // Filter Buttons
    $('#applyFiltersBtn').on('click', function() {
        loadRequests(1);
    });

    $('#resetFiltersBtn').on('click', function() {
        $('#filterUser').val('');
        $('#filterCompany').val('');
        $('#filterStatus').val('');
        $('#filterFromDate').val('');
        $('#filterToDate').val('');
        loadRequests(1);
    });

    // Process Save Button
    $('#saveProcessBtn').on('click', saveProcess);
});

function loadRequests(page) {
    currentPage = page;

    var filter = {
        Page: page,
        PageSize: pageSize,
        UserSearch: $('#filterUser').val(),
        CompanyId: $('#filterCompany').val() ? parseInt($('#filterCompany').val()) : null,
        Status: $('#filterStatus').val(),
        FromDate: $('#filterFromDate').val(),
        ToDate: $('#filterToDate').val()
    };

    $.get(APP.baseUrl + 'AdminRequests/GetAllRequests', filter, function (res) {
        var tbody = $('#requestsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="9" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="9" class="text-center">No requests found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var statusBadge = getStatusBadge(item.IsSuccessful);
            var externalIdDisplay = item.ExternalSyncId ? item.ExternalSyncId : '<span class="text-muted">-</span>';
            
            var actionBtn = '';
            if (item.IsSuccessful === null) {
                actionBtn = '<button class="btn btn-sm btn-primary" onclick="openProcessModal(' + item.Id + ')">Process</button>';
            } else {
                 actionBtn = '<button class="btn btn-sm btn-outline-secondary" onclick="openProcessModal(' + item.Id + ', ' + item.ExternalSyncId + ', ' + item.IsSuccessful + ')">Edit</button>';
            }

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.UserName + '<br><small class="text-muted">' + item.UserEmail + '</small></td>' +
                '<td>' + item.CompanyName + '</td>' +
                '<td>' + item.ToolName + '</td>' +
                '<td>' + externalIdDisplay + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + formatDateTime(item.CreatedAt) + '</td>' +
                '<td>' + formatDateTime(item.UpdatedAt) + '</td>' +
                '<td>' + actionBtn + '</td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function openProcessModal(requestId, currentExtId, currentSuccess) {
    $('#processRequestId').val(requestId);
    
    if (currentExtId !== undefined) {
        $('#externalSyncId').val(currentExtId);
        $('#processOutcome').val(currentSuccess.toString());
    } else {
        $('#externalSyncId').val('');
        $('#processOutcome').val('true');
    }

    var modal = new bootstrap.Modal(document.getElementById('processRequestModal'));
    modal.show();
}

function saveProcess() {
    var data = {
        RequestId: parseInt($('#processRequestId').val()),
        ExternalSyncId: $('#externalSyncId').val() ? parseInt($('#externalSyncId').val()) : null,
        IsSuccessful: $('#processOutcome').val() === 'true'
    };

    if (!data.ExternalSyncId) {
        // Optional? User asked to "set ExternalSyncId". Assume required or at least warned.
        // Let's make it optional if they just want to mark as failed? 
        // But usually sync implies an external ID.
        // I'll leave it valid if empty, passing null.
    }

    $.ajax({
        url: APP.baseUrl + 'AdminRequests/ProcessRequest',
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                alert('Error: ' + res.Message);
            } else {
                alert('Request processed successfully!');
                bootstrap.Modal.getInstance(document.getElementById('processRequestModal')).hide();
                loadRequests(currentPage);
            }
        },
        error: function () {
            alert('Failed to process request');
        }
    });
}

function getStatusBadge(isSuccessful) {
    if (isSuccessful === true) return '<span class="badge bg-success">Completed</span>';
    if (isSuccessful === false) return '<span class="badge bg-danger">Failed</span>';
    return '<span class="badge bg-warning text-dark">Pending</span>';
}

function renderPagination(totalRecords, page, pageSize) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#pagination').empty();

    if (totalPages <= 1) return;

    for (var i = 1; i <= totalPages; i++) {
        var activeClass = i === page ? 'active' : '';
        pagination.append(
            '<button class="btn btn-sm btn-outline-primary me-1 ' + activeClass +
            '" onclick="loadRequests(' + i + ')">' + i + '</button>'
        );
    }
}