/* ============================
   User Requests Management
============================ */
var currentPage = 1;
var pageSize = 20;
var createModal = null;

$(function () {
    createModal = new bootstrap.Modal(document.getElementById('createRequestModal'));

    loadDropdowns();
    loadRequests(1);

    $('#submitRequestBtn').on('click', submitRequest);
});

function loadDropdowns() {
    // Load employees
    $.get(APP.baseUrl + 'Attandance/GetEmployees', function (res) {
        if (res.Data) {
            var select = $('#employeeId');
            select.find('option:not(:first)').remove();
            $.each(res.Data, function (_, item) {
                select.append('<option value="' + item.Id + '">' + item.Name + '</option>');
            });
        }
    });

    // Load companies
    $.get(APP.baseUrl + 'Attandance/GetCompanies', function (res) {
        if (res.Data) {
            var select = $('#companyId');
            select.find('option:not(:first)').remove();
            $.each(res.Data, function (_, item) {
                select.append('<option value="' + item.Id + '">' + item.Name + '</option>');
            });
        }
    });

    // Load tools
    $.get(APP.baseUrl + 'Attandance/GetTools', function (res) {
        if (res.Data) {
            var select = $('#toolId');
            select.find('option:not(:first)').remove();
            $.each(res.Data, function (_, item) {
                select.append('<option value="' + item.Id + '">' + item.Name + '</option>');
            });
        }
    });
}

function loadRequests(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'Attandance/GetMyRequests', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#requestsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="9" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="9">No requests found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var statusBadge = getStatusBadge(item.Status);
            var canCancel = item.Status === 'Pending';

            var actions = canCancel
                ? '<button class="btn btn-sm btn-danger" onclick="cancelRequest(' + item.Id + ')">Cancel</button>'
                : '<span class="text-muted">-</span>';

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.EmployeeName + '</td>' +
                '<td>' + item.CompanyName + '</td>' +
                '<td>' + item.ToolName + '</td>' +
                '<td>' + formatDate(item.FromDate) + '</td>' +
                '<td>' + formatDate(item.ToDate) + '</td>' +
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

function getStatusBadge(status) {
    switch (status) {
        case 'Pending':
            return '<span class="badge bg-warning text-dark">Pending</span>';
        case 'Completed':
            return '<span class="badge bg-success">Completed</span>';
        case 'Failed':
            return '<span class="badge bg-danger">Failed</span>';
        default:
            return '<span class="badge bg-secondary">' + status + '</span>';
    }
}

function showCreateModal() {
    $('#createRequestForm')[0].reset();
    createModal.show();
}

function submitRequest() {
    var employeeId = $('#employeeId').val();
    var companyId = $('#companyId').val();
    var toolId = $('#toolId').val();
    var fromDate = $('#fromDate').val();
    var toDate = $('#toDate').val();

    if (!employeeId || !companyId || !toolId || !fromDate || !toDate) {
        Swal.fire('Validation Error', 'Please fill in all required fields', 'warning');
        return;
    }

    var data = {
        EmployeeId: parseInt(employeeId),
        CompanyId: parseInt(companyId),
        ToolId: parseInt(toolId),
        FromDate: fromDate,
        ToDate: toDate
    };

    $.ajax({
        url: APP.baseUrl + 'Attandance/CreateRequest',
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
            } else {
                Swal.fire('Success', res.Message, 'success');
                createModal.hide();
                loadRequests(1);
            }
        },
        error: function () {
            Swal.fire('Error', 'Failed to create request', 'error');
        }
    });
}

function cancelRequest(id) {
    Swal.fire({
        title: 'Cancel Request',
        text: 'Are you sure you want to cancel this request?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, cancel it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'Attandance/CancelRequest',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Cancelled', res.Message, 'success');
                        loadRequests(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to cancel request', 'error');
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
            '" onclick="loadRequests(' + i + ')">' + i + '</button>'
        );
    }
}
