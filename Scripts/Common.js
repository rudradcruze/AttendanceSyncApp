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
                window.location.href = APP.baseUrl + 'Auth/Login';
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

// ============================
// Pagination Renderer (Reusable)
// ============================
// Usage: renderPagination('#paginationContainer', totalRecords, currentPage, pageSize, loadDataFunction)
function renderPagination(container, totalRecords, page, pageSize, callback) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var $pagination = $(container).empty();

    if (totalPages <= 1) return;

    // Previous button
    if (page > 1) {
        $pagination.append(
            '<li class="page-item">' +
            '<a href="javascript:void(0)" class="page-link" data-page="' + (page - 1) + '">Previous</a>' +
            '</li>'
        );
    }

    // Page numbers with ellipsis for large page counts
    var startPage = Math.max(1, page - 2);
    var endPage = Math.min(totalPages, page + 2);

    if (startPage > 1) {
        $pagination.append('<li class="page-item"><a href="javascript:void(0)" class="page-link" data-page="1">1</a></li>');
        if (startPage > 2) {
            $pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
        }
    }

    for (var i = startPage; i <= endPage; i++) {
        $pagination.append(
            '<li class="page-item ' + (i === page ? 'active' : '') + '">' +
            '<a href="javascript:void(0)" class="page-link" data-page="' + i + '">' + i + '</a>' +
            '</li>'
        );
    }

    if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
            $pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
        }
        $pagination.append('<li class="page-item"><a href="javascript:void(0)" class="page-link" data-page="' + totalPages + '">' + totalPages + '</a></li>');
    }

    // Next button
    if (page < totalPages) {
        $pagination.append(
            '<li class="page-item">' +
            '<a href="javascript:void(0)" class="page-link" data-page="' + (page + 1) + '">Next</a>' +
            '</li>'
        );
    }

    // Click handler
    $pagination.find('.page-link[data-page]').on('click', function () {
        var targetPage = parseInt($(this).data('page'));
        if (callback && typeof callback === 'function') {
            callback(targetPage);
        }
    });
}

// ============================
// Status Utilities
// ============================
function getStatusText(status) {
    switch (status) {
        case 'NR': return 'New Request';
        case 'IP': return 'In Progress';
        case 'CP': return 'Completed';
        case 'RR': return 'Rejected';
        case 'CN': return 'Cancelled';
        default: return status || 'Unknown';
    }
}

function getStatusClass(status) {
    switch (status) {
        case 'NR': return 'status-nr';
        case 'IP': return 'status-ip';
        case 'CP': return 'status-cp';
        case 'RR': return 'status-rr';
        case 'CN': return 'status-cn';
        default: return '';
    }
}

function getStatusBadge(status) {
    return '<span class="status-badge ' + getStatusClass(status) + '">' + getStatusText(status) + '</span>';
}

// ============================
// HTML Escaping
// ============================
function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}

// ============================
// Message Display
// ============================
function showMessage(message, type, containerId) {
    var $container = containerId ? $(containerId) : $('#messageContainer');
    var alertClass = type === 'success' ? 'alert-success'
                   : type === 'error' ? 'alert-danger'
                   : type === 'warning' ? 'alert-warning'
                   : 'alert-info';

    var html = '<div class="alert ' + alertClass + ' alert-dismissible fade show" role="alert">' +
               escapeHtml(message) +
               '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
               '</div>';

    $container.html(html);

    // Auto-dismiss after 5 seconds
    setTimeout(function () {
        $container.find('.alert').alert('close');
    }, 5000);
}

// ============================
// Dropdown Loading Helper
// ============================
function loadDropdownOptions(selectId, url, valueProp, labelProp, placeholder) {
    var $select = $(selectId);
    $select.find('option:not(:first)').remove();

    if (placeholder) {
        $select.find('option:first').text(placeholder);
    }

    $.get(url, function (res) {
        if (res.Data && Array.isArray(res.Data)) {
            $.each(res.Data, function (_, item) {
                $select.append($('<option>', {
                    value: item[valueProp],
                    text: item[labelProp]
                }));
            });
        }
    });
}