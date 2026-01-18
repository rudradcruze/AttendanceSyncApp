/* ============================
   Admin Users Management
============================ */
var currentPage = 1;
var pageSize = 20;
var userModal = null;

$(function () {
    userModal = new bootstrap.Modal(document.getElementById('editUserModal'));
    loadUsers(1);

    $('#saveUserBtn').on('click', saveUser);
});

function loadUsers(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'AdminUsers/GetUsers', {
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
                '<td>' + formatDateTime(item.CreatedAt) + '</td>' +
                '<td>' + formatDateTime(item.UpdatedAt) + '</td>' +
                '<td>' + actions + '</td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function editUser(userId) {
    $.get(APP.baseUrl + 'AdminUsers/GetUser', { id: userId }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
            return;
        }

        var user = res.Data;
        $('#editUserId').val(user.Id);
        $('#editUserName').val(user.Name);
        $('#editUserEmail').val(user.Email);
        $('#editUserActive').prop('checked', user.IsActive);

        userModal.show();
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
        url: APP.baseUrl + 'AdminUsers/UpdateUser',
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
            } else {
                Swal.fire('Success', res.Message, 'success');
                userModal.hide();
                loadUsers(currentPage);
            }
        },
        error: function () {
            Swal.fire('Error', 'Failed to update user', 'error');
        }
    });
}

function toggleStatus(userId) {
    Swal.fire({
        title: 'Confirm',
        text: 'Are you sure you want to change this user\'s status?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, change it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminUsers/ToggleUserStatus',
                type: 'POST',
                data: { userId: userId },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Success', res.Message, 'success');
                        loadUsers(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to update user status', 'error');
                }
            });
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
            '" onclick="loadUsers(' + i + ')">' + i + '</button>'
        );
    }
}
