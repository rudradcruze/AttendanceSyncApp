/* ============================
   Attandance Synchronization
   Updated with authentication
============================ */
var currentPage = 1;
var pageSize = 20;
var currentUser = null;

/* ============================
   Document Ready
============================ */
$(function () {
    loadCurrentUser();
    loadDropdowns();
    loadSynchronizations(1);
    setInterval(updateStatusesOnly, 2000);

    // Form submission
    $('#syncForm').on('submit', function (e) {
        e.preventDefault();
        createSynchronization();
    });

    // Sync from and to date when from date changes
    $('#fromDate').on('change', function () {
        $('#toDate').val($(this).val());
    });
});

/* ============================
   Load Current User
============================ */
function loadCurrentUser() {
    $.get(APP.baseUrl + 'Auth/CurrentUser', function (res) {
        if (res.Data) {
            currentUser = res.Data;
            $('#userEmail').val(currentUser.Email);
        }
    });
}

/* ============================
   Load Dropdowns
============================ */
function loadDropdowns() {
    // Load Users
    $.get(APP.baseUrl + 'Attandance/GetUsers', function (res) {
        var select = $('#userId');
        select.find('option:not(:first)').remove();
        if (res.Data) {
            $.each(res.Data, function (_, user) {
                select.append('<option value="' + user.Id + '">' + user.Name + ' (' + user.Email + ')</option>');
            });
        }
    });

    // Load Companies
    $.get(APP.baseUrl + 'Attandance/GetCompanies', function (res) {
        var select = $('#companyId');
        select.find('option:not(:first)').remove();
        if (res.Data) {
            $.each(res.Data, function (_, company) {
                select.append('<option value="' + company.Id + '">' + company.Name + '</option>');
            });
        }
    });

    // Load Tools
    $.get(APP.baseUrl + 'Attandance/GetTools', function (res) {
        var select = $('#toolId');
        select.find('option:not(:first)').remove();
        if (res.Data) {
            $.each(res.Data, function (_, tool) {
                select.append('<option value="' + tool.Id + '">' + tool.Name + '</option>');
            });
        }
    });
}

/* ============================
   Load Synchronizations
============================ */
function loadSynchronizations(page) {
    currentPage = page;
    $.get(APP.baseUrl + 'Attandance/GetSynchronizationsPaged', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#syncTableBody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="7" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="7">No records found</td></tr>');
            renderPagination(0, 1, pageSize);
            return;
        }

        $.each(data.Data, function (_, item) {
            var statusClass = getStatusClass(item.Status);
            var statusText = getStatusText(item.Status);

            tbody.append(
                '<tr data-id="' + item.Id + '">' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.UserName + '</td>' +
                '<td>' + item.CompanyName + '</td>' +
                '<td>' + item.ToolName + '</td>' +
                '<td>' + formatDate(item.FromDate) + '</td>' +
                '<td>' + formatDate(item.ToDate) + '</td>' +
                '<td><span class="status-badge ' + statusClass + '">' + statusText + '</span></td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

/* ============================
   Create Synchronization
============================ */
function createSynchronization() {
    var data = {
        UserId: parseInt($('#userId').val()),
        CompanyId: parseInt($('#companyId').val()),
        ToolId: parseInt($('#toolId').val()),
        FromDate: $('#fromDate').val(),
        ToDate: $('#toDate').val()
    };

    if (!data.UserId || !data.CompanyId || !data.ToolId) {
        showMessage('Please select Employee, Company, and Tool.', 'danger');
        return;
    }

    $.ajax({
        url: APP.baseUrl + 'Attandance/CreateSynchronization',
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                showMessage(res.Message || res.Errors[0], 'danger');
            } else {
                showMessage(res.Message || 'Request created successfully!', 'success');
                // Reset form
                $('#userId').val('');
                $('#companyId').val('');
                $('#toolId').val('');
                $('#fromDate').val('');
                $('#toDate').val('');
                // Reload data
                loadSynchronizations(1);
            }
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = APP.baseUrl + 'Auth/Login';
            } else {
                showMessage('Failed to create request. Please try again.', 'danger');
            }
        }
    });
}

/* ============================
   Update Statuses (Polling)
============================ */
function updateStatusesOnly() {
    var ids = [];
    $('#syncTableBody tr[data-id]').each(function () {
        ids.push(parseInt($(this).data('id')));
    });

    if (ids.length === 0) return;

    $.post(APP.baseUrl + 'Attandance/GetStatusesByIds', { ids: ids }, function (res) {
        if (res.Errors && res.Errors.length > 0) return;
        if (!res.Data) return;

        $.each(res.Data, function (_, item) {
            var $row = $('#syncTableBody tr[data-id="' + item.Id + '"]');
            var $badge = $row.find('.status-badge');
            var newClass = getStatusClass(item.Status);
            var newText = getStatusText(item.Status);

            if ($badge.text().trim() !== newText) {
                $badge.removeClass('status-nr status-ip status-cp')
                    .addClass(newClass)
                    .text(newText);
            }
        });
    });
}

/* ============================
   Helper Functions
============================ */
function formatDate(value) {
    if (!value) return '-';

    // Handle /Date()/ format
    if (typeof value === 'string' && value.indexOf('/Date(') > -1) {
        var timestamp = parseInt(value.replace('/Date(', '').replace(')/', ''));
        var date = new Date(timestamp);
        return date.toLocaleDateString();
    }

    // Handle ISO format
    var date = new Date(value);
    if (isNaN(date.getTime())) return '-';
    return date.toLocaleDateString();
}

function getStatusText(status) {
    var statusMap = {
        'NR': 'New Request',
        'IP': 'In Progress',
        'CP': 'Completed'
    };
    return statusMap[status] || status;
}

function getStatusClass(status) {
    var classMap = {
        'NR': 'status-nr',
        'IP': 'status-ip',
        'CP': 'status-cp'
    };
    return classMap[status] || '';
}

function showMessage(msg, type) {
    var $msg = $('#message');
    $msg.removeClass('d-none alert-success alert-danger alert-warning alert-info')
        .addClass('alert alert-' + type)
        .text(msg)
        .fadeIn();

    setTimeout(function () {
        $msg.fadeOut(function () {
            $(this).addClass('d-none');
        });
    }, 5000);
}

function renderPagination(totalRecords, page, pageSize) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#pagination').empty();

    if (totalPages <= 1) return;

    // Previous button
    if (page > 1) {
        pagination.append('<li class="page-item"><a class="page-link" href="#" onclick="loadSynchronizations(' + (page - 1) + '); return false;">Previous</a></li>');
    }

    // Page numbers
    for (var i = 1; i <= totalPages; i++) {
        var activeClass = i === page ? 'active' : '';
        pagination.append('<li class="page-item ' + activeClass + '"><a class="page-link" href="#" onclick="loadSynchronizations(' + i + '); return false;">' + i + '</a></li>');
    }

    // Next button
    if (page < totalPages) {
        pagination.append('<li class="page-item"><a class="page-link" href="#" onclick="loadSynchronizations(' + (page + 1) + '); return false;">Next</a></li>');
    }
}
