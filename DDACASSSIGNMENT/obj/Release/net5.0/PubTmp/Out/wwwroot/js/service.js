var dataTable;


dataTable = $("#tblData").DataTable({
    "ajax": {
        "url": "/Admin/Service/GetAll"
    },
    "columns": [
        { "data": "price", "width": "15%" },
        { "data": "category.name", "width": "15%" },
        { "data": "size.name", "width": "15%" },
        { "data": "type", "width": "15%" },
        {
            "data": "id",
            "render": function (data) {
                return `
                    <div class="text-center">
                        <a href="/Admin/Service/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                            <i class="fas fa-edit"></i>
                        </a>
                        <a onclick=Delete("/Admin/Service/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
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
        title: "Delete Service",
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

