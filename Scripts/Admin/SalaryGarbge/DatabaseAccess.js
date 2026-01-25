/* ============================
   Database Access Management
   SalaryGarbge Namespace
============================ */
var currentServerIpId = null;

// Toast configuration
const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 2000,
    timerProgressBar: true,
    didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer)
        toast.addEventListener('mouseleave', Swal.resumeTimer)
    }
});

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

    // Save current scroll position
    var scrollPosition = $(window).scrollTop();

    $('#databaseListSection').show();
    $('#databaseTableBody').html('<tr><td colspan="4" class="text-center py-4"><div class="spinner-border" role="status"></div><div class="mt-2">Loading databases...</div></td></tr>');

    $.get(APP.baseUrl + 'AdminDatabaseAccess/GetDatabasesWithAccess',
        { serverIpId: currentServerIpId },
        function (res) {
            var tbody = $('#databaseTableBody').empty();

            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
                $('#databaseListSection').hide();
                return;
            }

            var databases = res.Data;
            if (!databases || databases.length === 0) {
                $('#noDatabasesMessage').show();
                $('#statsSection').hide();
                $('#databaseTable').hide();
                return;
            }

            $('#noDatabasesMessage').hide();
            $('#statsSection').show();
            $('#databaseTable').show();

            // Calculate statistics
            var stats = {
                total: databases.length,
                new: 0,
                granted: 0,
                revoked: 0
            };

            $.each(databases, function (index, db) {
                if (!db.ExistsInAccessTable) {
                    stats.new++;
                } else if (db.HasAccess) {
                    stats.granted++;
                } else {
                    stats.revoked++;
                }
            });

            // Update statistics display
            $('#statTotal').text(stats.total);
            $('#statNew').text(stats.new);
            $('#statGranted').text(stats.granted);
            $('#statRevoked').text(stats.revoked);

            // Render table rows
            $.each(databases, function (index, db) {
                var rowClass = '';
                var badgeHtml = '';
                var actionButton = '';

                if (!db.ExistsInAccessTable) {
                    // New database not yet in access table
                    rowClass = 'row-new';
                    badgeHtml = '<span class="badge-new">NEW</span>';
                    actionButton = '<button class="btn btn-sm btn-success" onclick="addDatabaseAccess(\'' + escapeHtml(db.DatabaseName) + '\')">Add Access</button>';
                } else if (db.HasAccess) {
                    // Access granted
                    rowClass = 'row-granted';
                    badgeHtml = '<span class="badge-granted">Access Granted</span>';
                    actionButton = '<button class="btn btn-sm btn-warning me-1" onclick="toggleAccess(\'' + escapeHtml(db.DatabaseName) + '\', false)">Revoke Access</button>' +
                        '<button class="btn btn-sm btn-danger" onclick="removeDatabaseAccess(\'' + escapeHtml(db.DatabaseName) + '\')">Remove</button>';
                } else {
                    // Access revoked
                    rowClass = 'row-revoked';
                    badgeHtml = '<span class="badge-revoked">Access Revoked</span>';
                    actionButton = '<button class="btn btn-sm btn-success me-1" onclick="toggleAccess(\'' + escapeHtml(db.DatabaseName) + '\', true)">Grant Access</button>' +
                        '<button class="btn btn-sm btn-danger" onclick="removeDatabaseAccess(\'' + escapeHtml(db.DatabaseName) + '\')">Remove</button>';
                }

                var row = '<tr class="' + rowClass + '">' +
                    '<td>' + (index + 1) + '</td>' +
                    '<td><strong>' + escapeHtml(db.DatabaseName) + '</strong></td>' +
                    '<td>' + badgeHtml + '</td>' +
                    '<td>' + actionButton + '</td>' +
                    '</tr>';

                tbody.append(row);
            });

            // Restore scroll position after rendering
            setTimeout(function() {
                $(window).scrollTop(scrollPosition);
            }, 0);
        }
    ).fail(function () {
        Swal.fire('Error', 'Failed to load databases', 'error');
        $('#databaseListSection').hide();
    });
}

function addDatabaseAccess(databaseName) {
    // No confirmation - add directly
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
                Toast.fire({
                    icon: 'success',
                    title: 'Access added successfully'
                });
                loadDatabases();
            }
        },
        error: function () {
            Swal.fire('Error', 'Failed to add database access', 'error');
        }
    });
}

function toggleAccess(databaseName, hasAccess) {
    // No confirmation - toggle directly
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
                var action = hasAccess ? 'granted' : 'revoked';
                Toast.fire({
                    icon: 'success',
                    title: 'Access ' + action + ' successfully'
                });
                loadDatabases();
            }
        },
        error: function () {
            Swal.fire('Error', 'Failed to update database access', 'error');
        }
    });
}

function removeDatabaseAccess(databaseName) {
    // Keep confirmation for remove action
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
