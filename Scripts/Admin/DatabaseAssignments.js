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
            
            var statusBadge = '';
            var actionBtn = '';
            
            if (item.IsRevoked) {
                statusBadge = '<span class="badge bg-danger">Revoked</span>';
                actionBtn = '<button class="btn btn-sm btn-success me-1" onclick="unrevokeAssignment(' + item.Id + ')">Un-revoke</button>';
            } else {
                statusBadge = '<span class="badge bg-success">Active</span>';
                actionBtn = '<button class="btn btn-sm btn-danger me-1" onclick="revokeAssignment(' + item.Id + ')">Revoke</button>';
            }

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.CompanyRequestId + '</td>' +
                '<td>' + item.UserName + '</td>' +
                '<td>' + item.EmployeeName + '</td>' +
                '<td>' + item.CompanyName + '</td>' +
                '<td>' + item.ToolName + '</td>' +
                '<td>' + dbConfig + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + formatDateTime(item.AssignedAt) + '</td>' +
                '<td>' +
                    actionBtn +
                    '<button class="btn btn-sm btn-info" onclick="viewDetails(' + item.Id + ')">Details</button>' +
                '</td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function revokeAssignment(id) {
    Swal.fire({
        title: 'Revoke Assignment?',
        text: 'This will break the database connection for this user.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, Revoke'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminDatabaseAssignments/RevokeAssignment',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Revoked', res.Message, 'success');
                        loadAssignments(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to revoke assignment', 'error');
                }
            });
        }
    });
}

function unrevokeAssignment(id) {
    Swal.fire({
        title: 'Un-revoke Assignment?',
        text: 'This will restore the database connection.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, Restore'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminDatabaseAssignments/UnrevokeAssignment',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Restored', res.Message, 'success');
                        loadAssignments(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to un-revoke assignment', 'error');
                }
            });
        }
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
