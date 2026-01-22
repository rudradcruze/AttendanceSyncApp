/* ============================
   Salary Garbage User Tool
   Real-time Database Scanning
============================ */

var currentScanType = 'confirm'; // 'confirm' or 'problematic'

$(function () {
    // Bind click events to tool cards
    $('#confirmGarbageCard').on('click', startConfirmScan);
    $('#problematicGarbageCard').on('click', startProblematicScan);
});

function startConfirmScan() {
    currentScanType = 'confirm';
    startScan('Confirm Garbage Scan', 'This will scan all configured server databases for employee records with GradeScaleId or BasicSalary issues. Continue?');
}

function startProblematicScan() {
    currentScanType = 'problematic';
    startScan('Problematic Garbage Scan', 'This will scan all configured server databases for salary mismatches between Employees and PromotionIncrements/Confirmations tables. Continue?');
}

function startScan(title, text) {
    // Show confirmation dialog
    Swal.fire({
        title: title,
        text: text,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: currentScanType === 'confirm' ? '#ee5a24' : '#f5576c',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, Start Scan',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            performScan();
        }
    });
}

function performScan() {
    // Hide card section, show progress section
    $('#cardSection').hide();
    $('#resultsSection').hide();
    $('#problematicResultsSection').hide();
    $('#progressSection').show();
    $('#progressList').empty();

    // First get all active servers
    $.get(APP.baseUrl + 'SalaryGarbge/GetActiveServers', function (res) {
        if (res.Errors && res.Errors.length > 0) {
            showError(res.Message);
            return;
        }

        var servers = res.Data;
        if (!servers || servers.length === 0) {
            showNoServersMessage();
            return;
        }

        // Start scanning servers one by one with real-time progress
        scanServersSequentially(servers, 0, []);
    }).fail(function () {
        showError('Failed to connect to the server');
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
