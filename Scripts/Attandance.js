/* ============================
   Global State
============================ */
var currentPage = 1;
var pageSize = 20;

/* ============================
   Document Ready
============================ */
$(function () {

    loadSynchronizations(1);

    // Poll only status every 2 seconds
    setInterval(updateStatusesOnly, 2000);

    // Sync toDate when fromDate changes
    $('#fromDate').on('change', function () {
        $('#toDate').val($(this).val());
    });

    // Create synchronization
    $('#syncForm').on('submit', function (e) {
        e.preventDefault();

        var fromDate = $('#fromDate').val();
        var toDate = $('#toDate').val();

        // Validate: ToDate must be same as FromDate
        if (fromDate !== toDate) {
            showMessage('To Date must be the same as From Date', 'danger');
            return;
        }

        $.post('/Attandance/CreateSynchronization', {
            fromDate: fromDate,
            toDate: toDate
        }).done(function (res) {
            var hasErrors = res.Errors && res.Errors.length > 0;
            showMessage(res.Message, hasErrors ? 'danger' : 'success');

            if (!hasErrors) {
                $('#syncForm')[0].reset();
                loadSynchronizations(currentPage);
            }
        });
    });
});

/* ============================
   Load Paged Data
============================ */
function loadSynchronizations(page) {
    currentPage = page;

    $.get('/Attandance/GetSynchronizationsPaged', {
        page: page,
        pageSize: pageSize
    }, function (res) {

        var tbody = $('#syncTableBody').empty();

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
            tbody.append(`
                <tr data-id="${item.Id}">
                    <td>${item.Id}</td>
                    <td>${formatDate(item.FromDate)}</td>
                    <td>${formatDate(item.ToDate)}</td>
                    <td>${item.CompanyName}</td>
                    <td class="status-cell">
                        <span class="status-badge ${getStatusClass(item.Status)}">
                            ${getStatusText(item.Status)}
                        </span>
                    </td>
                </tr>
            `);
        });

        renderPagination(pagedData.TotalRecords, pagedData.Page, pagedData.PageSize);
    });
}

/* ============================
   Pagination Renderer
============================ */
function renderPagination(totalRecords, page, pageSize) {

    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#pagination').empty();

    if (totalPages <= 1) return;

    for (var i = 1; i <= totalPages; i++) {
        pagination.append(`
            <li class="page-item ${i === page ? 'active' : ''}">
                <a href="javascript:void(0)" class="page-link" onclick="loadSynchronizations(${i})">${i}</a>
            </li>
        `);
    }
}

/* ============================
   Status Polling (Every 2 sec)
============================ */
function updateStatusesOnly() {
    var ids = [];

    $('tr[data-id]').each(function () {
        ids.push($(this).data('id'));
    });

    if (!ids.length) return;

    // Send only the IDs we need status for
    $.post('/Attandance/GetStatusesByIds', { ids: ids }, function (res) {

        // Check for errors
        if (res.Errors && res.Errors.length > 0) {
            return;
        }

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

    return `${y}-${m}-${d}`;
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

function showMessage(msg, type) {
    $('#message')
        .removeClass()
        .addClass('alert alert-' + type)
        .text(msg)
        .fadeIn()
        .delay(3000)
        .fadeOut();
}
