// Load user menu
$(function () {
    $.get(APP.baseUrl + 'Auth/CurrentUser', function (res) {
        var menu = $('#userNavMenu');
        if (res.Data) {
            var adminLink = res.Data.Role === 'ADMIN'
                ? '<li class="nav-item">' +
                '<a class="nav-link" href="' + APP.baseUrl + 'AdminDashboard">Admin Panel</a>' +
                '</li>'
                : '';
            menu.html(
                adminLink +
                '<li class="nav-item dropdown">' +
                '<a class="nav-link dropdown-toggle" href="#" data-bs-toggle="dropdown">' +
                res.Data.Name +
                '</a>' +
                '<ul class="dropdown-menu dropdown-menu-end">' +
                '<li>' +
                '<a class="dropdown-item" href="#" onclick="logout(); return false;">Logout</a>' +
                '</li>' +
                '</ul>' +
                '</li>'
            );
        } else {
            menu.html(
                '<li class="nav-item">' +
                '<a class="nav-link" href="' + APP.baseUrl + 'Auth/Login">Sign In</a>' +
                '</li>' +
                '<li class="nav-item">' +
                '<a class="nav-link" href="' + APP.baseUrl + 'Auth/Register">Sign Up</a>' +
                '</li>'
            );
        }
    }).fail(function (xhr) {
        if (xhr.status === 401) {
            window.location.href = APP.baseUrl + 'Auth/Login';
        }
    });
});