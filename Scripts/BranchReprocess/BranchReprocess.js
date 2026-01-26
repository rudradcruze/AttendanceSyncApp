// Branch Reprocess JavaScript
(function () {
    'use strict';

    // State management
    var state = {
        selectedServerIpId: null,
        selectedServerIpAddress: null,
        selectedDatabaseName: null,
        problemBranches: []
    };

    // DOM Elements
    var elements = {
        step1Section: null,
        step2Section: null,
        step3Section: null,
        step1Indicator: null,
        step2Indicator: null,
        step3Indicator: null,
        serverIpContainer: null,
        databaseContainer: null,
        dataTableBody: null,
        dataTableContainer: null,
        dataLoadingSpinner: null,
        noDataMessage: null,
        recordCount: null,
        loadingOverlay: null,
        monthStartDateInput: null,
        locationIdInput: null
    };

    // Initialize on document ready
    $(document).ready(function () {
        initializeElements();
        loadServerIps();
    });

    function initializeElements() {
        elements.step1Section = $('#step1Section');
        elements.step2Section = $('#step2Section');
        elements.step3Section = $('#step3Section');
        elements.step1Indicator = $('#step1Indicator');
        elements.step2Indicator = $('#step2Indicator');
        elements.step3Indicator = $('#step3Indicator');
        elements.serverIpContainer = $('#serverIpContainer');
        elements.databaseContainer = $('#databaseContainer');
        elements.dataTableBody = $('#dataTableBody');
        elements.dataTableContainer = $('#dataTableContainer');
        elements.dataLoadingSpinner = $('#dataLoadingSpinner');
        elements.noDataMessage = $('#noDataMessage');
        elements.recordCount = $('#recordCount');
        elements.loadingOverlay = $('#loadingOverlay');
        elements.monthStartDateInput = $('#monthStartDate');
        elements.locationIdInput = $('#locationId');
    }

    // Load server IPs
    function loadServerIps() {
        $.ajax({
            url: APP.baseUrl + 'BranchReprocess/GetServerIps',
            type: 'GET',
            success: function (response) {
                if (response.Errors && response.Errors.length > 0) {
                    showError(elements.serverIpContainer, response.Errors[0]);
                    return;
                }
                renderServerIps(response.Data);
            },
            error: function (xhr) {
                showError(elements.serverIpContainer, 'Failed to load server IPs');
            }
        });
    }

    function renderServerIps(serverIps) {
        if (!serverIps || serverIps.length === 0) {
            elements.serverIpContainer.html(
                '<div class="col-12 text-center py-5">' +
                '<p class="text-muted">No server IPs available</p>' +
                '</div>'
            );
            return;
        }

        var html = '';
        serverIps.forEach(function (ip) {
            html += '<div class="col-md-4 col-lg-3 mb-3">' +
                '<div class="ip-card" data-id="' + ip.Id + '" data-ip="' + ip.IpAddress + '" onclick="selectServerIp(' + ip.Id + ', \'' + ip.IpAddress + '\')">' +
                '<div class="text-center">' +
                '<div class="ip-icon">' +
                '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="currentColor" viewBox="0 0 16 16">' +
                '<path d="M6 12.5a.5.5 0 0 1 .5-.5h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1-.5-.5ZM3 8.062C3 6.76 4.235 5.765 5.53 5.886a26.58 26.58 0 0 0 4.94 0C11.765 5.765 13 6.76 13 8.062v1.157a.933.933 0 0 1-.765.935c-.845.147-2.34.346-4.235.346-1.895 0-3.39-.2-4.235-.346A.933.933 0 0 1 3 9.219V8.062Z"/>' +
                '<path d="M8.5 1.866a1 1 0 1 0-1 0V3h-2A4.5 4.5 0 0 0 1 7.5V8a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1v1a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2v-1a1 1 0 0 0 1-1V9a1 1 0 0 0-1-1v-.5A4.5 4.5 0 0 0 10.5 3h-2V1.866ZM14 7.5V13a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V7.5A3.5 3.5 0 0 1 5.5 4h5A3.5 3.5 0 0 1 14 7.5Z"/>' +
                '</svg>' +
                '</div>' +
                '<div class="ip-address">' + ip.IpAddress + '</div>' +
                '<div class="ip-desc">' + (ip.Description || 'Server') + '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
        });

        elements.serverIpContainer.html(html);
    }

    // Select server IP and load databases
    window.selectServerIp = function (id, ipAddress) {
        state.selectedServerIpId = id;
        state.selectedServerIpAddress = ipAddress;

        // Update UI
        $('.ip-card').removeClass('selected');
        $('.ip-card[data-id="' + id + '"]').addClass('selected');

        // Update display
        $('#selectedServerIpDisplay').text(ipAddress);
        $('#step3ServerDisplay').text(ipAddress);

        // Go to step 2
        goToStep(2);

        // Load databases
        loadDatabases(id);
    };

    function loadDatabases(serverIpId) {
        elements.databaseContainer.html(
            '<div class="col-12 text-center py-5">' +
            '<div class="spinner-border text-primary" role="status"></div>' +
            '<p class="mt-2 text-muted">Loading databases...</p>' +
            '</div>'
        );

        $.ajax({
            url: APP.baseUrl + 'BranchReprocess/GetDatabases',
            type: 'GET',
            data: { serverIpId: serverIpId },
            success: function (response) {
                if (response.Errors && response.Errors.length > 0) {
                    showError(elements.databaseContainer, response.Errors[0]);
                    return;
                }
                renderDatabases(response.Data);
            },
            error: function (xhr) {
                showError(elements.databaseContainer, 'Failed to load databases');
            }
        });
    }

    function renderDatabases(databases) {
        if (!databases || databases.length === 0) {
            elements.databaseContainer.html(
                '<div class="col-12 text-center py-5">' +
                '<p class="text-muted">No databases available on this server</p>' +
                '</div>'
            );
            return;
        }

        var html = '';
        databases.forEach(function (db) {
            html += '<div class="col-md-4 col-lg-3 mb-3">' +
                '<div class="db-card" data-name="' + db.DatabaseName + '" onclick="selectDatabase(\'' + db.DatabaseName + '\')">'+
                '<div class="text-center">' +
                '<div class="db-icon">' +
                '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="currentColor" viewBox="0 0 16 16">' +
                '<path d="M12.5 16a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Zm.5-5v1h1a.5.5 0 0 1 0 1h-1v1a.5.5 0 0 1-1 0v-1h-1a.5.5 0 0 1 0-1h1v-1a.5.5 0 0 1 1 0Z"/>' +
                '<path d="M12.096 6.223A4.92 4.92 0 0 0 13 5.698V7c0 .289-.213.654-.753 1.007a4.493 4.493 0 0 1 1.753.25V4c0-1.007-.875-1.755-1.904-2.223C11.022 1.289 9.573 1 8 1s-3.022.289-4.096.777C2.875 2.245 2 2.993 2 4v9c0 1.007.875 1.755 1.904 2.223C4.978 15.71 6.427 16 8 16c.536 0 1.058-.034 1.555-.097a4.525 4.525 0 0 1-.813-.927C8.5 14.992 8.252 15 8 15c-1.464 0-2.766-.27-3.682-.687C3.356 13.875 3 13.373 3 13v-1.302c.271.202.58.378.904.525C4.978 12.71 6.427 13 8 13h.027a4.552 4.552 0 0 1 0-1H8c-1.464 0-2.766-.27-3.682-.687C3.356 10.875 3 10.373 3 10V8.698c.271.202.58.378.904.525C4.978 9.71 6.427 10 8 10c.262 0 .52-.008.774-.024a4.525 4.525 0 0 1 1.102-1.132C9.298 8.944 8.666 9 8 9c-1.464 0-2.766-.27-3.682-.687C3.356 7.875 3 7.373 3 7V5.698c.271.202.58.378.904.525C4.978 6.711 6.427 7 8 7s3.022-.289 4.096-.777ZM3 4c0-.374.356-.875 1.318-1.313C5.234 2.271 6.536 2 8 2s2.766.27 3.682.687C12.644 3.125 13 3.627 13 4c0 .374-.356.875-1.318 1.313C10.766 5.729 9.464 6 8 6s-2.766-.27-3.682-.687C3.356 4.875 3 4.373 3 4Z"/>' +
                '</svg>' +
                '</div>' +
                '<div class="db-name">' + db.DatabaseName + '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
        });

        elements.databaseContainer.html(html);
    }

    // Select database and load last month
    window.selectDatabase = function (databaseName) {
        state.selectedDatabaseName = databaseName;

        // Update UI
        $('.db-card').removeClass('selected');
        $('.db-card[data-name="' + databaseName + '"]').addClass('selected');

        // Update displays
        $('#selectedServerIpDisplay').text(state.selectedServerIpAddress);
        $('#step3ServerDisplay').text(state.selectedServerIpAddress);
        $('#step3DatabaseDisplay').text(databaseName);

        // Go to step 3
        goToStep(3);

        // Fetch last month date
        fetchLastMonth();
    };

    function fetchLastMonth() {
        $.ajax({
            url: APP.baseUrl + 'BranchReprocess/GetLastMonth',
            type: 'GET',
            data: {
                serverIpId: state.selectedServerIpId,
                databaseName: state.selectedDatabaseName
            },
            success: function (response) {
                if (response.Data) {
                    elements.monthStartDateInput.val(response.Data);
                } else {
                    // Default to last month if failed
                    var d = new Date();
                    d.setMonth(d.getMonth() - 1);
                    d.setDate(1);
                    elements.monthStartDateInput.val(d.toISOString().slice(0, 10));
                }
            }
        });
    }

    // Load problem branches
    window.loadProblemBranches = function () {
        var month = elements.monthStartDateInput.val();
        var loc = elements.locationIdInput.val();

        if (!month) {
            Swal.fire('Warning', 'Please select a Month Start Date', 'warning');
            return;
        }

        elements.dataLoadingSpinner.show();
        elements.dataTableContainer.hide();
        elements.noDataMessage.hide();
        elements.recordCount.text('0 records');

        var payload = {
            ServerIpId: state.selectedServerIpId,
            DatabaseName: state.selectedDatabaseName,
            MonthStartDate: month,
            LocationId: loc
        };

        $.ajax({
            url: APP.baseUrl + 'BranchReprocess/LoadProblemBranches',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (response) {
                elements.dataLoadingSpinner.hide();

                if (response.Errors && response.Errors.length > 0) {
                    Swal.fire('Error', response.Errors[0], 'error');
                    return;
                }

                state.problemBranches = response.Data || [];
                renderDataTable(state.problemBranches);
            },
            error: function (xhr) {
                elements.dataLoadingSpinner.hide();
                Swal.fire('Error', 'Failed to load problem branches', 'error');
            }
        });
    };

    function renderDataTable(data) {
        if (!data || data.length === 0) {
            elements.noDataMessage.show();
            return;
        }

        elements.recordCount.text(data.length + ' records');
        var html = '';
        data.forEach(function (item) {
            html += '<tr>' +
                '<td>' + (item.PeriodFrom || '') + '</td>' +
                '<td>' + (item.BranchCode || '') + '</td>' +
                '<td>' + (item.BranchName || '') + '</td>' +
                '<td>' + (item.Remarks || '') + '</td>' +
                '<td><button class="reprocess-btn" onclick="reprocessBranch(\'' + item.BranchCode + '\')">Reprocess</button></td>' +
                '</tr>';
        });

        elements.dataTableBody.html(html);
        elements.dataTableContainer.show();
    }

    // Reprocess branch
    window.reprocessBranch = function (branchCode) {
        var monthDate = new Date(elements.monthStartDateInput.val());
        // Format Month: "MMM yyyy"
        var monthStr = monthDate.toLocaleString('default', { month: 'short', year: 'numeric' });

        // Prev Month
        var prevDate = new Date(monthDate);
        prevDate.setMonth(prevDate.getMonth() - 1);
        var prevMonthStr = prevDate.toLocaleString('default', { month: 'short', year: 'numeric' });

        Swal.fire({
            title: 'Confirm Reprocess',
            text: 'Reprocess branch ' + branchCode + ' for ' + monthStr + '?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, Reprocess',
            confirmButtonColor: '#ee5a24'
        }).then((result) => {
            if (result.isConfirmed) {
                executeReprocess(branchCode, monthStr, prevMonthStr);
            }
        });
    };

    function executeReprocess(branchCode, month, prevMonth) {
        showLoading('Reprocessing...', 'Please wait');

        var payload = {
            ServerIpId: state.selectedServerIpId,
            DatabaseName: state.selectedDatabaseName,
            BranchCode: branchCode,
            Month: month,
            PrevMonth: prevMonth
        };

        $.ajax({
            url: APP.baseUrl + 'BranchReprocess/ReprocessBranch',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (response) {
                hideLoading();
                if (response.Errors && response.Errors.length > 0) {
                    Swal.fire('Error', response.Errors[0], 'error');
                } else {
                    Swal.fire('Success', response.Message || 'Branch reprocessed successfully', 'success');
                }
            },
            error: function () {
                hideLoading();
                Swal.fire('Error', 'Failed to reprocess branch', 'error');
            }
        });
    }

    // Step navigation
    window.goToStep = function (step) {
        elements.step1Section.hide();
        elements.step2Section.hide();
        elements.step3Section.hide();

        elements.step1Indicator.removeClass('active completed');
        elements.step2Indicator.removeClass('active completed');
        elements.step3Indicator.removeClass('active completed');

        switch (step) {
            case 1:
                elements.step1Section.show();
                elements.step1Indicator.addClass('active');
                break;
            case 2:
                elements.step2Section.show();
                elements.step1Indicator.addClass('completed');
                elements.step2Indicator.addClass('active');
                break;
            case 3:
                elements.step3Section.show();
                elements.step1Indicator.addClass('completed');
                elements.step2Indicator.addClass('completed');
                elements.step3Indicator.addClass('active');
                break;
        }
    };

    function showLoading(text, subtext) {
        $('#loadingText').text(text || 'Processing...');
        $('#loadingSubtext').text(subtext || 'Please wait');
        elements.loadingOverlay.show();
    }

    function hideLoading() {
        elements.loadingOverlay.hide();
    }

    function showError(container, message) {
        container.html(
            '<div class="col-12 text-center py-5">' +
            '<div class="text-danger">' +
            '<p>' + message + '</p>' +
            '</div>' +
            '</div>'
        );
    }
})();
