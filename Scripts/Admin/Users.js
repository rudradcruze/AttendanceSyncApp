/* ============================
   Admin Users Management
============================ */
var currentPage = 1;
var pageSize = 20;

$(function () {
    loadUsers(1);

    // Save User Button
    $('#saveUserBtn').on('click', saveUser);
});

function loadUsers(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'Admin/GetUsers', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#usersTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="7" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="7">No users found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var roleBadge = item.Role === 'ADMIN'
                ? '<span class="badge bg-danger">ADMIN</span>'
                : '<span class="badge bg-secondary">USER</span>';

            var statusBadge = item.IsActive
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-danger">Inactive</span>';

            var actions =
                '<button class="btn btn-sm btn-primary me-1" onclick="editUser(' + item.Id + ')">Edit</button>' +
                '<button class="btn btn-sm ' + (item.IsActive ? 'btn-warning' : 'btn-success') + '" onclick="toggleStatus(' + item.Id + ')">' +
                (item.IsActive ? 'Deactivate' : 'Activate') +
                '</button>';

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.Name + '</td>' +
                '<td>' + item.Email + '</td>' +
                '<td>' + roleBadge + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + formatDate(item.CreatedAt) + '</td>' +
                '<td>' + actions + '</td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function editUser(userId) {
    $.get(APP.baseUrl + 'Admin/GetUser', { id: userId }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            alert('Error: ' + res.Message);
            return;
        }

        var user = res.Data;
        $('#editUserId').val(user.Id);
        $('#editUserName').val(user.Name);
        $('#editUserEmail').val(user.Email);
        $('#editUserActive').prop('checked', user.IsActive);

        var modal = new bootstrap.Modal(document.getElementById('editUserModal'));
        modal.show();
    });
}

function saveUser() {
    var data = {
        Id: parseInt($('#editUserId').val()),
        Name: $('#editUserName').val(),
        Email: $('#editUserEmail').val(),
        IsActive: $('#editUserActive').prop('checked')
    };

    $.ajax({
        url: APP.baseUrl + 'Admin/UpdateUser',
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                alert('Error: ' + res.Message);
            } else {
                alert('User updated successfully!');
                bootstrap.Modal.getInstance(document.getElementById('editUserModal')).hide();
                loadUsers(currentPage);
            }
        },
        error: function () {
            alert('Failed to update user');
        }
    });
}

function toggleStatus(userId) {
    if (!confirm('Are you sure you want to change this user\'s status?')) {
        return;
    }

    $.ajax({
        url: APP.baseUrl + 'Admin/ToggleUserStatus',
        type: 'POST',
        data: { userId: userId },
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                alert('Error: ' + res.Message);
            } else {
                loadUsers(currentPage);
            }
        },
        error: function () {
            alert('Failed to update user status');
        }
    });
}

function formatDate(value) {
    if (!value) return 'N/A';
    var date = new Date(value);
    return date.toLocaleDateString();
}

function renderPagination(totalRecords, page, pageSize) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#pagination').empty();

    if (totalPages <= 1) return;

    for (var i = 1; i <= totalPages; i++) {
        var activeClass = i === page ? 'active' : '';
        pagination.append(
            '<button class="btn btn-sm btn-outline-primary me-1 ' + activeClass +
            '" onclick="loadUsers(' + i + ')">' + i + '</button>'
        );
    }
}
