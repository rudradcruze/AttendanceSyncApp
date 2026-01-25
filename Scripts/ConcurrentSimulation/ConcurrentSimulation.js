// Concurrent Simulation JavaScript
(function () {
    'use strict';

    // State management
    var state = {
        selectedServerIpId: null,
        selectedServerIpAddress: null,
        selectedDatabaseName: null,
        periodEndData: []
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
        hitButtonContainer: null,
        recordCount: null,
        loadingOverlay: null,
        resultContainer: null
    };

    // Initialize on document ready
    $(document).ready(function () {
        initializeElements();
        loadServerIps();
        setupEventHandlers();
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
        elements.hitButtonContainer = $('#hitButtonContainer');
        elements.recordCount = $('#recordCount');
        elements.loadingOverlay = $('#loadingOverlay');
        elements.resultContainer = $('#resultContainer');
    }

    function setupEventHandlers() {
        $('#confirmHitBtn').on('click', function () {
            $('#confirmModal').modal('hide');
            executeHitConcurrent();
        });
    }

    // Load server IPs
    function loadServerIps() {
        $.ajax({
            url: APP.baseUrl + 'ConcurrentSimulation/GetServerIps',
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
            url: APP.baseUrl + 'ConcurrentSimulation/GetDatabases',
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
                '<div class="db-card" data-name="' + db.DatabaseName + '" onclick="selectDatabase(\'' + db.DatabaseName + '\')">' +
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

    // Select database and load data
    window.selectDatabase = function (databaseName) {
        state.selectedDatabaseName = databaseName;

        // Update UI
        $('.db-card').removeClass('selected');
        $('.db-card[data-name="' + databaseName + '"]').addClass('selected');

        // Update displays
        $('#step3ServerDisplay').text(state.selectedServerIpAddress);
        $('#step3DatabaseDisplay').text(databaseName);

        // Go to step 3
        goToStep(3);

        // Load period end data
        loadPeriodEndData();
    };

    function loadPeriodEndData() {
        // Show loading, hide others
        elements.dataLoadingSpinner.show();
        elements.dataTableContainer.hide();
        elements.noDataMessage.hide();
        elements.hitButtonContainer.hide();
        elements.resultContainer.html('');

        $.ajax({
            url: APP.baseUrl + 'ConcurrentSimulation/GetPeriodEndData',
            type: 'GET',
            data: {
                serverIpId: state.selectedServerIpId,
                databaseName: state.selectedDatabaseName
            },
            success: function (response) {
                elements.dataLoadingSpinner.hide();

                if (response.Errors && response.Errors.length > 0) {
                    elements.noDataMessage.find('h6').text('Error');
                    elements.noDataMessage.find('p').text(response.Errors[0]);
                    elements.noDataMessage.show();
                    return;
                }

                state.periodEndData = response.Data || [];
                renderDataTable(state.periodEndData);
            },
            error: function (xhr) {
                elements.dataLoadingSpinner.hide();
                elements.noDataMessage.find('h6').text('Error');
                elements.noDataMessage.find('p').text('Failed to load period end data');
                elements.noDataMessage.show();
            }
        });
    }

    function renderDataTable(data) {
        if (!data || data.length === 0) {
            elements.noDataMessage.show();
            elements.recordCount.text('0 records');
            return;
        }

        elements.recordCount.text(data.length + ' records');

        var html = '';
        data.forEach(function (entry, index) {
            html += '<tr>' +
                '<td>' + (index + 1) + '</td>' +
                '<td>' + entry.UserId + '</td>' +
                '<td>' + entry.Branch_Id + '</td>' +
                '<td>' + entry.Location_Id + '</td>' +
                '<td>' + entry.CompanyId + '</td>' +
                '<td><span class="badge bg-secondary">' + entry.Status + '</span></td>' +
                '<td>' + entry.EmployeeId + '</td>' +
                '<td><span class="badge bg-info">' + entry.PostProcessStatus + '</span></td>' +
                '</tr>';
        });

        elements.dataTableBody.html(html);
        elements.dataTableContainer.show();
        elements.hitButtonContainer.show();
    }

    // Hit concurrent - show confirmation
    window.hitConcurrent = function () {
        if (!state.periodEndData || state.periodEndData.length === 0) {
            alert('No data to insert');
            return;
        }

        $('#confirmRecordCount').text(state.periodEndData.length);
        $('#confirmModal').modal('show');
    };

    // Execute the concurrent insert
    function executeHitConcurrent() {
        showLoading('Inserting Records...', 'Processing ' + state.periodEndData.length + ' records simultaneously');

        var requestData = {
            ServerIpId: state.selectedServerIpId,
            DatabaseName: state.selectedDatabaseName
        };

        $.ajax({
            url: APP.baseUrl + 'ConcurrentSimulation/HitConcurrent',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (response) {
                hideLoading();

                if (response.Errors && response.Errors.length > 0) {
                    showResult(false, response.Errors[0], null);
                    return;
                }

                var data = response.Data;
                showResult(true, response.Message, data);

                // Disable hit button after success
                $('#hitConcurrentBtn').prop('disabled', true).text('Completed');
            },
            error: function (xhr) {
                hideLoading();
                showResult(false, 'Failed to execute concurrent insert', null);
            }
        });
    }

    function showResult(success, message, data) {
        var html = '';

        if (success) {
            html = '<div class="result-success">' +
                '<div class="d-flex align-items-center mb-2">' +
                '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor" class="me-2" viewBox="0 0 16 16">' +
                '<path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/>' +
                '</svg>' +
                '<strong>Success!</strong>' +
                '</div>' +
                '<p class="mb-2">' + message + '</p>';

            if (data) {
                html += '<div class="row text-center">' +
                    '<div class="col-4">' +
                    '<div class="fw-bold fs-4">' + data.TotalRecords + '</div>' +
                    '<small>Total Records</small>' +
                    '</div>' +
                    '<div class="col-4">' +
                    '<div class="fw-bold fs-4 text-success">' + data.SuccessCount + '</div>' +
                    '<small>Successful</small>' +
                    '</div>' +
                    '<div class="col-4">' +
                    '<div class="fw-bold fs-4 text-danger">' + data.FailedCount + '</div>' +
                    '<small>Failed</small>' +
                    '</div>' +
                    '</div>';

                if (data.Errors && data.Errors.length > 0) {
                    html += '<div class="mt-3"><strong>Errors:</strong><ul class="mb-0">';
                    data.Errors.forEach(function (err) {
                        html += '<li class="small">' + err + '</li>';
                    });
                    html += '</ul></div>';
                }
            }

            html += '</div>';
        } else {
            html = '<div class="result-error">' +
                '<div class="d-flex align-items-center mb-2">' +
                '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor" class="me-2" viewBox="0 0 16 16">' +
                '<path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/>' +
                '</svg>' +
                '<strong>Error</strong>' +
                '</div>' +
                '<p class="mb-0">' + message + '</p>' +
                '</div>';
        }

        elements.resultContainer.html(html);
    }

    // Step navigation
    window.goToStep = function (step) {
        // Hide all sections
        elements.step1Section.hide();
        elements.step2Section.hide();
        elements.step3Section.hide();

        // Reset all indicators
        elements.step1Indicator.removeClass('active completed');
        elements.step2Indicator.removeClass('active completed');
        elements.step3Indicator.removeClass('active completed');

        // Show selected section and update indicators
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

    // Helper functions
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
            '<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" class="mb-3" viewBox="0 0 16 16">' +
            '<path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/>' +
            '</svg>' +
            '<p>' + message + '</p>' +
            '</div>' +
            '</div>'
        );
    }

})();
