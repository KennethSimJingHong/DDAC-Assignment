var dataTable;

dataTable = $("#tblData").DataTable({
    "ajax": {
        "url": "/Admin/User/GetAll"
    },
    "columns": [
        { "data": "name", "width": "12%" },
        { "data": "email", "width": "12%" },
        { "data": "phoneNumber", "width": "12%" },
        { "data": "address", "width": "12%" },
        { "data": "city", "width": "12%" },
        { "data": "role", "width": "12%" },
        {
            "data": {id: "Id", lockoutEnd: "lockoutEnd"},
            "render": function (data) {
                var today = new Date().getTime();
                var lockout = new Date(data.lockoutEnd).getTime();
                    //user is currently locked
                if (lockout > today) {
                    //user is currently locked
                    return `
                    <div class="text-center">
                        <a onclick=ToggleLock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer;">
                            <i class="fas fa-lock-open"></i>
                        </a>
                        <a onclick=Delete("/Admin/User/Delete/${data.id}") class="btn btn-danger text-white" style="cursor:pointer">
                            <i class="fas fa-trash"></i>
                        </a>
                    </div>
                    `;
                }
                else {
                    return `
                    <div class="text-center">
                        <a onclick=ToggleLock('${data.id}') class="btn btn-success text-white" style="cursor:pointer;">
                            <i class="fas fa-lock"></i>
                        </a>
                        <a onclick=Delete("/Admin/User/Delete/${data.id}") class="btn btn-danger text-white" style="cursor:pointer">
                            <i class="fas fa-trash"></i>
                        </a>
                    </div>
                    `;
                }
            }, "width": "30%"
        }
    ]
});


function ToggleLock(id) {
    $.ajax({
        type: "POST",
        url: "/Admin/User/ToggleLock",
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            } else {
                toastr.error(data.message);
            }
        }
    })
}

function Delete(url) {
    swal({
        title: "Delete User",
        text: "Are you sure to delete?",
        buttons: true,
        dangerMode: true
    }).then((isDelete) => {
        if (isDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    } else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    });
}