/* Attandance Synchronization - Updated with Modal, Filters, and Company Database Selection */
var currentPage = 1;
var pageSize = 20;
var currentUser = null;
var selectedCompanyData = null;
var currentFilters = {};
var companyDatabases = [];

$(function () {
    loadCurrentUser();
    loadCompanyDatabases();

    // Company database change
    $('#companyDatabaseSelect').on('change', function () {
        var selectedIndex = this.selectedIndex;
        if (selectedIndex > 0 && companyDatabases.length > 0) {
            selectedCompanyData = companyDatabases[selectedIndex - 1]; // -1 because first option is placeholder
            $('#syncContent').removeClass('d-none');
            $('#noAccessMessage').addClass('d-none');

            // Update modal with selected company info
            $('#selectedCompanyName').val(selectedCompanyData.CompanyName);
            $('#selectedCompanyId').val(selectedCompanyData.CompanyId);
            $('#selectedToolId').val(selectedCompanyData.ToolId);
            $('#selectedDbConfigId').val(selectedCompanyData.DatabaseConfigurationId);

            loadEmployees();
            loadSynchronizations(1);
        } else {
            selectedCompanyData = null;
            $('#syncContent').addClass('d-none');
        }
    });

    // Sync dates
    $('#fromDate').on('change', function () {
        $('#toDate').val($(this).val());
    });

    // Auto-refresh statuses
    setInterval(updateStatusesOnly, 3000);
});

function loadCurrentUser() {
    $.get(APP.baseUrl + 'Auth/CurrentUser', function (res) {
        if (res.Data) {
            currentUser = res.Data;
            $('#userEmail').val(currentUser.Email);
        }
    }).fail(function (xhr) {
        if (xhr.status === 401) {
            window.location.href = APP.baseUrl + 'Auth/Login';
        }
    });
}

function loadCompanyDatabases() {
    $.get(APP.baseUrl + 'Attandance/GetMyCompanyDatabases', function (res) {
        var select = $('#companyDatabaseSelect');
        select.find('option:not(:first)').remove();

        if (res.Errors && res.Errors.length > 0) {
            $('#noAccessMessage').removeClass('d-none');
            return;
        }

        if (res.Data && res.Data.length > 0) {
            companyDatabases = res.Data;
            $.each(res.Data, function (_, item) {
                select.append(
                    '<option value="' + item.DatabaseAssignmentId + '">' +
                    escapeHtml(item.CompanyName) + ' - ' + escapeHtml(item.DatabaseName) +
                    '</option>'
                );
            });
        } else {
            $('#noAccessMessage').removeClass('d-none');
        }
    }).fail(function (xhr) {
        if (xhr.status === 401) {
            window.location.href = APP.baseUrl + 'Auth/Login';
        } else {
            $('#noAccessMessage').removeClass('d-none');
        }
    });
}

function loadEmployees() {
    $.get(APP.baseUrl + 'Attandance/GetEmployees', function (res) {
        var select = $('#employeeId');
        var filterSelect = $('#filterEmployee');

        select.find('option:not(:first)').remove();
        filterSelect.find('option:not(:first)').remove();

        if (res.Data) {
            $.each(res.Data, function (_, emp) {
                select.append('<option value="' + emp.Id + '">' + escapeHtml(emp.Name) + '</option>');
                filterSelect.append('<option value="' + emp.Id + '">' + escapeHtml(emp.Name) + '</option>');
            });
        }
    });
}

function loadSynchronizations(page) {
    currentPage = page;
    var tbody = $('#syncTableBody');
    tbody.html('<tr><td colspan="8" class="text-center">Loading...</td></tr>');

    if (!selectedCompanyData) {
        tbody.html('<tr><td colspan="8" class="text-center">Select a company database to view requests</td></tr>');
        return;
    }

    $.get(APP.baseUrl + 'Attandance/GetMyRequests', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        tbody.empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="8" class="text-center text-danger">' + res.Message + '</td></tr>');
            $('#pagination').empty();
            return;
        }

        if (!res.Data || !res.Data.Data || !res.Data.Data.length) {
            tbody.append('<tr><td colspan="8" class="text-center">No records found</td></tr>');
            $('#pagination').empty();
            return;
        }

        // Filter by selected company if needed
        var filteredData = res.Data.Data;
        if (selectedCompanyData) {
            filteredData = filteredData.filter(function(item) {
                // Filter by company name since we may not have CompanyId in response
                return item.CompanyName === selectedCompanyData.CompanyName;
            });
        }

        if (filteredData.length === 0) {
            tbody.append('<tr><td colspan="8" class="text-center">No records found for selected company</td></tr>');
            $('#pagination').empty();
            return;
        }

        $.each(filteredData, function (_, item) {
            var statusClass = getStatusClass(item.Status);
            var externalId = item.ExternalSyncId || '-';

            tbody.append(
                '<tr data-id="' + item.Id + '">' +
                '<td>' + item.Id + '</td>' +
                '<td>' + escapeHtml(item.EmployeeName) + '</td>' +
                '<td>' + escapeHtml(item.CompanyName) + '</td>' +
                '<td>' + escapeHtml(item.ToolName) + '</td>' +
                '<td>' + formatDate(item.FromDate) + '</td>' +
                '<td>' + formatDate(item.ToDate) + '</td>' +
                '<td><span class="status-badge ' + statusClass + '">' + item.Status + '</span></td>' +
                '<td>' + externalId + '</td>' +
                '</tr>'
            );
        });

        renderPagination(res.Data.TotalRecords, res.Data.Page, res.Data.PageSize);
    }).fail(function (xhr) {
        if (xhr.status === 401) {
            window.location.href = APP.baseUrl + 'Auth/Login';
        } else {
            tbody.html('<tr><td colspan="8" class="text-center text-danger">Failed to load requests</td></tr>');
        }
    });
}

function showCreateModal() {
    if (!selectedCompanyData) {
        Swal.fire('Error', 'Please select a company database first.', 'error');
        return;
    }

    $('#employeeId').val('');
    $('#fromDate').val('');
    $('#toDate').val('');

    var modal = new bootstrap.Modal(document.getElementById('createSyncModal'));
    modal.show();
}

function createSynchronization() {
    var employeeId = parseInt($('#employeeId').val());
    var fromDate = $('#fromDate').val();
    var toDate = $('#toDate').val();

    if (!employeeId) {
        Swal.fire('Error', 'Please select an employee.', 'error');
        return;
    }

    if (!fromDate || !toDate) {
        Swal.fire('Error', 'Please select date range.', 'error');
        return;
    }

    if (!selectedCompanyData) {
        Swal.fire('Error', 'Please select a company database.', 'error');
        return;
    }

    var data = {
        EmployeeId: employeeId,
        CompanyId: selectedCompanyData.CompanyId,
        ToolId: selectedCompanyData.ToolId,
        FromDate: fromDate,
        ToDate: toDate
    };

    $('#submitSyncBtn').prop('disabled', true).text('Creating...');

    $.ajax({
        url: APP.baseUrl + 'Attandance/CreateRequest',
        type: 'POST',
        data: data,
        success: function (res) {
            $('#submitSyncBtn').prop('disabled', false).text('Create Request');

            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message || res.Errors[0], 'error');
            } else {
                Swal.fire('Success', res.Message || 'Request created successfully!', 'success');
                bootstrap.Modal.getInstance(document.getElementById('createSyncModal')).hide();
                loadSynchronizations(1);
            }
        },
        error: function (xhr) {
            $('#submitSyncBtn').prop('disabled', false).text('Create Request');

            if (xhr.status === 401) {
                window.location.href = APP.baseUrl + 'Auth/Login';
            } else {
                Swal.fire('Error', 'Failed to create request. Please try again.', 'error');
            }
        }
    });
}

function applyFilters() {
    currentFilters = {
        employeeId: $('#filterEmployee').val(),
        fromDate: $('#filterFromDate').val(),
        toDate: $('#filterToDate').val(),
        status: $('#filterStatus').val()
    };
    loadSynchronizations(1);
}

function clearFilters() {
    $('#filterEmployee').val('');
    $('#filterFromDate').val('');
    $('#filterToDate').val('');
    $('#filterStatus').val('');
    currentFilters = {};
    loadSynchronizations(1);
}

function updateStatusesOnly() {
    if (!selectedCompanyData) return;

    var ids = [];
    $('#syncTableBody tr[data-id]').each(function () {
        ids.push(parseInt($(this).data('id')));
    });

    if (ids.length === 0) return;

    $.post(APP.baseUrl + 'Attandance/GetStatusesByIds', { ids: ids }, function (res) {
        if (!res.Data) return;

        $.each(res.Data, function (_, item) {
            var $row = $('#syncTableBody tr[data-id="' + item.Id + '"]');
            var $badge = $row.find('.status-badge');
            var newClass = getStatusClass(item.Status);

            if ($badge.text().trim() !== item.Status) {
                $badge.removeClass('status-pending status-completed status-failed status-nr status-ip status-cp')
                    .addClass(newClass)
                    .text(item.Status);
            }
        });
    });
}

function formatDate(value) {
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
    return y + '-' + m + '-' + d;
}

function getStatusClass(status) {
    var classMap = {
        'Pending': 'status-pending',
        'Completed': 'status-completed',
        'Failed': 'status-failed',
        'NR': 'status-nr',
        'IP': 'status-ip',
        'CP': 'status-cp'
    };
    return classMap[status] || '';
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
        pagination.append('<li class="page-item"><a class="page-link" href="#" onclick="loadSynchronizations(' + (page - 1) + '); return false;">Previous</a></li>');
    }

    for (var i = 1; i <= totalPages; i++) {
        var activeClass = i === page ? 'active' : '';
        pagination.append('<li class="page-item ' + activeClass + '"><a class="page-link" href="#" onclick="loadSynchronizations(' + i + '); return false;">' + i + '</a></li>');
    }

    if (page < totalPages) {
        pagination.append('<li class="page-item"><a class="page-link" href="#" onclick="loadSynchronizations(' + (page + 1) + '); return false;">Next</a></li>');
    }
}
