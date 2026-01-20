/* ============================
   Company Request Management
============================ */
var currentPage = 1;
var pageSize = 20;
var createModal = null;
var pollingInterval = null;

$(function () {
    createModal = new bootstrap.Modal(document.getElementById('createRequestModal'));

    loadDropdowns();
    loadRequests(1);

    // Start polling
    startPolling();

    $('#submitRequestBtn').on('click', submitRequest);
});

function startPolling() {
    pollingInterval = setInterval(function() {
        loadRequests(currentPage, true);
    }, 2000);
}

function loadDropdowns() {
    // Load employees
    $.get(APP.baseUrl + 'CompanyRequest/GetEmployees', function (res) {
        if (res.Data) {
            var select = $('#employeeId');
            select.find('option:not(:first)').remove();
            $.each(res.Data, function (_, item) {
                select.append('<option value="' + item.Id + '">' + item.Name + '</option>');
            });
        }
    });

    // Load companies
    $.get(APP.baseUrl + 'CompanyRequest/GetCompanies', function (res) {
        if (res.Data) {
            var select = $('#companyId');
            select.find('option:not(:first)').remove();
            $.each(res.Data, function (_, item) {
                select.append('<option value="' + item.Id + '">' + item.Name + '</option>');
            });
        }
    });

    // Load tools
    $.get(APP.baseUrl + 'CompanyRequest/GetTools', function (res) {
        if (res.Data) {
            var select = $('#toolId');
            select.find('option:not(:first)').remove();
            $.each(res.Data, function (_, item) {
                select.append('<option value="' + item.Id + '">' + item.Name + '</option>');
            });
        }
    });
}

function loadRequests(page, isPolling) {
    if (!isPolling) {
        currentPage = page;
    }

    $.get(APP.baseUrl + 'CompanyRequest/GetMyRequests', {
        page: currentPage,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#requestsTable tbody');
        if (!isPolling) tbody.empty();

        if (res.Errors && res.Errors.length > 0) {
            if (!isPolling) tbody.append('<tr><td colspan="8" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        var rows = '';

        if (!data.Data || !data.Data.length) {
            rows = '<tr><td colspan="8">No requests found</td></tr>';
        } else {
            $.each(data.Data, function (_, item) {
                var statusBadge = getStatusBadge(item);
                var actions = '';

                if (item.IsCancelled) {
                    actions = '<span class="text-muted">-</span>';
                } else if (item.CanCancel) {
                    actions = '<button class="btn btn-sm btn-danger" onclick="cancelRequest(' + item.Id + ')">Cancel</button>';
                } else {
                    actions = '<span class="text-muted">-</span>';
                }

                rows += '<tr>' +
                    '<td>' + item.Id + '</td>' +
                    '<td>' + item.EmployeeName + '</td>' +
                    '<td>' + item.CompanyName + '</td>' +
                    '<td>' + item.ToolName + '</td>' +
                    '<td>' + statusBadge + '</td>' +
                    '<td>' + formatDateTime(item.CreatedAt) + '</td>' +
                    '<td>' + formatDateTime(item.UpdatedAt) + '</td>' +
                    '<td>' + actions + '</td>' +
                    '</tr>';
            });
        }
        
        tbody.html(rows);

        if (!isPolling) {
            renderPagination(data.TotalRecords, data.Page, data.PageSize);
        }
    });
}

function getStatusBadge(item) {
    if (item.IsCancelled) {
        return '<span class="badge bg-secondary">Cancelled</span>';
    }

    switch (item.Status) {
        case 'NR':
            return '<span class="badge bg-warning text-dark">New Request</span>';
        case 'IP':
            return '<span class="badge bg-info">In Progress</span>';
        case 'CP':
            return '<span class="badge bg-success">Completed</span>';
        case 'RR':
            return '<span class="badge bg-danger">Rejected</span>';
        default:
            return '<span class="badge bg-secondary">' + item.Status + '</span>';
    }
}

// ... existing code ...

function showCreateModal() {
    $('#createRequestForm')[0].reset();
    createModal.show();
}

function submitRequest() {
    var employeeId = $('#employeeId').val();
    var companyId = $('#companyId').val();
    var toolId = $('#toolId').val();

    if (!employeeId || !companyId || !toolId) {
        Swal.fire('Validation Error', 'Please fill in all required fields', 'warning');
        return;
    }

    var data = {
        EmployeeId: parseInt(employeeId),
        CompanyId: parseInt(companyId),
        ToolId: parseInt(toolId)
    };

    $.ajax({
        url: APP.baseUrl + 'CompanyRequest/CreateRequest',
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
                url: APP.baseUrl + 'CompanyRequest/CancelRequest',
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
