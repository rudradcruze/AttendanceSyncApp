/* User Dashboard - Tool Cards */

$(function () {
    loadUserTools();
});

function loadUserTools() {
    $.get(APP.baseUrl + 'Attandance/GetMyTools', function (res) {
        var container = $('#toolsContainer').empty();

        if (res.Errors && res.Errors.length > 0) {
            container.html('<div class="col-12 text-center text-danger">' + res.Message + '</div>');
            return;
        }

        if (!res.Data || res.Data.length === 0) {
            container.addClass('d-none');
            $('#noToolsMessage').removeClass('d-none');
            return;
        }

        $.each(res.Data, function (_, tool) {
            var cardClass = tool.IsImplemented ? 'implemented' : 'under-development';
            var icon = getToolIcon(tool.ToolName);
            var badge = tool.IsImplemented ? '' : '<span class="badge-development">Coming Soon</span>';

            var card = $(
                '<div class="col-md-4 col-lg-3 mb-4">' +
                    '<div class="card tool-card shadow ' + cardClass + '" ' +
                         'data-tool-id="' + tool.ToolId + '" ' +
                         'data-tool-name="' + escapeHtml(tool.ToolName) + '" ' +
                         'data-route="' + (tool.RouteUrl || '') + '" ' +
                         'data-implemented="' + tool.IsImplemented + '">' +
                        badge +
                        '<div class="card-body">' +
                            '<div class="tool-icon">' + icon + '</div>' +
                            '<div class="tool-name">' + escapeHtml(tool.ToolName) + '</div>' +
                            '<div class="tool-description">' + escapeHtml(tool.ToolDescription || '') + '</div>' +
                        '</div>' +
                    '</div>' +
                '</div>'
            );

            container.append(card);
        });

        // Bind click events
        $('.tool-card').on('click', function () {
            var isImplemented = $(this).data('implemented');
            var route = $(this).data('route');
            var toolName = $(this).data('tool-name');

            if (isImplemented && route) {
                window.location.href = route.replace('~/', APP.baseUrl);
            } else {
                showUnderDevelopmentModal(toolName);
            }
        });
    }).fail(function (xhr) {
        if (xhr.status === 401) {
            window.location.href = APP.baseUrl + 'Auth/Login';
        } else {
            var container = $('#toolsContainer').empty();
            container.html('<div class="col-12 text-center text-danger">Failed to load tools</div>');
        }
    });
}

function getToolIcon(toolName) {
    var icons = {
        'Attendance Sync': '<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" viewBox="0 0 16 16"><path d="M11 6.5a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1zm-3 0a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1zm-5 3a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1zm3 0a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1z"/><path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5zM1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4H1z"/></svg>',
        'default': '<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" viewBox="0 0 16 16"><path d="M9.405 1.05c-.413-1.4-2.397-1.4-2.81 0l-.1.34a1.464 1.464 0 0 1-2.105.872l-.31-.17c-1.283-.698-2.686.705-1.987 1.987l.169.311c.446.82.023 1.841-.872 2.105l-.34.1c-1.4.413-1.4 2.397 0 2.81l.34.1a1.464 1.464 0 0 1 .872 2.105l-.17.31c-.698 1.283.705 2.686 1.987 1.987l.311-.169a1.464 1.464 0 0 1 2.105.872l.1.34c.413 1.4 2.397 1.4 2.81 0l.1-.34a1.464 1.464 0 0 1 2.105-.872l.31.17c1.283.698 2.686-.705 1.987-1.987l-.169-.311a1.464 1.464 0 0 1 .872-2.105l.34-.1c1.4-.413 1.4-2.397 0-2.81l-.34-.1a1.464 1.464 0 0 1-.872-2.105l.17-.31c.698-1.283-.705-2.686-1.987-1.987l-.311.169a1.464 1.464 0 0 1-2.105-.872l-.1-.34zM8 10.93a2.929 2.929 0 1 1 0-5.86 2.929 2.929 0 0 1 0 5.858z"/></svg>'
    };
    return icons[toolName] || icons['default'];
}

function showUnderDevelopmentModal(toolName) {
    $('#underDevToolName').text(toolName);
    var modal = new bootstrap.Modal(document.getElementById('underDevelopmentModal'));
    modal.show();
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}
