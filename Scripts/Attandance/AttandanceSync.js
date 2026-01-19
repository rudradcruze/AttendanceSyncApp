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
        var selectedId = parseInt($(this).val());
        
        if (selectedId) {
            // Find the selected company data by ID
            selectedCompanyData = companyDatabases.find(function(item) {
                return item.DatabaseAssignmentId === selectedId;
            });

            if (selectedCompanyData) {
                $('#syncContent').removeClass('d-none');
                $('#noAccessMessage').addClass('d-none');

                // Update modal with selected company info
                $('#selectedCompanyName').val(selectedCompanyData.CompanyName);
                $('#selectedCompanyId').val(selectedCompanyData.CompanyId);
                $('#selectedToolId').val(selectedCompanyData.ToolId);
                $('#selectedDbConfigId').val(selectedCompanyData.DatabaseConfigurationId);

                loadEmployees();
                loadSynchronizations(1);
                return;
            }
        }
        
        // Default / Reset
        selectedCompanyData = null;
        $('#syncContent').addClass('d-none');
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
        var $select = $('#companyDatabaseSelect');
        
        // Clear existing options except the first one
        $select.find('option:not(:first)').remove();

        if (res.Data && Array.isArray(res.Data) && res.Data.length > 0) {
            companyDatabases = res.Data;
            
            for (var i = 0; i < res.Data.length; i++) {
                var item = res.Data[i];
                var optionText = (item.CompanyName || '') + ' - ' + (item.DatabaseName || '');
                var $option = $('<option>', { 
                    value: item.DatabaseAssignmentId,
                    text: optionText
                });
                $select.append($option);
            }
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
        url: APP.baseUrl + 'Attandance/CreateOnTheFlySynchronization',
        type: 'POST',
        data: data,
        success: function (res) {
            $('#submitSyncBtn').prop('disabled', false).text('Create');

            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message || res.Errors[0], 'error');
            } else {
                Swal.fire('Success', res.Message || 'Synchronization created successfully!', 'success');
                // Reset form
                $('#fromDate').val('');
                $('#toDate').val('');
                loadSynchronizations(1);
            }
        },
        error: function (xhr) {
            $('#submitSyncBtn').prop('disabled', false).text('Create');

            if (xhr.status === 401) {
                window.location.href = APP.baseUrl + 'Auth/Login';
            } else {
                Swal.fire('Error', 'Failed to create synchronization. Please try again.', 'error');
            }
        }
    });
}

function setSortDirection(dir) {
    $('#sortDirection').val(dir);
    $('.sort-dir-btn').removeClass('active');
    $('.sort-dir-btn[data-value="' + dir + '"]').addClass('active');
}

function loadSynchronizations(page) {
    currentPage = page;
    var tbody = $('#syncTableBody');
    tbody.html('<tr><td colspan="6" class="text-center">Loading...</td></tr>');

    if (!selectedCompanyData) {
        tbody.html('<tr><td colspan="6" class="text-center">Select a company database to view requests</td></tr>');
        return;
    }

    $.get(APP.baseUrl + 'Attandance/GetMyRequests', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        tbody.empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="6" class="text-center text-danger">' + res.Message + '</td></tr>');
            $('#pagination').empty();
            return;
        }

        if (!res.Data || !res.Data.Data || !res.Data.Data.length) {
            tbody.append('<tr><td colspan="6" class="text-center">No records found</td></tr>');
            $('#pagination').empty();
            return;
        }

        // Filter by selected company if needed
        var filteredData = res.Data.Data;
        if (selectedCompanyData) {
            filteredData = filteredData.filter(function(item) {
                return item.CompanyName === selectedCompanyData.CompanyName;
            });
        }

        if (filteredData.length === 0) {
            tbody.append('<tr><td colspan="6" class="text-center">No records found for selected company</td></tr>');
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
            tbody.html('<tr><td colspan="6" class="text-center text-danger">Failed to load requests</td></tr>');
        }
    });
}
