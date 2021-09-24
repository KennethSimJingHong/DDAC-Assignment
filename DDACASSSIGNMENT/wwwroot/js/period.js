var dataTable;


dataTable = $("#tblData").DataTable({
    "ajax": {
        "url": "/Admin/Period/GetAll"
    },
    "columns": [
        { "data": "name", "width": "70%" },
        {
            "data": "id",
            "render": function (data) {
                return `
                    <div class="text-center">
                        <a href="/Admin/Period/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                            <i class="fas fa-edit"></i>
                        </a>
                        <a onclick=Delete("/Admin/Period/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
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
        title: "Delete Period",
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

