/* ============================
   Admin Requests Management
============================ */
var currentPage = 1;
var pageSize = 20;

$(function () {
    loadRequests(1);

    // Test Connection Button
    $('#testConnectionBtn').on('click', testConnection);

    // Assign Database Button
    $('#assignDatabaseBtn').on('click', assignDatabase);
});

function loadRequests(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'AdminRequests/GetAllRequests', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#requestsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="7" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="7">No requests found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var statusBadge = getStatusBadge(item.Status);
            var actionBtn = item.Status !== 'CP'
                ? '<button class="btn btn-sm btn-primary" onclick="openAssignModal(' + item.Id + ')">Assign DB</button>'
                : '<span class="text-success">Assigned</span>';

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.UserName + '</td>' +
                '<td>' + item.CompanyName + '</td>' +
                '<td>' + item.ToolName + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + formatDate(item.CreatedAt) + '</td>' +
                '<td>' + actionBtn + '</td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function openAssignModal(requestId) {
    // Reset form
    $('#requestId').val(requestId);
    $('#databaseIP').val('.');
    $('#databaseName').val('');
    $('#databaseUserId').val('');
    $('#databasePassword').val('');
    $('#connectionStatus').text('');

    // Load request info
    $.get(APP.baseUrl + 'AdminRequests/GetRequest', { id: requestId }, function (res) {
        if (res.Data) {
            var req = res.Data;
            $('#reqUserName').text(req.UserName);
            $('#reqUserEmail').text(req.UserEmail);
            $('#reqCompanyName').text(req.CompanyName);
            $('#reqToolName').text(req.ToolName);
        }
    });

    var modal = new bootstrap.Modal(document.getElementById('assignDatabaseModal'));
    modal.show();
}

function testConnection() {
    var data = {
        DatabaseIP: $('#databaseIP').val(),
        DatabaseName: $('#databaseName').val(),
        DatabaseUserId: $('#databaseUserId').val(),
        DatabasePassword: $('#databasePassword').val()
    };

    if (!data.DatabaseIP || !data.DatabaseName || !data.DatabaseUserId || !data.DatabasePassword) {
        $('#connectionStatus').html('<span class="text-danger">All fields are required</span>');
        return;
    }

    $('#connectionStatus').html('<span class="text-info">Testing...</span>');

    $.ajax({
        url: APP.baseUrl + 'AdminRequests/TestDatabaseConnection',
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                $('#connectionStatus').html('<span class="text-danger">Failed: ' + res.Message + '</span>');
            } else {
                $('#connectionStatus').html('<span class="text-success">Connection successful!</span>');
            }
        },
        error: function () {
            $('#connectionStatus').html('<span class="text-danger">Connection failed</span>');
        }
    });
}

function assignDatabase() {
    var data = {
        RequestId: parseInt($('#requestId').val()),
        DatabaseIP: $('#databaseIP').val(),
        DatabaseName: $('#databaseName').val(),
        DatabaseUserId: $('#databaseUserId').val(),
        DatabasePassword: $('#databasePassword').val()
    };

    if (!data.DatabaseIP || !data.DatabaseName || !data.DatabaseUserId || !data.DatabasePassword) {
        alert('All fields are required');
        return;
    }

    $.ajax({
        url: APP.baseUrl + 'AdminRequests/AssignDatabase',
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                alert('Error: ' + res.Message);
            } else {
                alert('Database assigned successfully! Request marked as Completed.');
                bootstrap.Modal.getInstance(document.getElementById('assignDatabaseModal')).hide();
                loadRequests(currentPage);
            }
        },
        error: function () {
            alert('Failed to assign database');
        }
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

// Simple formate (without hours/minutes/sec)
//function formatDate(value) {
//    if (!value) return 'N/A';
//    var date = new Date(value);
//    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
//}

function formatDate(value) {
    if (!value) return 'N/A';
    console.log(value)

    var date = new Date(value);

    return date.toLocaleString(undefined, {
        year: 'numeric',
        month: 'short',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: true
    });
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
