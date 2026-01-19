/* ============================
   Database Assignments Management
============================ */
var currentPage = 1;
var pageSize = 20;

$(function () {
    loadAssignments(1);
});

function loadAssignments(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'AdminDatabaseAssignments/GetAll', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#assignmentsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="10" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="10">No assignments found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var dbConfig = item.DatabaseIP + ' / ' + item.DatabaseName;

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.CompanyRequestId + '</td>' +
                '<td>' + item.UserName + '</td>' +
                '<td>' + item.EmployeeName + '</td>' +
                '<td>' + item.CompanyName + '</td>' +
                '<td>' + item.ToolName + '</td>' +
                '<td>' + dbConfig + '</td>' +
                '<td>' + item.AssignedByName + '</td>' +
                '<td>' + formatDateTime(item.AssignedAt) + '</td>' +
                '<td>' +
                    '<button class="btn btn-sm btn-info" onclick="viewDetails(' + item.Id + ')">View Details</button>' +
                '</td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function viewDetails(id) {
    $.get(APP.baseUrl + 'AdminDatabaseAssignments/Get', { id: id }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message || 'Failed to load details', 'error');
            return;
        }

        if (res.Data) {
            var data = res.Data;
            $('#detailRequestId').text(data.CompanyRequestId);
            $('#detailUserName').text(data.UserName);
            $('#detailUserEmail').text(data.UserEmail);
            $('#detailEmployeeName').text(data.EmployeeName);
            $('#detailCompanyName').text(data.CompanyName);
            $('#detailToolName').text(data.ToolName);
            $('#detailDatabaseIP').text(data.DatabaseIP);
            $('#detailDatabaseName').text(data.DatabaseName);
            $('#detailDatabaseUserId').text(data.DatabaseUserId);
            $('#detailAssignedBy').text(data.AssignedByName);
            $('#detailAssignedAt').text(formatDateTime(data.AssignedAt));

            var modal = new bootstrap.Modal(document.getElementById('viewDetailsModal'));
            modal.show();
        }
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
            '" onclick="loadAssignments(' + i + ')">' + i + '</button>'
        );
    }
}
