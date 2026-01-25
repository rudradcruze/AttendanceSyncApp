/* ============================
   Database Access Management
   SalaryGarbge Namespace
============================ */
var currentServerIpId = null;

$(function () {
    loadServerIps();
    $('#serverIpSelect').on('change', onServerIpChange);
});

function loadServerIps() {
    $.get(APP.baseUrl + 'AdminDatabaseAccess/GetServerIps', function (res) {
        var select = $('#serverIpSelect').empty();
        select.append('<option value="">-- Select a Server IP --</option>');

        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
            return;
        }

        var servers = res.Data;
        if (servers && servers.length > 0) {
            $.each(servers, function (_, server) {
                select.append(
                    '<option value="' + server.Id + '">' +
                    escapeHtml(server.IpAddress) +
                    (server.Description ? ' - ' + escapeHtml(server.Description) : '') +
                    '</option>'
                );
            });
        }
    }).fail(function () {
        Swal.fire('Error', 'Failed to load server IPs', 'error');
    });
}

function onServerIpChange() {
    var serverIpId = $('#serverIpSelect').val();

    if (!serverIpId) {
        $('#databaseListSection').hide();
        return;
    }

    currentServerIpId = parseInt(serverIpId);
    loadDatabases();
}

function loadDatabases() {
    if (!currentServerIpId) return;

    $('#databaseListSection').show();
    $('#databaseList').html('<div class="text-center py-4"><div class="spinner-border" role="status"></div><div class="mt-2">Loading databases...</div></div>');

    $.get(APP.baseUrl + 'AdminDatabaseAccess/GetDatabasesWithAccess',
        { serverIpId: currentServerIpId },
        function (res) {
            var container = $('#databaseList').empty();

            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
                $('#databaseListSection').hide();
                return;
            }

            var databases = res.Data;
            if (!databases || databases.length === 0) {
                $('#noDatabasesMessage').show();
                return;
            }

            $('#noDatabasesMessage').hide();

            $.each(databases, function (_, db) {
                var cardClass = 'database-card';
                var badgeHtml = '';
                var actionButton = '';

                if (!db.ExistsInAccessTable) {
                    // New database not yet in access table
                    cardClass += ' new-database';
                    badgeHtml = '<span class="access-badge new-badge">NEW</span>';
                    actionButton = '<button class="btn btn-sm btn-success" onclick="addDatabaseAccess(\'' + escapeHtml(db.DatabaseName) + '\')">Add Access</button>';
                } else if (db.HasAccess) {
                    // Access granted
                    cardClass += ' has-access';
                    badgeHtml = '<span class="access-badge granted-badge">Access Granted</span>';
                    actionButton = '<button class="btn btn-sm btn-warning me-1" onclick="toggleAccess(\'' + escapeHtml(db.DatabaseName) + '\', false)">Revoke Access</button>' +
                        '<button class="btn btn-sm btn-danger" onclick="removeDatabaseAccess(\'' + escapeHtml(db.DatabaseName) + '\')">Remove</button>';
                } else {
                    // Access revoked
                    cardClass += ' no-access';
                    badgeHtml = '<span class="access-badge revoked-badge">Access Revoked</span>';
                    actionButton = '<button class="btn btn-sm btn-success me-1" onclick="toggleAccess(\'' + escapeHtml(db.DatabaseName) + '\', true)">Grant Access</button>' +
                        '<button class="btn btn-sm btn-danger" onclick="removeDatabaseAccess(\'' + escapeHtml(db.DatabaseName) + '\')">Remove</button>';
                }

                var card = '<div class="' + cardClass + '">' +
                    '<div class="d-flex justify-content-between align-items-center">' +
                    '<div>' +
                    '<span class="database-name">' + escapeHtml(db.DatabaseName) + '</span>' +
                    ' ' + badgeHtml +
                    '</div>' +
                    '<div>' + actionButton + '</div>' +
                    '</div>' +
                    '</div>';

                container.append(card);
            });
        }
    ).fail(function () {
        Swal.fire('Error', 'Failed to load databases', 'error');
        $('#databaseListSection').hide();
    });
}

function addDatabaseAccess(databaseName) {
    Swal.fire({
        title: 'Add Database Access',
        text: 'Grant access to database "' + databaseName + '"?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, Add Access'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminDatabaseAccess/AddDatabaseAccess',
                type: 'POST',
                data: {
                    serverIpId: currentServerIpId,
                    databaseName: databaseName
                },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Success', res.Message, 'success');
                        loadDatabases();
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to add database access', 'error');
                }
            });
        }
    });
}

function toggleAccess(databaseName, hasAccess) {
    var action = hasAccess ? 'grant' : 'revoke';
    var actionTitle = hasAccess ? 'Grant Access' : 'Revoke Access';

    Swal.fire({
        title: actionTitle,
        text: 'Are you sure you want to ' + action + ' access to "' + databaseName + '"?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: hasAccess ? '#28a745' : '#ffc107',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, ' + actionTitle
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminDatabaseAccess/UpdateDatabaseAccess',
                type: 'POST',
                data: {
                    serverIpId: currentServerIpId,
                    databaseName: databaseName,
                    hasAccess: hasAccess
                },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Success', res.Message, 'success');
                        loadDatabases();
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to update database access', 'error');
                }
            });
        }
    });
}

function removeDatabaseAccess(databaseName) {
    Swal.fire({
        title: 'Remove Database Access',
        text: 'Are you sure you want to remove "' + databaseName + '" from the access list? This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, Remove'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminDatabaseAccess/RemoveDatabaseAccess',
                type: 'POST',
                data: {
                    serverIpId: currentServerIpId,
                    databaseName: databaseName
                },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Deleted', res.Message, 'success');
                        loadDatabases();
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to remove database access', 'error');
                }
            });
        }
    });
}

function refreshDatabases() {
    loadDatabases();
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}
