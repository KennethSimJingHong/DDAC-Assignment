var chart1 = document.getElementById('chart1').getContext('2d');
var chart2 = document.getElementById('chart2').getContext('2d');

$.ajax({
    url: "/Admin/Dashboard/GetAll",
    method: "GET",
    dataType: "json",
    success: function (data) {
        new Chart(chart1, {
            type: 'pie',
            data: {
                datasets: [{
                    data: data.data1,
                    backgroundColor: ["#Eeb346b", "#43AA8B", "#253D5B"],
                }],
                labels: ["Admin", "Driver", "Customer"],
            },
            options: {
                title: {
                    text: "Users",
                    display: true,
                }
            }
        });

        new Chart(chart2, {
            type: 'pie',
            data: {
                datasets: [{
                    data: [12, 32, 13, 42, 32, 54, 76, 12, 23, 31, 24, 45],
                    backgroundColor: ["#Eeb346b", "#43AA8B", "#253D5B"],
                }],
                labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            },
            options: {
                title: {
                    text: "Earning",
                    display: true,
                }
            }
        });
        
    },
})






//dataTable = $("#tblData").DataTable({
//    "ajax": {
//        "url": "/Admin/Category/GetAll"
//    },
//    "columns": [
//        { "data": "name", "width": "70%" },
//        {
//            "data": "id",
//            "render": function (data) {
//                return `
//                    <div class="text-center">
//                        <a href="/Admin/Category/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
//                            <i class="fas fa-edit"></i>
//                        </a>
//                        <a onclick=Delete("/Admin/Category/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
//                            <i class="fas fa-trash"></i>
//                        </a>
//                    </div>
    
//                `;
//            }, "width": "30%"
//        }
//    ]
//});

//function Delete(url) {

//    swal({
//        title: "Delete Category",
//        text: "Are you sure to delete?",
//        buttons: true,
//        dangerMode:true
//    }).then((isDelete)=>{
//        if (isDelete) {
//            $.ajax({
//                type: "DELETE",
//                url: url,
//                success: function (data) {
//                    if (data.success) {
//                        toastr.success(data.message);
//                        dataTable.ajax.reload();
//                    } else {
//                        toastr.error(data.message);
//                    }
//                }
//            })
//        }
//    });
//}

