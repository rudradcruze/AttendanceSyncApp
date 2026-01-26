/* ============================
   Salary Garbage User Tool
   Real-time Database Scanning
============================ */

var currentScanType = 'confirm'; // 'confirm' or 'problematic'
var selectedServerIpId = null;
var selectedDatabases = [];
var currentGarbageData = []; // Store current results for export

$(function () {
    // Bind click events to tool cards
    $('#confirmGarbageCard').on('click', startConfirmScan);
    $('#problematicGarbageCard').on('click', startProblematicScan);
});

function startConfirmScan() {
    currentScanType = 'confirm';
    $('#scanTypeTitle').text('Confirm Garbage Scan');
    $('#selectionHeaderIcon').removeClass('problematic-icon').addClass('confirm-icon');
    showSelectionSection();
}

function startProblematicScan() {
    currentScanType = 'problematic';
    $('#scanTypeTitle').text('Problematic Garbage Scan');
    $('#selectionHeaderIcon').removeClass('confirm-icon').addClass('problematic-icon');
    showSelectionSection();
}

function showSelectionSection() {
    $('#cardSection').hide();
    $('#selectionSection').show();
    $('#resultsSection').hide();
    $('#problematicResultsSection').hide();
    $('#progressSection').hide();

    // Reset selections
    selectedServerIpId = null;
    selectedDatabases = [];
    currentGarbageData = [];

    // Reset UI
    $('#databaseSelectionDiv').hide();
    $('#serverIpSelectionDiv').show();
    $('#startScanBtn').prop('disabled', true);
    $('#selectedServerIpDisplay').text('-');
    $('#selectedCount').text('0');
    $('.ip-card').removeClass('selected');
    $('.db-card').removeClass('selected');
    $('#databaseContainer').empty();

    // Load server IPs
    loadServerIps();
}

function loadServerIps() {
    $.get(APP.baseUrl + 'SalaryGarbge/GetActiveServers', function (res) {
        var container = $('#serverIpContainer');

        if (res.Errors && res.Errors.length > 0) {
            container.html(
                '<div class="col-12 text-center py-5">' +
                '<div class="text-danger">' +
                '<p>' + escapeHtml(res.Message) + '</p>' +
                '</div>' +
                '</div>'
            );
            return;
        }

        var servers = res.Data;
        if (!servers || servers.length === 0) {
            container.html(
                '<div class="col-12 text-center py-5">' +
                '<p class="text-muted">No server IPs available</p>' +
                '</div>'
            );
            return;
        }

        var html = '';
        $.each(servers, function (_, server) {
            html += '<div class="col-md-4 col-lg-3 mb-3">' +
                '<div class="ip-card" data-id="' + server.Id + '" data-ip="' + escapeHtml(server.IpAddress) + '" onclick="selectServerIp(' + server.Id + ', \'' + escapeHtml(server.IpAddress) + '\')">' +
                '<div class="text-center">' +
                '<div class="ip-icon">' +
                '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="currentColor" viewBox="0 0 16 16">' +
                '<path d="M6 12.5a.5.5 0 0 1 .5-.5h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1-.5-.5ZM3 8.062C3 6.76 4.235 5.765 5.53 5.886a26.58 26.58 0 0 0 4.94 0C11.765 5.765 13 6.76 13 8.062v1.157a.933.933 0 0 1-.765.935c-.845.147-2.34.346-4.235.346-1.895 0-3.39-.2-4.235-.346A.933.933 0 0 1 3 9.219V8.062Z"/>' +
                '<path d="M8.5 1.866a1 1 0 1 0-1 0V3h-2A4.5 4.5 0 0 0 1 7.5V8a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1v1a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2v-1a1 1 0 0 0 1-1V9a1 1 0 0 0-1-1v-.5A4.5 4.5 0 0 0 10.5 3h-2V1.866ZM14 7.5V13a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V7.5A3.5 3.5 0 0 1 5.5 4h5A3.5 3.5 0 0 1 14 7.5Z"/>' +
                '</svg>' +
                '</div>' +
                '<div class="ip-address">' + escapeHtml(server.IpAddress) + '</div>' +
                '<div class="ip-desc">' + (server.Description ? escapeHtml(server.Description) : 'Server') + '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
        });

        container.html(html);
    }).fail(function () {
        $('#serverIpContainer').html(
            '<div class="col-12 text-center py-5">' +
            '<div class="text-danger">' +
            '<p>Failed to load server IPs</p>' +
            '</div>' +
            '</div>'
        );
    });
}

// Select server IP and load databases
function selectServerIp(id, ipAddress) {
    selectedServerIpId = id;
    selectedDatabases = [];

    // Update UI
    $('.ip-card').removeClass('selected');
    $('.ip-card[data-id="' + id + '"]').addClass('selected');

    // Update display
    $('#selectedServerIpDisplay').text(ipAddress);

    // Show database selection
    $('#databaseSelectionDiv').show();

    // Load databases
    loadDatabases();
}

function loadDatabases() {
    if (!selectedServerIpId) return;

    var container = $('#databaseContainer');
    container.html(
        '<div class="col-12 text-center py-5">' +
        '<div class="spinner-border text-primary" role="status"></div>' +
        '<p class="mt-2 text-muted">Loading databases...</p>' +
        '</div>'
    );

    $('#startScanBtn').prop('disabled', true);

    // Use SalaryGarbge endpoint to get only accessible databases
    $.get(APP.baseUrl + 'SalaryGarbge/GetAccessibleDatabases',
        { serverIpId: selectedServerIpId },
        function (res) {
            if (res.Errors && res.Errors.length > 0) {
                container.html(
                    '<div class="col-12 text-center py-5">' +
                    '<div class="text-danger">' +
                    '<p>' + escapeHtml(res.Message) + '</p>' +
                    '</div>' +
                    '</div>'
                );
                return;
            }

            var databases = res.Data;

            if (!databases || databases.length === 0) {
                container.html(
                    '<div class="col-12 text-center py-5">' +
                    '<p class="text-muted">No accessible databases found on this server</p>' +
                    '</div>'
                );
                Swal.fire('Warning', 'No accessible databases found for this server. Please contact your administrator to grant database access.', 'warning');
                return;
            }

            var html = '';
            $.each(databases, function (_, db) {
                html += '<div class="col-md-4 col-lg-3 mb-3">' +
                    '<div class="db-card" data-name="' + escapeHtml(db.DatabaseName) + '" onclick="toggleDatabase(\'' + escapeHtml(db.DatabaseName) + '\')">' +
                    '<div class="text-center">' +
                    '<div class="db-icon">' +
                    '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="currentColor" viewBox="0 0 16 16">' +
                    '<path d="M12.5 16a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Zm.5-5v1h1a.5.5 0 0 1 0 1h-1v1a.5.5 0 0 1-1 0v-1h-1a.5.5 0 0 1 0-1h1v-1a.5.5 0 0 1 1 0Z"/>' +
                    '<path d="M12.096 6.223A4.92 4.92 0 0 0 13 5.698V7c0 .289-.213.654-.753 1.007a4.493 4.493 0 0 1 1.753.25V4c0-1.007-.875-1.755-1.904-2.223C11.022 1.289 9.573 1 8 1s-3.022.289-4.096.777C2.875 2.245 2 2.993 2 4v9c0 1.007.875 1.755 1.904 2.223C4.978 15.71 6.427 16 8 16c.536 0 1.058-.034 1.555-.097a4.525 4.525 0 0 1-.813-.927C8.5 14.992 8.252 15 8 15c-1.464 0-2.766-.27-3.682-.687C3.356 13.875 3 13.373 3 13v-1.302c.271.202.58.378.904.525C4.978 12.71 6.427 13 8 13h.027a4.552 4.552 0 0 1 0-1H8c-1.464 0-2.766-.27-3.682-.687C3.356 10.875 3 10.373 3 10V8.698c.271.202.58.378.904.525C4.978 9.71 6.427 10 8 10c.262 0 .52-.008.774-.024a4.525 4.525 0 0 1 1.102-1.132C9.298 8.944 8.666 9 8 9c-1.464 0-2.766-.27-3.682-.687C3.356 7.875 3 7.373 3 7V5.698c.271.202.58.378.904.525C4.978 6.711 6.427 7 8 7s3.022-.289 4.096-.777ZM3 4c0-.374.356-.875 1.318-1.313C5.234 2.271 6.536 2 8 2s2.766.27 3.682.687C12.644 3.125 13 3.627 13 4c0 .374-.356.875-1.318 1.313C10.766 5.729 9.464 6 8 6s-2.766-.27-3.682-.687C3.356 4.875 3 4.373 3 4Z"/>' +
                    '</svg>' +
                    '</div>' +
                    '<div class="db-name">' + escapeHtml(db.DatabaseName) + '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>';
            });

            container.html(html);
            updateSelectedCount();
        }
    ).fail(function () {
        container.html(
            '<div class="col-12 text-center py-5">' +
            '<div class="text-danger">' +
            '<p>Failed to load databases</p>' +
            '</div>' +
            '</div>'
        );
    });
}

// Toggle database selection (for multi-select)
function toggleDatabase(databaseName) {
    var card = $('.db-card[data-name="' + databaseName + '"]');

    if (card.hasClass('selected')) {
        // Deselect
        card.removeClass('selected');
        var index = selectedDatabases.indexOf(databaseName);
        if (index > -1) {
            selectedDatabases.splice(index, 1);
        }
    } else {
        // Select
        card.addClass('selected');
        if (selectedDatabases.indexOf(databaseName) === -1) {
            selectedDatabases.push(databaseName);
        }
    }

    updateSelectedCount();
    validateSelection();
}

function updateSelectedCount() {
    var count = selectedDatabases.length;
    $('#selectedCount').text(count);
}

function validateSelection() {
    var isValid = selectedServerIpId && selectedDatabases.length > 0;
    $('#startScanBtn').prop('disabled', !isValid);
}

function selectAllDatabases() {
    $('.db-card').addClass('selected');
    selectedDatabases = [];
    $('.db-card').each(function() {
        var dbName = $(this).data('name');
        if (dbName && selectedDatabases.indexOf(dbName) === -1) {
            selectedDatabases.push(dbName);
        }
    });
    updateSelectedCount();
    validateSelection();
}

function deselectAllDatabases() {
    $('.db-card').removeClass('selected');
    selectedDatabases = [];
    updateSelectedCount();
    validateSelection();
}

function cancelSelection() {
    $('#selectionSection').hide();
    $('#cardSection').show();
    selectedServerIpId = null;
    selectedDatabases = [];
    currentGarbageData = [];

    // Reset UI elements
    $('.ip-card').removeClass('selected');
    $('.db-card').removeClass('selected');
    $('#selectedServerIpDisplay').text('-');
    $('#selectedCount').text('0');
}

function startScanWithSelection() {
    if (!selectedServerIpId || selectedDatabases.length === 0) {
        Swal.fire('Warning', 'Please select a server IP and at least one database', 'warning');
        return;
    }

    performScan();
}

function performScan() {
    // Hide sections, show progress
    $('#cardSection').hide();
    $('#selectionSection').hide();
    $('#resultsSection').hide();
    $('#problematicResultsSection').hide();
    $('#progressSection').show();
    $('#progressList').empty();
    currentGarbageData = [];

    // Get selected server info
    $.get(APP.baseUrl + 'SalaryGarbge/GetActiveServers', function (res) {
        if (res.Errors && res.Errors.length > 0) {
            showError(res.Message);
            return;
        }

        var servers = res.Data;
        var selectedServer = servers.find(function(s) { return s.Id === selectedServerIpId; });

        if (!selectedServer) {
            showError('Selected server not found');
            return;
        }

        // Scan only selected databases on the selected server
        scanSelectedDatabases(selectedServer, selectedDatabases, []);
    }).fail(function () {
        showError('Failed to connect to the server');
    });
}

function scanSelectedDatabases(server, databaseNames, allGarbageData) {
    var progressId = 'server-' + server.Id;

    // Add progress item for this server
    addProgressItem(progressId, server.IpAddress, 'Starting scan...', 'scanning');

    // Scan the selected databases sequentially
    scanDatabasesSequentially(server, databaseNames, 0, allGarbageData, progressId, function (serverGarbageData) {
        updateProgressItem(progressId, server.IpAddress, 'Completed (' + databaseNames.length + ' databases, ' + serverGarbageData.length + ' issues)', 'completed');

        // Show results (1 server, total databases scanned)
        showResults(1, serverGarbageData);
    });
}

function scanServersSequentially(servers, index, allGarbageData) {
    if (index >= servers.length) {
        // All servers scanned, show results
        showResults(servers.length, allGarbageData);
        return;
    }

    var server = servers[index];
    var progressId = 'server-' + server.Id;

    // Add progress item for this server
    addProgressItem(progressId, server.IpAddress, 'Connecting...', 'scanning');

    // Get databases on this server
    $.get(APP.baseUrl + 'SalaryGarbge/GetDatabases', { serverIpId: server.Id }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            updateProgressItem(progressId, server.IpAddress, 'Failed: ' + res.Message, 'error');
            // Continue to next server
            scanServersSequentially(servers, index + 1, allGarbageData);
            return;
        }

        var databases = res.Data;
        if (!databases || databases.length === 0) {
            updateProgressItem(progressId, server.IpAddress, 'No databases found', 'completed');
            scanServersSequentially(servers, index + 1, allGarbageData);
            return;
        }

        // Scan databases on this server
        scanDatabasesSequentially(server, databases, 0, allGarbageData, progressId, function (serverGarbageData) {
            allGarbageData = allGarbageData.concat(serverGarbageData);
            updateProgressItem(progressId, server.IpAddress, 'Completed (' + databases.length + ' databases, ' + serverGarbageData.length + ' issues)', 'completed');
            // Continue to next server
            scanServersSequentially(servers, index + 1, allGarbageData);
        });
    }).fail(function () {
        updateProgressItem(progressId, server.IpAddress, 'Connection failed', 'error');
        scanServersSequentially(servers, index + 1, allGarbageData);
    });
}

function scanDatabasesSequentially(server, databases, dbIndex, garbageData, progressId, callback) {
    if (dbIndex >= databases.length) {
        callback(garbageData);
        return;
    }

    var dbName = databases[dbIndex];
    updateProgressItem(progressId, server.IpAddress, 'Scanning: ' + dbName + ' (' + (dbIndex + 1) + '/' + databases.length + ')', 'scanning');

    // Use appropriate endpoint based on scan type
    var scanUrl = currentScanType === 'confirm'
        ? APP.baseUrl + 'SalaryGarbge/ScanDatabase'
        : APP.baseUrl + 'SalaryGarbge/ScanProblematicDatabase';

    $.ajax({
        url: scanUrl,
        type: 'POST',
        data: { serverIpId: server.Id, databaseName: dbName },
        success: function (res) {
            if (res.Data && res.Data.length > 0) {
                garbageData = garbageData.concat(res.Data);
            }
            // Continue to next database
            scanDatabasesSequentially(server, databases, dbIndex + 1, garbageData, progressId, callback);
        },
        error: function () {
            // Skip failed database, continue
            scanDatabasesSequentially(server, databases, dbIndex + 1, garbageData, progressId, callback);
        }
    });
}

function addProgressItem(id, serverIp, message, status) {
    var iconHtml = getStatusIcon(status);
    var html = '<div class="progress-item ' + status + '" id="' + id + '">' +
        '<div class="status-icon">' + iconHtml + '</div>' +
        '<div class="progress-info">' +
        '<strong>' + escapeHtml(serverIp) + '</strong>' +
        '<div class="text-muted small">' + escapeHtml(message) + '</div>' +
        '</div>' +
        '</div>';
    $('#progressList').append(html);
}

function updateProgressItem(id, serverIp, message, status) {
    var item = $('#' + id);
    item.removeClass('scanning completed error').addClass(status);
    item.find('.status-icon').html(getStatusIcon(status));
    item.find('.progress-info').html(
        '<strong>' + escapeHtml(serverIp) + '</strong>' +
        '<div class="text-muted small">' + escapeHtml(message) + '</div>'
    );
}

function getStatusIcon(status) {
    if (status === 'scanning') {
        return '<div class="spinner-border spinner-border-sm" role="status"></div>';
    } else if (status === 'completed') {
        return '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" class="bi bi-check-circle-fill" viewBox="0 0 16 16"><path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/></svg>';
    } else {
        return '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" class="bi bi-x-circle-fill" viewBox="0 0 16 16"><path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/></svg>';
    }
}

function showResults(totalServers, garbageData) {
    // Hide progress section
    $('#progressSection').hide();

    // Store results for export
    currentGarbageData = garbageData;

    // Count unique databases
    var uniqueDatabases = {};
    garbageData.forEach(function (item) {
        var key = item.ServerIp + '|' + item.DatabaseName;
        uniqueDatabases[key] = true;
    });
    var totalDbs = Object.keys(uniqueDatabases).length;

    if (currentScanType === 'confirm') {
        showConfirmResults(totalServers, totalDbs, garbageData);
    } else {
        showProblematicResults(totalServers, totalDbs, garbageData);
    }
}

function showConfirmResults(totalServers, totalDatabases, garbageData) {
    // Update summary
    $('#totalServers').text(totalServers);
    $('#totalDatabases').text(totalDatabases);
    $('#totalGarbage').text(garbageData.length);

    // Sort data by Server IP (numerically) then by Database name (alphabetically)
    garbageData = sortGarbageData(garbageData);
    currentGarbageData = garbageData; // Ensure stored data is also sorted

    // Populate table
    var tbody = $('#garbageTableBody').empty();
    if (garbageData.length === 0) {
        $('#garbageTable').hide();
        $('#noDataMessage').show();
    } else {
        $('#garbageTable').show();
        $('#noDataMessage').hide();

        var rowNum = 1;
        garbageData.forEach(function (item) {
            var problemBadge = getProblemBadge(item.Problem);
            var row = '<tr>' +
                '<td>' + rowNum++ + '</td>' +
                '<td>' + escapeHtml(item.ServerIp) + '</td>' +
                '<td>' + escapeHtml(item.DatabaseName) + '</td>' +
                '<td>' + item.EmployeeId + '</td>' +
                '<td>' + item.EmployeeCode + '</td>' +
                '<td>' + escapeHtml(item.EmployeeName) + '</td>' +
                '<td>' + problemBadge + '</td>' +
                '</tr>';
            tbody.append(row);
        });
    }

    // Show results section
    $('#resultsSection').show();
}

function showProblematicResults(totalServers, totalDatabases, problematicData) {
    // Update summary
    $('#problematicTotalServers').text(totalServers);
    $('#problematicTotalDatabases').text(totalDatabases);
    $('#totalProblematic').text(problematicData.length);

    // Sort data by Server IP (numerically) then by Database name (alphabetically)
    problematicData = sortGarbageData(problematicData);
    currentGarbageData = problematicData; // Ensure stored data is also sorted

    // Populate table
    var tbody = $('#problematicTableBody').empty();
    if (problematicData.length === 0) {
        $('#problematicTable').hide();
        $('#noProblematicDataMessage').show();
    } else {
        $('#problematicTable').show();
        $('#noProblematicDataMessage').hide();

        var rowNum = 1;
        problematicData.forEach(function (item) {
            var issueBadge = getIssueBadge(item.IssueTableName);
            var row = '<tr>' +
                '<td>' + rowNum++ + '</td>' +
                '<td>' + escapeHtml(item.ServerIp) + '</td>' +
                '<td>' + escapeHtml(item.DatabaseName) + '</td>' +
                '<td>' + item.EmployeeId + '</td>' +
                '<td>' + item.EmployeeCode + '</td>' +
                '<td>' + escapeHtml(item.EmployeeName) + '</td>' +
                '<td>' + issueBadge + '</td>' +
                '<td class="salary-mismatch">' + formatCurrency(item.CurrentBasicSalary) + '</td>' +
                '<td class="salary-expected">' + formatCurrency(item.ExpectedBasicSalary) + '</td>' +
                '</tr>';
            tbody.append(row);
        });
    }

    // Show results section
    $('#problematicResultsSection').show();
}

function exportToExcel() {
    if (!currentGarbageData || currentGarbageData.length === 0) {
        Swal.fire('Warning', 'No data to export', 'warning');
        return;
    }

    var tableContent = '';
    var fileName = '';

    if (currentScanType === 'confirm') {
        fileName = 'Salary_Garbage_Report_' + new Date().toISOString().slice(0, 10) + '.xls';
        tableContent = '<table>' +
            '<thead>' +
            '<tr>' +
            '<th>Server IP</th>' +
            '<th>Database Name</th>' +
            '<th>Employee ID</th>' +
            '<th>Employee Code</th>' +
            '<th>Employee Name</th>' +
            '<th>Problem</th>' +
            '</tr>' +
            '</thead>' +
            '<tbody>';

        currentGarbageData.forEach(function (item) {
            tableContent += '<tr>' +
                '<td>' + (item.ServerIp || '') + '</td>' +
                '<td>' + (item.DatabaseName || '') + '</td>' +
                '<td>' + (item.EmployeeId || '') + '</td>' +
                '<td>' + (item.EmployeeCode || '') + '</td>' +
                '<td>' + (item.EmployeeName || '') + '</td>' +
                '<td>' + (item.Problem || '') + '</td>' +
                '</tr>';
        });
    } else {
        fileName = 'Problematic_Salary_Report_' + new Date().toISOString().slice(0, 10) + '.xls';
        tableContent = '<table>' +
            '<thead>' +
            '<tr>' +
            '<th>Server IP</th>' +
            '<th>Database Name</th>' +
            '<th>Employee ID</th>' +
            '<th>Employee Code</th>' +
            '<th>Employee Name</th>' +
            '<th>Issue Table</th>' +
            '<th>Current Basic Salary</th>' +
            '<th>Expected Basic Salary</th>' +
            '</tr>' +
            '</thead>' +
            '<tbody>';

        currentGarbageData.forEach(function (item) {
            tableContent += '<tr>' +
                '<td>' + (item.ServerIp || '') + '</td>' +
                '<td>' + (item.DatabaseName || '') + '</td>' +
                '<td>' + (item.EmployeeId || '') + '</td>' +
                '<td>' + (item.EmployeeCode || '') + '</td>' +
                '<td>' + (item.EmployeeName || '') + '</td>' +
                '<td>' + (item.IssueTableName || '') + '</td>' +
                '<td>' + (item.CurrentBasicSalary || '0') + '</td>' +
                '<td>' + (item.ExpectedBasicSalary || '0') + '</td>' +
                '</tr>';
        });
    }

    tableContent += '</tbody></table>';

    // Create a Blob with the Excel MIME type
    var blob = new Blob([tableContent], { type: 'application/vnd.ms-excel' });
    
    // Create a download link and trigger it
    var url = window.URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    
    // Cleanup
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}

function getIssueBadge(issueTableName) {
    var badgeClass = 'issue-badge ';
    if (issueTableName === 'PromotionIncrements') {
        badgeClass += 'issue-promotion';
    } else if (issueTableName === 'Confirmations') {
        badgeClass += 'issue-confirmation';
    }
    return '<span class="' + badgeClass + '">' + escapeHtml(issueTableName) + '</span>';
}

function formatCurrency(value) {
    if (value === null || value === undefined) return '0.00';
    return parseFloat(value).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function getProblemBadge(problem) {
    var badgeClass = 'problem-badge ';
    if (problem.indexOf('GradeScaleId is 0') >= 0) {
        badgeClass += 'problem-gradescale-0';
    } else if (problem.indexOf('GradeScaleId is NULL') >= 0) {
        badgeClass += 'problem-gradescale-null';
    } else if (problem.indexOf('BasicSalary is 0') >= 0) {
        badgeClass += 'problem-salary-0';
    } else if (problem.indexOf('BasicSalary is NULL') >= 0) {
        badgeClass += 'problem-salary-null';
    }
    return '<span class="' + badgeClass + '">' + escapeHtml(problem) + '</span>';
}

function resetAndShowCard() {
    $('#resultsSection').hide();
    $('#problematicResultsSection').hide();
    $('#progressSection').hide();
    $('#cardSection').show();
    currentGarbageData = []; // Clear data on reset
}

function showNoServersMessage() {
    $('#progressSection').hide();
    Swal.fire({
        title: 'No Servers Configured',
        text: 'No active server IPs are configured. Please ask an administrator to configure server IPs.',
        icon: 'warning',
        confirmButtonColor: '#667eea'
    }).then(function () {
        $('#cardSection').show();
    });
}

function showError(message) {
    $('#progressSection').hide();
    Swal.fire({
        title: 'Error',
        text: message,
        icon: 'error',
        confirmButtonColor: '#ee5a24'
    }).then(function () {
        $('#cardSection').show();
    });
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}

// Parse IP address to numeric value for sorting
function ipToNumber(ip) {
    if (!ip) return 0;
    var parts = ip.split('.');
    if (parts.length !== 4) return 0;
    return ((parseInt(parts[0], 10) || 0) * 16777216) +
           ((parseInt(parts[1], 10) || 0) * 65536) +
           ((parseInt(parts[2], 10) || 0) * 256) +
           (parseInt(parts[3], 10) || 0);
}

// Sort data by Server IP (numerically) then by Database name (alphabetically)
function sortGarbageData(data) {
    return data.sort(function (a, b) {
        // First compare by Server IP numerically
        var ipA = ipToNumber(a.ServerIp);
        var ipB = ipToNumber(b.ServerIp);
        if (ipA !== ipB) {
            return ipA - ipB;
        }
        // Then compare by Database name alphabetically
        var dbA = (a.DatabaseName || '').toLowerCase();
        var dbB = (b.DatabaseName || '').toLowerCase();
        return dbA.localeCompare(dbB);
    });
}
