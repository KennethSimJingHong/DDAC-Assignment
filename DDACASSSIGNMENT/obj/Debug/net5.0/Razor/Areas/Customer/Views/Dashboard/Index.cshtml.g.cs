#pragma checksum "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "6c6ee4f09aebb9ab6c851adfbe9226b94c9cefc5"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_Customer_Views_Dashboard_Index), @"mvc.1.0.view", @"/Areas/Customer/Views/Dashboard/Index.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\_ViewImports.cshtml"
using DDACASSSIGNMENT;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\_ViewImports.cshtml"
using DDACASSSIGNMENT.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"6c6ee4f09aebb9ab6c851adfbe9226b94c9cefc5", @"/Areas/Customer/Views/Dashboard/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"3f110760eb7c708c3597209bb3ab418e9a8f7be9", @"/Areas/Customer/_ViewImports.cshtml")]
    public class Areas_Customer_Views_Dashboard_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<IEnumerable<DDACASSSIGNMENT.Models.Operation>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#nullable restore
#line 3 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
  
    ViewData["Title"] = "Index";
    Layout = "~/Views/Shared/_Layout.cshtml";

#line default
#line hidden
#nullable disable
            WriteLiteral(@"
<h1>Dashboard</h1><br><br>
<div class=""row"">
    <div class=""col-6"">
        <h2 class=""text-dark"">Purchase History</h2>
    </div>
</div>

<table class=""table"">
    <thead>
        <tr>
            <th>
                Service Type
            </th>
            <th>
                Category
            </th>
            <th>Size</th>
            <th>Total Price (RM)</th>
            <th>Subscription Duration *Main ONLY</th>
            <th>Purchase Date</th>
        </tr>
    </thead>
    <tbody>
");
#nullable restore
#line 31 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
 foreach (var item in Model) {

#line default
#line hidden
#nullable disable
            WriteLiteral("        <tr>\r\n            <td>\r\n                ");
#nullable restore
#line 34 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
           Write(Html.DisplayFor(modelItem => item.Order.Service.Type));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n            </td>\r\n            <td>\r\n                <img");
            BeginWriteAttribute("src", " src=\"", 865, "\"", 938, 1);
#nullable restore
#line 37 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
WriteAttributeValue("", 871, Html.DisplayFor(modelItem => item.Order.Service.Category.ImageUrl), 871, 67, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" width=\"200\"/>\r\n                <p><br> Category: ");
#nullable restore
#line 38 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
                             Write(Html.DisplayFor(modelItem => item.Order.Service.Category.Name));

#line default
#line hidden
#nullable disable
            WriteLiteral("</p>\r\n\r\n            </td>\r\n            <td>\r\n                ");
#nullable restore
#line 42 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
           Write(Html.DisplayFor(modelItem => item.Order.Service.Size.Name));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n            </td>\r\n            <td>\r\n                ");
#nullable restore
#line 45 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
           Write(Html.DisplayFor(modelItem => item.TotalPrice));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n            </td>\r\n            <td>\r\n                ");
#nullable restore
#line 48 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
           Write(Html.DisplayFor(modelItem => item.Duration));

#line default
#line hidden
#nullable disable
            WriteLiteral(" Months\r\n            </td>\r\n            <td>\r\n                ");
#nullable restore
#line 51 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
           Write(Html.DisplayFor(modelItem => item.OperationDate));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n            </td>\r\n        </tr>\r\n");
#nullable restore
#line 54 "C:\Users\hp\source\repos\DDACASSSIGNMENT-FINAL\DDACASSSIGNMENT\Areas\Customer\Views\Dashboard\Index.cshtml"
}

#line default
#line hidden
#nullable disable
            WriteLiteral("    </tbody>\r\n</table>\r\n");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<IEnumerable<DDACASSSIGNMENT.Models.Operation>> Html { get; private set; }
    }
}
#pragma warning restore 1591
