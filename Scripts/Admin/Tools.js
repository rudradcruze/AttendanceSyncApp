/* ============================
   Admin Tools Management
============================ */
var currentPage = 1;
var pageSize = 20;
var toolModal = null;

$(function () {
    toolModal = new bootstrap.Modal(document.getElementById('toolModal'));
    loadTools(1);

    $('#saveToolBtn').on('click', saveTool);
});

function loadTools(page) {
    currentPage = page;

    $.get(APP.baseUrl + 'AdminTools/GetTools', {
        page: page,
        pageSize: pageSize
    }, function (res) {
        var tbody = $('#toolsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="7" class="text-danger">' + res.Message + '</td></tr>');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="7">No tools found</td></tr>');
            return;
        }

        $.each(data.Data, function (_, item) {
            var statusBadge = item.IsActive
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-danger">Inactive</span>';

            var devBadge = item.IsUnderDevelopment
                ? '<span class="badge bg-warning text-dark">Under Development</span>'
                : '<span class="badge bg-info">Ready</span>';

            var actions =
                '<button class="btn btn-sm btn-primary me-1" onclick="editTool(' + item.Id + ')">Edit</button>' +
                '<button class="btn btn-sm ' + (item.IsActive ? 'btn-warning' : 'btn-success') + ' me-1" onclick="toggleStatus(' + item.Id + ')">' +
                (item.IsActive ? 'Deactivate' : 'Activate') +
                '</button>' +
                '<button class="btn btn-sm btn-danger" onclick="deleteTool(' + item.Id + ')">Delete</button>';

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + escapeHtml(item.Name) + '</td>' +
                '<td>' + escapeHtml(item.Description || 'N/A') + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + devBadge + '</td>' +
                '<td>' + formatDateTime(item.CreatedAt) + '</td>' +
                '<td>' + actions + '</td>' +
                '</tr>'
            );
        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);
    });
}

function showCreateModal() {
    $('#toolModalTitle').text('Add Tool');
    $('#toolId').val('');
    $('#toolName').val('');
    $('#toolDescription').val('');
    $('#toolActive').prop('checked', true);
    $('#toolUnderDevelopment').prop('checked', true);
    toolModal.show();
}

function editTool(id) {
    $.get(APP.baseUrl + 'AdminTools/GetTool', { id: id }, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
            return;
        }

        var tool = res.Data;
        $('#toolModalTitle').text('Edit Tool');
        $('#toolId').val(tool.Id);
        $('#toolName').val(tool.Name);
        $('#toolDescription').val(tool.Description || '');
        $('#toolActive').prop('checked', tool.IsActive);
        $('#toolUnderDevelopment').prop('checked', tool.IsUnderDevelopment);
        toolModal.show();
    });
}

function saveTool() {
    var id = $('#toolId').val();
    var name = $('#toolName').val().trim();
    var description = $('#toolDescription').val().trim();
    var isActive = $('#toolActive').prop('checked');
    var isUnderDevelopment = $('#toolUnderDevelopment').prop('checked');

    if (!name) {
        Swal.fire('Validation Error', 'Tool name is required', 'warning');
        return;
    }

    var url = id ? APP.baseUrl + 'AdminTools/UpdateTool' : APP.baseUrl + 'AdminTools/CreateTool';
    var data = id
        ? { Id: parseInt(id), Name: name, Description: description, IsActive: isActive, IsUnderDevelopment: isUnderDevelopment }
        : { Name: name, Description: description, IsActive: isActive, IsUnderDevelopment: isUnderDevelopment };

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.Errors && res.Errors.length > 0) {
                Swal.fire('Error', res.Message, 'error');
            } else {
                Swal.fire('Success', res.Message, 'success');
                toolModal.hide();
                loadTools(currentPage);
            }
        },
        error: function () {
            Swal.fire('Error', 'Failed to save tool', 'error');
        }
    });
}

function toggleStatus(id) {
    Swal.fire({
        title: 'Confirm',
        text: 'Are you sure you want to change this tool\'s status?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, change it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminTools/ToggleToolStatus',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Success', res.Message, 'success');
                        loadTools(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to update status', 'error');
                }
            });
        }
    });
}

function deleteTool(id) {
    Swal.fire({
        title: 'Delete Tool',
        text: 'Are you sure you want to delete this tool? This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete it'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: APP.baseUrl + 'AdminTools/DeleteTool',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.Errors && res.Errors.length > 0) {
                        Swal.fire('Error', res.Message, 'error');
                    } else {
                        Swal.fire('Deleted', res.Message, 'success');
                        loadTools(currentPage);
                    }
                },
                error: function () {
                    Swal.fire('Error', 'Failed to delete tool', 'error');
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
            '" onclick="loadTools(' + i + ')">' + i + '</button>'
        );
    }
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}
