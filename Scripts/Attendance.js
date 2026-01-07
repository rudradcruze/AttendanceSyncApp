$(function () {

    loadSynchronizations();
    setInterval(updateStatusesOnly, 2000);

    $('#syncForm').submit(function (e) {
        e.preventDefault();

        $.post('/Attendance/CreateSynchronization', {
            fromDate: $('#fromDate').val(),
            toDate: $('#toDate').val()
        }).done(function (res) {
            showMessage(res.message, res.success ? 'success' : 'danger');
            if (res.success) {
                $('#syncForm')[0].reset();
                loadSynchronizations();
            }
        });
    });
});

function loadSynchronizations() {
    $.get('/Attendance/GetSynchronizations', function (data) {

        var tbody = $('#syncTableBody').empty();

        if (!data.length) {
            tbody.append('<tr><td colspan="5" class="text-center">No records</td></tr>');
            return;
        }

        $.each(data, function (_, item) {
            tbody.append(`
                <tr data-id="${item.Id}">
                    <td>${item.Id}</td>
                    <td>${item.FromDate}</td>
                    <td>${item.ToDate}</td>
                    <td>${item.CompanyName}</td>
                    <td class="status-cell">
                        <span class="status-badge ${getStatusClass(item.Status)}">
                            ${getStatusText(item.Status)}
                        </span>
                    </td>
                </tr>
            `);
        });
    });
}

function updateStatusesOnly() {
    $.get('/Attendance/GetSynchronizations', function (data) {

        $.each(data, function (_, item) {
            var row = $('tr[data-id="' + item.Id + '"]');
            if (!row.length) return;

            var badge = row.find('.status-badge');
            var newText = getStatusText(item.Status);

            if (badge.text() !== newText) {
                badge
                    .removeClass('status-nr status-ip status-cp')
                    .addClass(getStatusClass(item.Status))
                    .text(newText);
            }
        });
    });
}

function getStatusText(s) {
    return s === 'NR' ? 'New Request' :
        s === 'IP' ? 'In Progress' :
            s === 'CP' ? 'Completed' : s;
}

function getStatusClass(s) {
    return s === 'NR' ? 'status-nr' :
        s === 'IP' ? 'status-ip' :
            s === 'CP' ? 'status-cp' : '';
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
