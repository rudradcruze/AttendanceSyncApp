/* ============================
   Admin Employees Management
============================ */
var currentPage = 1;
var pageSize = 20;
var employeeModal = null;

$(function () {
    employeeModal = new bootstrap.Modal(document.getElementById('employeeModal'));
    loadEmployees(1);

    $('#saveEmployeeBtn').on('click', saveEmployee);
});

function loadEmployees(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'AdminEmployees/GetEmployees', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#employeesTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="6" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="6">No employees found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var statusBadge = item.IsActive
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-danger">Inactive</span>';

            var actions =
                '<button class="btn btn-sm btn-primary me-1" onclick="editEmployee(' + item.Id + ')">Edit</button>' +
                '<button class="btn btn-sm ' + (item.IsActive ? 'btn-warning' : 'btn-success') + ' me-1" onclick="toggleStatus(' + item.Id + ')">' +
                (item.IsActive ? 'Deactivate' : 'Activate') +
                '</button>' +
                '<button class="btn btn-sm btn-danger" onclick="deleteEmployee(' + item.Id + ')">Delete</button>';

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.Name + '</td>' +
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

function showCreateModal() {
    $('#employeeModalTitle').text('Add Employee');
    $('#employeeId').val('');
    $('#employeeName').val('');
    $('#employeeActive').prop('checked', true);
    employeeModal.show();
}

function editEmployee(id) {
    $.get(APP.baseUrl + 'AdminEmployees/GetEmployee', { id: id }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
            return;
        }

        var employee = res.Data;
        $('#employeeModalTitle').text('Edit Employee');
        $('#employeeId').val(employee.Id);
        $('#employeeName').val(employee.Name);
        $('#employeeActive').prop('checked', employee.IsActive);
        employeeModal.show();
    });
}

function saveEmployee() {
    var id = $('#employeeId').val();
    var name = $('#employeeName').val().trim();
    var isActive = $('#employeeActive').prop('checked');

    if (!name) {
        Swal.fire('Validation Error', 'Employee name is required', 'warning');
        return;
    }

    var url = id ? APP.baseUrl + 'AdminEmployees/UpdateEmployee' : APP.baseUrl + 'AdminEmployees/CreateEmployee';
    var data = id
        ? { Id: parseInt(id), Name: name, IsActive: isActive }
        : { Name: name, IsActive: isActive };

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
            } else {
                Swal.fire('Success', res.Message, 'success');
                employeeModal.hide();
                loadEmployees(currentPage);
            }
        },
        error: function () {
            Swal.fire('Error', 'Failed to save employee', 'error');
        }
    });
}

function toggleStatus(id) {
    Swal.fire({
        title: 'Confirm',
        text: 'Are you sure you want to change this employee\'s status?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, change it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminEmployees/ToggleEmployeeStatus',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Success', res.Message, 'success');
                        loadEmployees(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to update status', 'error');
                }
            });
        }
    });
}

function deleteEmployee(id) {
    Swal.fire({
        title: 'Delete Employee',
        text: 'Are you sure you want to delete this employee? This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminEmployees/DeleteEmployee',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Deleted', res.Message, 'success');
                        loadEmployees(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to delete employee', 'error');
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
            '" onclick="loadEmployees(' + i + ')">' + i + '</button>'
        );
    }
}
