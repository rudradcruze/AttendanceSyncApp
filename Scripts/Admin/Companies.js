/* ============================
   Admin Companies Management
============================ */
var currentPage = 1;
var pageSize = 20;
var companyModal = null;

$(function () {
    companyModal = new bootstrap.Modal(document.getElementById('companyModal'));
    loadCompanies(1);

    $('#saveCompanyBtn').on('click', saveCompany);
});

function loadCompanies(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'AdminCompanies/GetCompanies', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#companiesTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="6" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="6">No companies found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var statusBadge = item.Status === 'Active'
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-danger">Inactive</span>';

            var actions =
                '<button class="btn btn-sm btn-primary me-1" onclick="editCompany(' + item.Id + ')">Edit</button>' +
                '<button class="btn btn-sm ' + (item.Status === 'Active' ? 'btn-warning' : 'btn-success') + ' me-1" onclick="toggleStatus(' + item.Id + ')">' +
                (item.Status === 'Active' ? 'Deactivate' : 'Activate') +
                '</button>' +
                '<button class="btn btn-sm btn-danger" onclick="deleteCompany(' + item.Id + ')">Delete</button>';

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.Name + '</td>' +
                '<td>' + (item.Email || 'N/A') + '</td>' +
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
    $('#companyModalTitle').text('Add Company');
    $('#companyId').val('');
    $('#companyName').val('');
    $('#companyEmail').val('');
    $('#companyStatus').val('Active');
    companyModal.show();
}

function editCompany(id) {
    $.get(APP.baseUrl + 'AdminCompanies/GetCompany', { id: id }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
            return;
        }

        var company = res.Data;
        $('#companyModalTitle').text('Edit Company');
        $('#companyId').val(company.Id);
        $('#companyName').val(company.Name);
        $('#companyEmail').val(company.Email || '');
        $('#companyStatus').val(company.Status);
        companyModal.show();
    });
}

function saveCompany() {
    var id = $('#companyId').val();
    var name = $('#companyName').val().trim();
    var email = $('#companyEmail').val().trim();
    var status = $('#companyStatus').val();

    if (!name) {
        Swal.fire('Validation Error', 'Company name is required', 'warning');
        return;
    }

    var url = id ? APP.baseUrl + 'AdminCompanies/UpdateCompany' : APP.baseUrl + 'AdminCompanies/CreateCompany';
    var data = id
        ? { Id: parseInt(id), Name: name, Email: email, Status: status }
        : { Name: name, Email: email, Status: status };

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
            } else {
                Swal.fire('Success', res.Message, 'success');
                companyModal.hide();
                loadCompanies(currentPage);
            }
        },
        error: function () {
            Swal.fire('Error', 'Failed to save company', 'error');
        }
    });
}

function toggleStatus(id) {
    Swal.fire({
        title: 'Confirm',
        text: 'Are you sure you want to change this company\'s status?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, change it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminCompanies/ToggleCompanyStatus',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Success', res.Message, 'success');
                        loadCompanies(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to update status', 'error');
                }
            });
        }
    });
}

function deleteCompany(id) {
    Swal.fire({
        title: 'Delete Company',
        text: 'Are you sure you want to delete this company? This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminCompanies/DeleteCompany',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Deleted', res.Message, 'success');
                        loadCompanies(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to delete company', 'error');
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
            '" onclick="loadCompanies(' + i + ')">' + i + '</button>'
        );
    }
}
