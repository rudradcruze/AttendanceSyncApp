/* ============================
   Global State
============================ */
var currentPage = 1;
var pageSize = 20;
var selectedCompanyData = null;
var companyDatabases = [];

// DEFAULT SORT (Initial Load)
var sortColumn = 'ToDate';
var sortDirection = 'DESC';

/* ============================
   Document Ready
============================ */
$(function () {
    loadCompanyDatabases();

    // Company database change
    $('#companyDatabaseSelect').on('change', function () {
        var selectedId = parseInt($(this).val());

        if (selectedId) {
            // Find the selected company data by ID
            selectedCompanyData = companyDatabases.find(function (item) {
                return item.DatabaseAssignmentId === selectedId;
            });

            if (selectedCompanyData) {
                $('#syncContent').removeClass('d-none');
                $('#noAccessMessage').addClass('d-none');

                // Store selected company data in hidden fields
                $('#selectedCompanyId').val(selectedCompanyData.CompanyId);
                $('#selectedToolId').val(selectedCompanyData.ToolId);
                $('#selectedDbConfigId').val(selectedCompanyData.DatabaseConfigurationId);
                $('#employeeId').val(selectedCompanyData.EmployeeId);

                loadSynchronizations(1);
                return;
            }
        }

        // Default / Reset
        selectedCompanyData = null;
        $('#syncContent').addClass('d-none');
    });

    // Sync toDate when fromDate changes
    $('#fromDate').on('change', function () {
        $('#toDate').val($(this).val());
    });

    // Sort direction buttons
    $(document).on('click', '.sort-dir-btn', function () {
        $('.sort-dir-btn').removeClass('active btn-primary').addClass('btn-outline-secondary');
        $(this).addClass('active btn-primary').removeClass('btn-outline-secondary');
        $('#sortDirection').val($(this).data('value'));
    });

    // Poll statuses every 2 seconds
    setInterval(updateStatusesOnly, 2000);
});

/* ============================
   Load Company Databases
============================ */
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

/* ============================
   Load Paged Data
============================ */
function loadSynchronizations(page) {
    currentPage = page;
    var tbody = $('#syncTableBody');
    tbody.html('<tr><td colspan="5" class="text-center">Loading...</td></tr>');

    if (!selectedCompanyData) {
        tbody.html('<tr><td colspan="5" class="text-center">Select a company database to view requests</td></tr>');
        return;
    }

    $.get(APP.baseUrl + 'Attandance/GetMyRequests', {
        page: page,
        pageSize: pageSize,
        companyId: selectedCompanyData.CompanyId,
        sortColumn: sortColumn,
        sortDirection: sortDirection
    }, function (res) {
        tbody.empty();

        // Check for errors
        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="5" class="text-center text-danger">' + res.Message + '</td></tr>');
            $('#pagination').empty();
            return;
        }

        var pagedData = res.Data;
        if (!pagedData || !pagedData.Data || !pagedData.Data.length) {
            tbody.append('<tr><td colspan="5" class="text-center">No records found</td></tr>');
            $('#pagination').empty();
            return;
        }

        $.each(pagedData.Data, function (_, item) {
            tbody.append(
                '<tr data-id="' + item.Id + '">' +
                '<td>' + item.Id + '</td>' +
                '<td>' + formatDate(item.FromDate) + '</td>' +
                '<td>' + formatDate(item.ToDate) + '</td>' +
                '<td>' + escapeHtml(item.CompanyName) + '</td>' +
                '<td class="status-cell">' +
                '<span class="status-badge ' + getStatusClass(item.Status) + '">' +
                getStatusText(item.Status) +
                '</span>' +
                '</td>' +
                '</tr>'
            );
        });

        renderPagination(pagedData.TotalRecords, pagedData.Page, pagedData.PageSize);
    }).fail(function (xhr) {
        if (xhr.status === 401) {
            window.location.href = APP.baseUrl + 'Auth/Login';
        } else {
            tbody.html('<tr><td colspan="5" class="text-center text-danger">Failed to load requests</td></tr>');
        }
    });
}

/* ============================
   Pagination Renderer
============================ */
function renderPagination(totalRecords, page, pageSize) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#pagination').empty();

    if (totalPages <= 1) return;

    // Previous button
    if (page > 1) {
        pagination.append(
            '<li class="page-item">' +
            '<a href="javascript:void(0)" class="page-link" onclick="loadSynchronizations(' + (page - 1) + ')">Previous</a>' +
            '</li>'
        );
    }

    // Page numbers
    for (var i = 1; i <= totalPages; i++) {
        pagination.append(
            '<li class="page-item ' + (i === page ? 'active' : '') + '">' +
            '<a href="javascript:void(0)" class="page-link" onclick="loadSynchronizations(' + i + ')">' + i + '</a>' +
            '</li>'
        );
    }

    // Next button
    if (page < totalPages) {
        pagination.append(
            '<li class="page-item">' +
            '<a href="javascript:void(0)" class="page-link" onclick="loadSynchronizations(' + (page + 1) + ')">Next</a>' +
            '</li>'
        );
    }
}

/* ============================
   Sorting
============================ */
function applySorting() {
    sortColumn = $('#sortColumn').val();
    sortDirection = $('#sortDirection').val();
    loadSynchronizations(1);
}

/* ============================
   Status Polling (Every 2 sec)
============================ */
function updateStatusesOnly() {
    if (!selectedCompanyData) return;

    var ids = [];
    $('#syncTableBody tr[data-id]').each(function () {
        ids.push(parseInt($(this).data('id')));
    });

    if (ids.length === 0) return;

    // Use external status endpoint for external DB records
    $.post(APP.baseUrl + 'Attandance/GetExternalStatusesByIds', {
        companyId: selectedCompanyData.CompanyId,
        ids: ids
    }, function (res) {
        // Check for errors
        if (res.Errors && res.Errors.length > 0) {
            return;
        }

        if (!res.Data) return;

        $.each(res.Data, function (_, item) {
            var row = $('tr[data-id="' + item.Id + '"]');
            var badge = row.find('.status-badge');
            var newText = getStatusText(item.Status);

            if (badge.text().trim() !== newText) {
                badge
                    .removeClass('status-nr status-ip status-cp')
                    .addClass(getStatusClass(item.Status))
                    .text(newText);
            }
        });
    });
}

/* ============================
   Modal Functions
============================ */
function showCreateModal() {
    if (!selectedCompanyData) {
        Swal.fire('Error', 'Please select a company database first.', 'error');
        return;
    }

    // Reset fields
    $('#fromDate').val('');
    $('#toDate').val('');

    var modal = new bootstrap.Modal(document.getElementById('createSyncModal'));
    modal.show();
}

function createSynchronization() {
    var employeeId = parseInt($('#employeeId').val());
    var fromDate = $('#fromDate').val();
    var toDate = $('#toDate').val();

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
                bootstrap.Modal.getInstance(document.getElementById('createSyncModal')).hide();
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

/* ============================
   Helpers
============================ */

// Format /Date(...)/
function formatDate(value) {
    if (!value) return 'N/A';

    var date;

    if (typeof value === 'string' && value.indexOf('/Date(') === 0) {
        var ts = parseInt(value.replace(/\/Date\((\d+)\)\//, '$1'));
        date = new Date(ts);
    } else {
        date = new Date(value);
    }

    if (isNaN(date.getTime())) return 'Invalid Date';

    var y = date.getFullYear();
    var m = String(date.getMonth() + 1).padStart(2, '0');
    var d = String(date.getDate()).padStart(2, '0');

    return y + '-' + m + '-' + d;
}

function getStatusText(s) {
    return s === 'NR' ? 'New Request'
        : s === 'IP' ? 'In Progress'
            : s === 'CP' ? 'Completed'
                : s;
}

function getStatusClass(s) {
    return s === 'NR' ? 'status-nr'
        : s === 'IP' ? 'status-ip'
            : s === 'CP' ? 'status-cp'
                : '';
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}
