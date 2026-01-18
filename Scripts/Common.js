function logout() {
    Swal.fire({
        title: 'Logout',
        text: 'Are you sure you want to logout?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, logout'
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(APP.baseUrl + 'Auth/Logout', function () {
                window.location.href = APP.baseUrl + '/Auth/Login';
            });
        }
    });
}

// Global date formatter
function formatDate(value) {
    if (!value) return 'N/A';
    var date = new Date(parseInt(value.replace('/Date(', '').replace(')/', '')));
    if (isNaN(date.getTime())) {
        date = new Date(value);
    }
    if (isNaN(date.getTime())) return 'N/A';
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

function formatDateTime(value) {
    if (!value) return 'N/A';
    var date = new Date(parseInt(value.replace('/Date(', '').replace(')/', '')));
    if (isNaN(date.getTime())) {
        date = new Date(value);
    }
    if (isNaN(date.getTime())) return 'N/A';
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: true
    });
}

// Global error handler for AJAX
$(document).ajaxError(function (event, xhr) {
    if (xhr.status === 401) {
        Swal.fire({
            title: 'Session Expired',
            text: 'Your session has expired. Please login again.',
            icon: 'warning',
            confirmButtonText: 'Login'
        }).then(() => {
            window.location.href = APP.baseUrl + 'Auth/Login';
        });
    }
});