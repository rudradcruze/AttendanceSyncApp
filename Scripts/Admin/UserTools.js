/* Admin User Tools Management */
var currentPage = 1;
var pageSize = 20;

$(function () {
    loadAssignments(1);
    loadUsersDropdown();
    loadToolsDropdown();
});

function loadAssignments(page) {
    currentPage = page;
    var tbody = $('#assignmentsTable tbody');
    tbody.html('<tr><td colspan="8" class="text-center">Loading...</td></tr>');

    $.get(APP.baseUrl + 'AdminUserTools/GetAssignments', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        tbody.empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="8" class="text-center text-danger">' + res.Message + '</td></tr>');
            return;
        }

        if (!res.Data || !res.Data.Data || !res.Data.Data.length) {
            tbody.append('<tr><td colspan="8" class="text-center">No assignments found</td></tr>');
            $('#pagination').empty();
            return;
        }

        $.each(res.Data.Data, function (_, item) {
            var statusBadge = item.IsRevoked
                ? '<span class="badge bg-danger">Revoked</span>'
                : '<span class="badge bg-success">Active</span>';

            var actions = item.IsRevoked
                ? '<button class="btn btn-sm btn-success" onclick="unrevokeTool(' + item.UserId + ', ' + item.ToolId + ')">Restore</button>'
                : '<button class="btn btn-sm btn-danger" onclick="revokeTool(' + item.UserId + ', ' + item.ToolId + ')">Revoke</button>';

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + escapeHtml(item.UserName) + '</td>' +
                '<td>' + escapeHtml(item.UserEmail) + '</td>' +
                '<td>' + escapeHtml(item.ToolName) + '</td>' +
                '<td>' + escapeHtml(item.AssignedByName) + '</td>' +
                '<td>' + formatDateTime(item.AssignedAt) + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + actions + '</td>' +
                '</tr>'
            );
        });

        renderPagination(res.Data.TotalRecords, res.Data.Page, res.Data.PageSize);
    }).fail(function (xhr) {
        if (xhr.status === 401) {
            window.location.href = APP.baseUrl + 'Auth/Login';
        } else {
            tbody.html('<tr><td colspan="8" class="text-center text-danger">Failed to load assignments</td></tr>');
        }
    });
}

function showAssignModal() {
    $('#selectUser').val('');
    $('#selectTool').val('');
    var modal = new bootstrap.Modal(document.getElementById('assignModal'));
    modal.show();
}

function loadUsersDropdown() {
    $.get(APP.baseUrl + 'AdminUserTools/GetAllUsers', function (res) {
        var select = $('#selectUser');
        select.find('option:not(:first)').remove();
        if (res.Data) {
            $.each(res.Data, function (_, user) {
                select.append('<option value="' + user.Id + '">' + escapeHtml(user.Name) + ' (' + escapeHtml(user.Email) + ')</option>');
            });
        }
    });
}

function loadToolsDropdown() {
    $.get(APP.baseUrl + 'AdminUserTools/GetAllTools', function (res) {
        var select = $('#selectTool');
        select.find('option:not(:first)').remove();
        if (res.Data) {
            $.each(res.Data, function (_, tool) {
                select.append('<option value="' + tool.Id + '">' + escapeHtml(tool.Name) + '</option>');
            });
        }
    });
}

function assignTool() {
    var userId = $('#selectUser').val();
    var toolId = $('#selectTool').val();

    if (!userId || !toolId) {
        Swal.fire('Error', 'Please select both user and tool', 'error');
        return;
    }

    $.post(APP.baseUrl + 'AdminUserTools/AssignTool', {
        UserId: userId,
        ToolId: toolId
    }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
        } else {
            Swal.fire('Success', res.Message, 'success');
            bootstrap.Modal.getInstance(document.getElementById('assignModal')).hide();
            loadAssignments(currentPage);
        }
    }).fail(function (xhr) {
        if (xhr.status === 401) {
            window.location.href = APP.baseUrl + 'Auth/Login';
        } else {
            Swal.fire('Error', 'Failed to assign tool', 'error');
        }
    });
}

function revokeTool(userId, toolId) {
    Swal.fire({
        title: 'Confirm Revoke',
        text: 'Are you sure you want to revoke this tool access?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Revoke'
    }).then(function (result) {
        if (result.isConfirmed) {
            $.post(APP.baseUrl + 'AdminUserTools/RevokeTool', {
                UserId: userId,
                ToolId: toolId
            }, function (res) {
                if (res.Errors && res.Errors.length > 0) {
                    Swal.fire('Error', res.Message, 'error');
                } else {
                    Swal.fire('Success', res.Message, 'success');
                    loadAssignments(currentPage);
                }
            }).fail(function (xhr) {
                if (xhr.status === 401) {
                    window.location.href = APP.baseUrl + 'Auth/Login';
                } else {
                    Swal.fire('Error', 'Failed to revoke tool', 'error');
                }
            });
        }
    });
}

function unrevokeTool(userId, toolId) {
    Swal.fire({
        title: 'Confirm Restore',
        text: 'Are you sure you want to restore this tool access?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Restore'
    }).then(function (result) {
        if (result.isConfirmed) {
            $.post(APP.baseUrl + 'AdminUserTools/UnrevokeTool', {
                userId: userId,
                toolId: toolId
            }, function (res) {
                if (res.Errors && res.Errors.length > 0) {
                    Swal.fire('Error', res.Message, 'error');
                } else {
                    Swal.fire('Success', res.Message, 'success');
                    loadAssignments(currentPage);
                }
            }).fail(function (xhr) {
                if (xhr.status === 401) {
                    window.location.href = APP.baseUrl + 'Auth/Login';
                } else {
                    Swal.fire('Error', 'Failed to restore tool', 'error');
                }
            });
        }
    });
}

function formatDateTime(value) {
    if (!value) return '-';
    if (typeof value === 'string' && value.indexOf('/Date(') > -1) {
        var timestamp = parseInt(value.replace('/Date(', '').replace(')/', ''));
        var date = new Date(timestamp);
        return formatDateObj(date);
    }
    var date = new Date(value);
    if (isNaN(date.getTime())) return '-';
    return formatDateObj(date);
}

function formatDateObj(date) {
    var y = date.getFullYear();
    var m = String(date.getMonth() + 1).padStart(2, '0');
    var d = String(date.getDate()).padStart(2, '0');
    var h = String(date.getHours()).padStart(2, '0');
    var min = String(date.getMinutes()).padStart(2, '0');
    return y + '-' + m + '-' + d + ' ' + h + ':' + min;
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}

function renderPagination(totalRecords, page, pageSize) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#pagination').empty();

    if (totalPages <= 1) return;

    if (page > 1) {
        pagination.append('<li class="page-item"><a class="page-link" href="#" onclick="loadAssignments(' + (page - 1) + '); return false;">Previous</a></li>');
    }

    for (var i = 1; i <= totalPages; i++) {
        var activeClass = i === page ? 'active' : '';
        pagination.append('<li class="page-item ' + activeClass + '"><a class="page-link" href="#" onclick="loadAssignments(' + i + '); return false;">' + i + '</a></li>');
    }

    if (page < totalPages) {
        pagination.append('<li class="page-item"><a class="page-link" href="#" onclick="loadAssignments(' + (page + 1) + '); return false;">Next</a></li>');
    }
}
