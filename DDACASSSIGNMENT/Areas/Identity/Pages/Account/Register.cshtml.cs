using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DDACASSSIGNMENT.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;


        private CloudBlobContainer GetCloudBlobContainer()
        {
            // step 1: read appsettings:json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build();

            //step 2: to get key access from the appsettings.json and put inside the code
            CloudStorageAccount accountdetails = CloudStorageAccount.Parse(configure["ConnectionStrings:blobstorageconnection"]);

            //step 3: how to refer to an existing / new container in the blob storage account.
            CloudBlobClient clientagent = accountdetails.CreateCloudBlobClient();
            CloudBlobContainer container = clientagent.GetContainerReference("users");

            return container;
        }

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<Microsoft.AspNetCore.Authentication.AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            public string City { get; set; }

            [Required]
            public string PhoneNumber { get; set; }

            public string Role { get; set; }

            public IEnumerable<SelectListItem> RoleList { get; set; }

            public string ProfilePicture { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            Input = new InputModel()
            {
                RoleList = _roleManager.Roles.Where(u => u.Name != SD.Role_Customer).Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                }),

            };
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {


                //link to the correct storage account first
                CloudBlobContainer container = GetCloudBlobContainer();
                //create container
                if (!await container.ExistsAsync()) { await container.CreateIfNotExistsAsync(); }
                CloudBlockBlob blobitem = null;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    try
                    {
                        blobitem = container.GetBlockBlobReference(Input.Email + ".jpg");
                        var stream = files[0].OpenReadStream();
                        blobitem.UploadFromStreamAsync(stream).Wait();
                        Input.ProfilePicture = blobitem.Uri.ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Name = Input.Name,
                    PhoneNumber = Input.PhoneNumber,
                    Address = Input.Address,
                    City = Input.City,
                    Role = Input.Role,
                    ProfilePicture = Input.ProfilePicture
                };

                if(User.IsInRole(SD.Role_Admin))
                {
                    user.EmailConfirmed = true;
                }
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));

                    }
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Customer))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                    }
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Worker))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Worker));
                    }

                    

                    if (user.Role == null)
                    {
                        if (_context.ApplicationUsers.ToList().Count == 1)
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_Admin);
                        }
                        else if (User.IsInRole(SD.Role_Admin))
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_Worker);
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                        }
                            
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, user.Role);
                    }


                    _logger.LogInformation("User created a new account with password.");


                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                    
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        if (!user.EmailConfirmed)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            return RedirectToAction("Index", "User", new { Area = "Admin" });
                        };
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
