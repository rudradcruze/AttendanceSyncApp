/* ============================
   Admin Server IP Management
   SalaryGarbge Namespace
============================ */
var currentPage = 1;
var pageSize = 20;
var serverIpModal = null;
var isEditMode = false;

$(function () {
    serverIpModal = new bootstrap.Modal(document.getElementById('serverIpModal'));
    loadServerIps(1);

    $('#saveServerIpBtn').on('click', saveServerIp);
});

function loadServerIps(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'AdminServerIp/GetServerIps', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#serverIpsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="8" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="8">No server IPs found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var statusBadge = item.IsActive
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-danger">Inactive</span>';

            var passwordCell = '<span class="password-masked" data-id="' + item.Id + '" style="cursor: pointer;" onclick="revealPassword(this, ' + item.Id + ')">******</span>';

            var actions =
                '<button class="btn btn-sm btn-primary me-1" onclick="editServerIp(' + item.Id + ')">Edit</button>' +
                '<button class="btn btn-sm ' + (item.IsActive ? 'btn-warning' : 'btn-success') + ' me-1" onclick="toggleStatus(' + item.Id + ')">' +
                (item.IsActive ? 'Deactivate' : 'Activate') +
                '</button>' +
                '<button class="btn btn-sm btn-danger" onclick="deleteServerIp(' + item.Id + ')">Delete</button>';

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + escapeHtml(item.IpAddress) + '</td>' +
                '<td>' + escapeHtml(item.DatabaseUser) + '</td>' +
                '<td>' + passwordCell + '</td>' +
                '<td>' + escapeHtml(item.Description || 'N/A') + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + formatDateTime(item.CreatedAt) + '</td>' +
                '<td>' + actions + '</td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function revealPassword(element, id) {
    var $el = $(element);

    // If already revealed, hide it
    if ($el.data('revealed')) {
        $el.html('******');
        $el.data('revealed', false);
        return;
    }

    // Fetch the password from server
    $.get(APP.baseUrl + 'AdminServerIp/GetServerIp', { id: id }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
            return;
        }

        var password = res.Data.DatabasePassword;
        $el.html('<code>' + escapeHtml(password) + '</code>');
        $el.data('revealed', true);
    });
}

function showCreateModal() {
    isEditMode = false;
    $('#serverIpModalTitle').text('Add Server IP');
    $('#serverIpId').val('');
    $('#ipAddress').val('');
    $('#databaseUser').val('');
    $('#databasePassword').val('').attr('type', 'password').attr('placeholder', '').prop('required', true);
    $('#showPassword').prop('checked', false);
    $('#serverDescription').val('');
    $('#serverActive').prop('checked', true);

    // Show required asterisk, hide hint
    $('.password-required').show();
    $('.password-hint').hide();

    serverIpModal.show();
}

function editServerIp(id) {
    $.get(APP.baseUrl + 'AdminServerIp/GetServerIp', { id: id }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
            return;
        }

        isEditMode = true;
        var serverIp = res.Data;
        $('#serverIpModalTitle').text('Edit Server IP');
        $('#serverIpId').val(serverIp.Id);
        $('#ipAddress').val(serverIp.IpAddress);
        $('#databaseUser').val(serverIp.DatabaseUser);
        $('#databasePassword').val('').attr('type', 'password').attr('placeholder', '').prop('required', false);
        $('#showPassword').prop('checked', false);
        $('#serverDescription').val(serverIp.Description || '');
        $('#serverActive').prop('checked', serverIp.IsActive);

        // Hide required asterisk, show hint
        $('.password-required').hide();
        $('.password-hint').show();

        serverIpModal.show();
    });
}

function saveServerIp() {
    var id = $('#serverIpId').val();
    var ipAddress = $('#ipAddress').val().trim();
    var databaseUser = $('#databaseUser').val().trim();
    var databasePassword = $('#databasePassword').val();
    var description = $('#serverDescription').val().trim();
    var isActive = $('#serverActive').prop('checked');

    if (!ipAddress) {
        Swal.fire('Validation Error', 'IP address is required', 'warning');
        return;
    }

    if (!databaseUser) {
        Swal.fire('Validation Error', 'Database user is required', 'warning');
        return;
    }

    // Password is required only for new entries
    if (!id && !databasePassword) {
        Swal.fire('Validation Error', 'Database password is required', 'warning');
        return;
    }

    var url = id ? APP.baseUrl + 'AdminServerIp/UpdateServerIp' : APP.baseUrl + 'AdminServerIp/CreateServerIp';
    var data = {
        IpAddress: ipAddress,
        DatabaseUser: databaseUser,
        Description: description,
        IsActive: isActive
    };

    if (id) {
        data.Id = parseInt(id);
        // Only send password if it was changed
        if (databasePassword) {
            data.DatabasePassword = databasePassword;
        }
    } else {
        data.DatabasePassword = databasePassword;
    }

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
            } else {
                Swal.fire('Success', res.Message, 'success');
                serverIpModal.hide();
                loadServerIps(currentPage);
            }
        },
        error: function () {
            Swal.fire('Error', 'Failed to save server IP', 'error');
        }
    });
}

function toggleStatus(id) {
    Swal.fire({
        title: 'Confirm',
        text: 'Are you sure you want to change this server IP\'s status?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, change it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminServerIp/ToggleServerIpStatus',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Success', res.Message, 'success');
                        loadServerIps(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to update status', 'error');
                }
            });
        }
    });
}

function deleteServerIp(id) {
    Swal.fire({
        title: 'Delete Server IP',
        text: 'Are you sure you want to delete this server IP? This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminServerIp/DeleteServerIp',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Deleted', res.Message, 'success');
                        loadServerIps(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to delete server IP', 'error');
                }
            });
        }
    });
}

function togglePasswordVisibility() {
    var passwordField = $('#databasePassword');
    var isChecked = $('#showPassword').prop('checked');
    passwordField.attr('type', isChecked ? 'text' : 'password');
}

function renderPagination(totalRecords, page, pageSize) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#pagination').empty();

    if (totalPages <= 1) return;

    for (var i = 1; i <= totalPages; i++) {
        var activeClass = i === page ? 'active' : '';
        pagination.append(
            '<button class="btn btn-sm btn-outline-primary me-1 ' + activeClass +
            '" onclick="loadServerIps(' + i + ')">' + i + '</button>'
        );
    }
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}
