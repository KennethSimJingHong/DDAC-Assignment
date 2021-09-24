var dataTable;


dataTable = $("#tblData").DataTable({
    "ajax": {
        "url": "/Admin/Group/GetAll"
    },
    "columns": [
        { "data": "name", "width": "35%" },
        {
            "data": "id",
            "render": function (data) {
                return `
                    <div class="text-center">
                        <a href="/Admin/Group/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                            <i class="fas fa-edit"></i>
                        </a>
                        <a href="/Admin/Group/Manage/${data}" class="btn btn-warning text-white" style="cursor:pointer">
                            <i class="fas fa-users"></i>
                        </a>
                        <a onclick=Delete("/Admin/Group/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                            <i class="fas fa-trash"></i>
                        </a>
                    </div>
    
                `;
            }, "width": "30%"
        }
    ]
});

function Delete(url) {

    swal({
        title: "Delete Group",
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

