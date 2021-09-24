using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using DDACASSSIGNMENT.Models;

namespace DDACASSSIGNMENT.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
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

        public CloudTable GetCloudTable()
        {
            // step 1: read appsettings:json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build();

            //step 2: to get key access from the appsettings.json and put inside the code
            CloudStorageAccount accountdetails = CloudStorageAccount.Parse(configure["ConnectionStrings:blobstorageconnection"]);

            //step 3: how to refer to an existing / new container in the blob storage account.
            CloudTableClient clientagent = accountdetails.CreateCloudTableClient();
            CloudTable table = clientagent.GetTableReference("logs");

            return table;
        }

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            public string City { get; set; }

            [Required]
            public string PhoneNumber { get; set; }

            public string ProfilePicture { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        Name = info.Principal.FindFirstValue(ClaimTypes.Name),
                        ProfilePicture = info.Principal.FindFirstValue(ClaimTypes.UserData)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {

                //link to the correct storage account first
                CloudBlobContainer container = GetCloudBlobContainer();
                //create container
                await container.CreateIfNotExistsAsync();
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
                    EmailConfirmed = true,
                    ProfilePicture = Input.ProfilePicture
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {

                    //link to the correct storage account first
                    CloudTable table = GetCloudTable();
                    //create table
                    if (!await table.ExistsAsync()) { await table.CreateIfNotExistsAsync(); }
                    DateTime now = DateTime.Now;
                    string dt = now.ToString("MMddyyyyHHmmss");
                    Log log = new Log(Input.Email, dt);
                    log.Status = "LOG IN";

                    try
                    {
                        TableOperation insert = TableOperation.Insert(log);
                        TableResult insertResult = table.ExecuteAsync(insert).Result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
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

                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {

                        //link to the correct storage account first
                        CloudTable table1 = GetCloudTable();
                        //create table
                        if (!await table1.ExistsAsync()) { await table1.CreateIfNotExistsAsync(); }
                        DateTime now1 = DateTime.Now;
                        string dt1 = now1.ToString("MMddyyyyHHmmss");
                        Log log1 = new Log(Input.Email, dt1);
                        log1.Status = "LOG IN";

                        try
                        {
                            TableOperation insert1 = TableOperation.Insert(log1);
                            TableResult insertResult1 = table1.ExecuteAsync(insert1).Result;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
