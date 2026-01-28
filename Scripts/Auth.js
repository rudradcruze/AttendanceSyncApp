/* ============================
   Google Sign-In Handler
============================ */
function handleGoogleSignIn(response) {
    // Show spinner and hide Google button
    $('#googleSignInContainer').hide();
    $('#googleSignInSpinner').show();

    $.ajax({
        url: APP.baseUrl + 'Auth/GoogleSignIn',
        type: 'POST',
        data: {
            idToken: response.credential
        },
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                // Hide spinner and show Google button on error
                $('#googleSignInSpinner').hide();
                $('#googleSignInContainer').show();
                showMessage(res.Message || res.Errors[0], 'danger');
            } else if (res.Data) {
                showMessage('Login successful! Redirecting...', 'success');
                // Keep spinner visible during redirect
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
            // Hide spinner and show Google button on error
            $('#googleSignInSpinner').hide();
            $('#googleSignInContainer').show();
            showMessage('Login failed. Please try again.', 'danger');
        }
    });
}

/* ============================
   Google Sign-Up Handler
============================ */
function handleGoogleSignUp(response) {
    // Show spinner and hide Google button
    $('#googleSignUpContainer').hide();
    $('#googleSignUpSpinner').show();

    $.ajax({
        url: APP.baseUrl + 'Auth/GoogleSignUp',
        type: 'POST',
        data: {
            idToken: response.credential
        },
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                // Hide spinner and show Google button on error
                $('#googleSignUpSpinner').hide();
                $('#googleSignUpContainer').show();
                showMessage(res.Message || res.Errors[0], 'danger');
            } else if (res.Data) {
                showMessage('Registration successful! Redirecting...', 'success');
                // Keep spinner visible during redirect
                setTimeout(function () {
                    window.location.href = APP.baseUrl;
                }, 1000);
            }
        },
        error: function () {
            // Hide spinner and show Google button on error
            $('#googleSignUpSpinner').hide();
            $('#googleSignUpContainer').show();
            showMessage('Registration failed. Please try again.', 'danger');
        }
    });
}

/* ============================
   Email Login Form Handler
============================ */
$(function () {
    $('#emailLoginForm').on('submit', function (e) {
        e.preventDefault();

        var email = $('#loginEmail').val();
        var password = $('#loginPassword').val();

        if (!email || !password) {
            showMessage('Please enter both email and password.', 'warning');
            return;
        }

        // Show spinner and disable button
        var $button = $('#loginButton');

        // Direct DOM manipulation to ensure it works
        $button.find('.button-text').hide();
        $button.find('.button-spinner').show();
        $button.prop('disabled', true);

        $.ajax({
            url: APP.baseUrl + 'Auth/UserLogin',
            type: 'POST',
            data: {
                email: email,
                password: password
            },
            success: function (res) {
                if (res.Errors && res.Errors.length > 0) {
                    // Hide spinner and enable button on error
                    $button.find('.button-text').show();
                    $button.find('.button-spinner').hide();
                    $button.prop('disabled', false);
                    showMessage(res.Message || res.Errors[0], 'danger');
                } else if (res.Data) {
                    showMessage('Login successful! Redirecting...', 'success');
                    // Keep spinner visible during redirect
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
                // Hide spinner and enable button on error
                $button.find('.button-text').show();
                $button.find('.button-spinner').hide();
                $button.prop('disabled', false);
                showMessage('Login failed. Please try again.', 'danger');
            }
        });
    });
});

/* ============================
   Email Registration Form Handler
============================ */
$(function () {
    $('#emailRegisterForm').on('submit', function (e) {
        e.preventDefault();

        var name = $('#registerName').val().trim();
        var email = $('#registerEmail').val().trim();
        var password = $('#registerPassword').val();
        var confirmPassword = $('#registerConfirmPassword').val();

        // Client-side validation
        if (!name || !email || !password || !confirmPassword) {
            showMessage('Please fill in all fields.', 'warning');
            return;
        }

        if (name.length < 2) {
            showMessage('Name must be at least 2 characters.', 'warning');
            return;
        }

        if (password.length < 8) {
            showMessage('Password must be at least 8 characters.', 'warning');
            return;
        }

        if (password !== confirmPassword) {
            showMessage('Passwords do not match.', 'warning');
            return;
        }

        // Show spinner and disable button
        var $button = $('#registerButton');

        // Direct DOM manipulation to ensure it works
        $button.find('.button-text').hide();
        $button.find('.button-spinner').show();
        $button.prop('disabled', true);

        $.ajax({
            url: APP.baseUrl + 'Auth/Register',
            type: 'POST',
            data: {
                name: name,
                email: email,
                password: password,
                confirmPassword: confirmPassword
            },
            success: function (res) {
                if (res.Errors && res.Errors.length > 0) {
                    // Hide spinner and enable button on error
                    $button.find('.button-text').show();
                    $button.find('.button-spinner').hide();
                    $button.prop('disabled', false);
                    showMessage(res.Message || res.Errors[0], 'danger');
                } else if (res.Data) {
                    showMessage('Registration successful! Redirecting...', 'success');
                    // Keep spinner visible during redirect
                    setTimeout(function () {
                        window.location.href = APP.baseUrl;
                    }, 1000);
                }
            },
            error: function () {
                // Hide spinner and enable button on error
                $button.find('.button-text').show();
                $button.find('.button-spinner').hide();
                $button.prop('disabled', false);
                showMessage('Registration failed. Please try again.', 'danger');
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

/**
 * Toggles the loading state of a button (shows/hides spinner and disables/enables button).
 * @param {jQuery} $button - The button element to update.
 * @param {boolean} isLoading - True to show spinner and disable button, false to hide spinner and enable button.
 */
function setButtonLoading($button, isLoading) {
    if (isLoading) {
        // Show spinner, hide text, disable button
        $button.find('.button-text').hide();
        $button.find('.button-spinner').show();
        $button.prop('disabled', true);
    } else {
        // Hide spinner, show text, enable button
        $button.find('.button-text').show();
        $button.find('.button-spinner').hide();
        $button.prop('disabled', false);
    }
}
