/* ============================
   Company Requests Management
============================ */
var companyCurrentPage = 1;
var companyPageSize = 20;

$(function () {
    loadCompanyRequests(1);

    // Update Company Status Button
    $('#updateStatusBtn').on('click', updateCompanyStatus);
});

function loadCompanyRequests(page) {
    companyCurrentPage = page;

    $.get(APP.baseUrl + 'AdminCompanyRequests/GetAllCompanyRequests', {
        page: page,
        pageSize: companyPageSize
    }, function (res) {
        var tbody = $('#companyRequestsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="9" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="9">No company requests found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var statusBadge = getCompanyStatusBadge(item.Status, item.IsCancelled);
            var cancelledBadge = item.IsCancelled
                ? '<span class="badge bg-secondary">Yes</span>'
                : '<span class="badge bg-light text-dark">No</span>';

            var actionBtn = '';
            if (item.IsCancelled) {
                actionBtn = '<span class="badge bg-secondary">Locked</span>';
            } else if (item.CanProcess) {
                actionBtn = '<button class="btn btn-sm btn-primary" onclick="openCompanyStatusModal(' + item.Id + ')">Update Status</button>';
            } else {
                actionBtn = '<span class="text-muted">-</span>';
            }

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.UserName + '</td>' +
                '<td>' + item.EmployeeName + '</td>' +
                '<td>' + item.CompanyName + '</td>' +
                '<td>' + item.ToolName + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + cancelledBadge + '</td>' +
                '<td>' + formatDateTime(item.CreatedAt) + '</td>' +
                '<td>' + formatDateTime(item.UpdatedAt) + '</td>' +
                '<td>' + actionBtn + '</td>' +
                '</tr>'
            );
        });

        renderCompanyPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function getCompanyStatusBadge(status, isCancelled) {
    if (isCancelled) {
        return '<span class="badge bg-secondary">Cancelled</span>';
    }

    var statusMap = {
        'NR': '<span class="badge bg-warning text-dark">New Request</span>',
        'IP': '<span class="badge bg-info">In Progress</span>',
        'CP': '<span class="badge bg-success">Completed</span>',
        'RR': '<span class="badge bg-danger">Rejected</span>'
    };
    return statusMap[status] || '<span class="badge bg-secondary">' + status + '</span>';
}

function openCompanyStatusModal(requestId) {
    // Reset form
    $('#crRequestId').val(requestId);
    $('#crStatus').val('');

    // Load request info
    $.get(APP.baseUrl + 'AdminCompanyRequests/GetCompanyRequest', { id: requestId }, function (res) {
        if (res.Data) {
            var req = res.Data;
            $('#crUserName').text(req.UserName);
            $('#crUserEmail').text(req.UserEmail);
            $('#crEmployeeName').text(req.EmployeeName);
            $('#crCompanyName').text(req.CompanyName);
            $('#crToolName').text(req.ToolName);
            $('#crStatus').val(req.Status);
        }
    });

    var modal = new bootstrap.Modal(document.getElementById('updateCompanyStatusModal'));
    modal.show();
}

function updateCompanyStatus() {
    var requestId = parseInt($('#crRequestId').val());
    var status = $('#crStatus').val();

    if (!status) {
        Swal.fire('Validation Error', 'Please select a status', 'warning');
        return;
    }

    $.ajax({
        url: APP.baseUrl + 'AdminCompanyRequests/UpdateCompanyRequestStatus',
        type: 'POST',
        data: {
            requestId: requestId,
            status: status
        },
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
            } else {
                Swal.fire('Success', res.Message, 'success');
                bootstrap.Modal.getInstance(document.getElementById('updateCompanyStatusModal')).hide();
                loadCompanyRequests(companyCurrentPage);
            }
        },
        error: function () {
            Swal.fire('Error', 'Failed to update status', 'error');
        }
    });
}

function renderCompanyPagination(totalRecords, page, pageSize) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#companyPagination').empty();

    if (totalPages <= 1) return;

    for (var i = 1; i <= totalPages; i++) {
        var activeClass = i === page ? 'active' : '';
        pagination.append(
            '<button class="btn btn-sm btn-outline-primary me-1 ' + activeClass +
            '" onclick="loadCompanyRequests(' + i + ')">' + i + '</button>'
        );
    }
}
