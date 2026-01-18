/* ============================
   Admin Database Configurations
============================ */
var currentPage = 1;
var pageSize = 20;
var isEdit = false;

$(function () {
    loadConfigs(1);
    loadCompanies();

    $('#saveConfigBtn').on('click', saveConfig);
});

function loadConfigs(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'AdminDatabaseConfigurations/GetAll', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#configsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="9" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="9">No configurations found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            // Note: We don't have the password yet, so we just pass the ID to togglePassword
            var passwordMask = '<span class="pwd-toggle text-primary" style="cursor:pointer; font-weight:bold;" onclick="togglePassword(this, ' + item.Id + ')">***</span>';
            
            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + item.CompanyName + '</td>' +
                '<td>' + item.DatabaseIP + '</td>' +
                '<td>' + item.DatabaseName + '</td>' +
                '<td>' + item.DatabaseUserId + '</td>' +
                '<td>' + passwordMask + '</td>' +
                '<td>' + formatDateTime(item.CreatedAt) + '</td>' +
                '<td>' + formatDateTime(item.UpdatedAt) + '</td>' +
                '<td>' +
                    '<button class="btn btn-sm btn-info me-1" onclick="openEditModal(' + item.Id + ')">Edit</button>' +
                    '<button class="btn btn-sm btn-danger" onclick="deleteConfig(' + item.Id + ')">Delete</button>' +
                '</td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function togglePassword(element, configId) {
    var $el = $(element);
    
    // If it's currently masked (***), fetch and show
    if ($el.text() === '***') {
        $el.text('Loading...');
        
        $.get(APP.baseUrl + 'AdminDatabaseConfigurations/GetPassword', { id: configId }, function(res) {
            if (res.Data) {
                $el.text(res.Data);
                // Auto-hide after 10 seconds for security
                setTimeout(function() {
                    if ($el.text() !== '***') {
                        $el.text('***');
                    }
                }, 10000);
            } else {
                $el.text('Error');
                setTimeout(function() { $el.text('***'); }, 2000);
            }
        }).fail(function() {
            $el.text('Error');
            setTimeout(function() { $el.text('***'); }, 2000);
        });
        
    } else {
        // If it's already showing the password (or loading/error), hide it
        $el.text('***');
    }
}

function loadCompanies() {
    $.get(APP.baseUrl + 'AdminDatabaseConfigurations/GetCompanies', function (res) {
        var select = $('#companyId');
        select.find('option:not(:first)').remove(); // Keep default option

        if (res.Data) {
            $.each(res.Data, function (_, item) {
                select.append('<option value="' + item.Id + '">' + item.Name + '</option>');
            });
        }
    });
}

function openCreateModal() {
    isEdit = false;
    $('#modalTitle').text('Add Configuration');
    $('#configId').val('');
    $('#configForm')[0].reset();
    $('#passwordHelp').text('Required for new configurations.');
    $('#databasePassword').prop('required', true);
    
    new bootstrap.Modal(document.getElementById('configModal')).show();
}

function openEditModal(id) {
    isEdit = true;
    $('#modalTitle').text('Edit Configuration');
    $('#passwordHelp').text('Leave blank to keep existing password.');
    $('#databasePassword').prop('required', false);

    $.get(APP.baseUrl + 'AdminDatabaseConfigurations/Get', { id: id }, function (res) {
        if (res.Data) {
            var data = res.Data;
            $('#configId').val(data.Id);
            $('#companyId').val(data.CompanyId);
            $('#databaseIP').val(data.DatabaseIP);
            $('#databaseName').val(data.DatabaseName);
            $('#databaseUserId').val(data.DatabaseUserId);
            $('#databasePassword').val(''); // Don't show password

            new bootstrap.Modal(document.getElementById('configModal')).show();
        } else {
            Swal.fire('Error', res.Message, 'error');
        }
    });
}

function saveConfig() {
    var id = $('#configId').val();
    var companyId = $('#companyId').val();
    var ip = $('#databaseIP').val();
    var dbName = $('#databaseName').val();
    var userId = $('#databaseUserId').val();
    var password = $('#databasePassword').val();

    if (!companyId || !ip || !dbName || !userId) {
        Swal.fire('Validation', 'Please fill all required fields', 'warning');
        return;
    }

    if (!isEdit && !password) {
        Swal.fire('Validation', 'Password is required for new configurations', 'warning');
        return;
    }

    var url = isEdit ? 'AdminDatabaseConfigurations/Update' : 'AdminDatabaseConfigurations/Create';
    var data = {
        Id: id,
        CompanyId: companyId,
        DatabaseIP: ip,
        DatabaseName: dbName,
        DatabaseUserId: userId,
        DatabasePassword: password
    };

    $.ajax({
        url: APP.baseUrl + url,
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
            } else {
                Swal.fire('Success', res.Message, 'success');
                bootstrap.Modal.getInstance(document.getElementById('configModal')).hide();
                loadConfigs(currentPage);
            }
        },
        error: function () {
            Swal.fire('Error', 'Operation failed', 'error');
        }
    });
}

function deleteConfig(id) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminDatabaseConfigurations/Delete',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Deleted!', res.Message, 'success');
                        loadConfigs(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Delete failed', 'error');
                }
            });
        }
    });
}

function renderPagination(totalRecords, page, pageSize) {
    var totalPages = Math.ceil(totalRecords / pageSize);
    var pagination = $('#pagination').empty();

    if (totalPages <= 1) return;

    for (var i = 1; i <= totalPages; i++) {
        var activeClass = i === page ? 'active' : '';
        pagination.append(
            '<button class="btn btn-sm btn-outline-primary me-1 ' + activeClass +
            '" onclick="loadConfigs(' + i + ')">' + i + '</button>'
        );
    }
}