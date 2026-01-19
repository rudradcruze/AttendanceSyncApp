/* ============================
   Google Sign-In Handler
============================ */
function handleGoogleSignIn(response) {
    $.ajax({
        url: APP.baseUrl + 'Auth/GoogleSignIn',
        type: 'POST',
        data: {
            idToken: response.credential
        },
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                showMessage(res.Message || res.Errors[0], 'danger');
            } else if (res.Data) {
                showMessage('Login successful! Redirecting...', 'success');
                setTimeout(function () {
                    if (res.Data.Role === 'ADMIN') {
                        window.location.href = APP.baseUrl + 'AdminDashboard';
                    } else {
                        window.location.href = APP.baseUrl;
                    }
                }, 1000);
            }
        },
        error: function () {
            showMessage('Login failed. Please try again.', 'danger');
        }
    });
}

/* ============================
   Google Sign-Up Handler
============================ */
function handleGoogleSignUp(response) {
    $.ajax({
        url: APP.baseUrl + 'Auth/GoogleSignUp',
        type: 'POST',
        data: {
            idToken: response.credential
        },
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                showMessage(res.Message || res.Errors[0], 'danger');
            } else if (res.Data) {
                showMessage('Registration successful! Redirecting...', 'success');
                setTimeout(function () {
                    window.location.href = APP.baseUrl;
                }, 1000);
            }
        },
        error: function () {
            showMessage('Registration failed. Please try again.', 'danger');
        }
    });
}

/* ============================
   Admin Login Form Handler
============================ */
$(function () {
    $('#adminLoginForm').on('submit', function (e) {
        e.preventDefault();

        var email = $('#adminEmail').val();
        var password = $('#adminPassword').val();

        $.ajax({
            url: APP.baseUrl + 'Auth/AdminLogin',
            type: 'POST',
            data: {
                email: email,
                password: password
            },
            success: function (res) {
                if (res.Errors && res.Errors.length > 0) {
                    showMessage(res.Message || res.Errors[0], 'danger');
                } else if (res.Data) {
                    showMessage('Login successful! Redirecting...', 'success');
                    setTimeout(function () {
                        window.location.href = APP.baseUrl + 'AdminDashboard';
                    }, 1000);
                }
            },
            error: function () {
                showMessage('Login failed. Please try again.', 'danger');
            }
        });
    });
});

/* ============================
   Logout Handler
============================ */
function logout() {
    $.ajax({
        url: APP.baseUrl + 'Auth/Logout',
        type: 'POST',
        success: function () {
            window.location.href = APP.baseUrl + 'Auth/Login';
        },
        error: function () {
            window.location.href = APP.baseUrl + 'Auth/Login';
        }
    });
}

/* ============================
   Helper Functions
============================ */
function showMessage(msg, type) {
    var $msg = $('#message');
    $msg.removeClass('d-none alert-success alert-danger alert-warning alert-info')
        .addClass('alert alert-' + type)
        .text(msg)
        .fadeIn();

    if (type === 'success') {
        // Don't auto-hide success messages (will redirect)
    } else {
        setTimeout(function () {
            $msg.fadeOut();
        }, 5000);
    }
}
