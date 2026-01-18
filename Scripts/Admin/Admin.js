$(function () {
    $.get(APP.baseUrl + 'Auth/CurrentUser', function (res) {
        if (res.Data) {
            $('#adminUserInfo').html('<span>' + res.Data.Name + '</span>');
        }
    }).fail(function (xhr) {
        if (xhr.status === 401 || (xhr.responseJSON && xhr.responseJSON.Message)) {
            window.location.href = APP.baseUrl + 'Auth/Login';
        }
    });
});