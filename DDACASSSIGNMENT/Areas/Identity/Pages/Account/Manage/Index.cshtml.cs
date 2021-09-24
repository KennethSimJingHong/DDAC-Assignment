using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DDACASSSIGNMENT.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            public string City { get; set; }

            public string ProfilePicture { get; set; }
        }

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

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            string image = null;
            Username = userName;
            CloudBlobContainer container = GetCloudBlobContainer();
            //create container
            if (!await container.ExistsAsync()) { await container.CreateIfNotExistsAsync(); }
            BlobResultSegment itemslisting = container.ListBlobsSegmentedAsync(null).Result;

            foreach(IListBlobItem item in itemslisting.Results)
            {
                if(item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    if (Path.GetExtension(blob.Name) == ".jpg")
                    {
                        if (blob.Uri.ToString().Contains(userName))
                        {
                            image = blob.Uri.ToString();
                        }
                    }
                }
            }



            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Name = user.Name,
                Address = user.Address,
                City = user.City,
                ProfilePicture = image
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user = (ApplicationUser) await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = (ApplicationUser) await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            if (Input.Name != user.Name)
            {
                user.Name = Input.Name; //name column in db refer to the current input value
            }
            if (Input.Address != user.Address)
            {
                user.Address = Input.Address; //name column in db refer to the current input value
            }
            if (Input.City != user.City)
            {
                user.City = Input.City; //name column in db refer to the current input value
            }

            if (Input.ProfilePicture != user.ProfilePicture)
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
                        blobitem = container.GetBlockBlobReference(user.UserName + ".jpg");
                        var stream = files[0].OpenReadStream();
                        blobitem.UploadFromStreamAsync(stream).Wait();
                        user.ProfilePicture = blobitem.Uri.ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                }
            }
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
