using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using IdentityModel;
using IdentityService.Models;

namespace IdentityService.Pages.Register
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Index(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public RegisterViewModel InputUser { get; set; }

        [BindProperty]
        public bool RegisterSuccess { get; set; }

        public IActionResult OnGet(string returnUrl)
        {
            InitializeViewModel(returnUrl);
            return Page();
        }

        private void InitializeViewModel(string returnUrl)
        {
            InputUser = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };
        }

        public async Task<IActionResult> OnPost()
        {
            if (IsInvalidRegistration()) return Redirect("~/");

            var registrationResult = await RegisterUserAsync();

            if (registrationResult)
            {
                RegisterSuccess = true;
            }

            return Page();
        }

        private bool IsInvalidRegistration()
        {
            return InputUser.Button != "register" || !ModelState.IsValid;
        }

        private async Task<bool> RegisterUserAsync()
        {
            var user = CreateUserInstance();
            var result = await _userManager.CreateAsync(user, InputUser.Password);

            if (result.Succeeded)
            {
                var claims = new[]
                {
                    new Claim(JwtClaimTypes.Name, InputUser.FullName)
                };

                await _userManager.AddClaimsAsync(user, claims);
                return true;
            }
            return false;
        }

        private ApplicationUser CreateUserInstance()
        {
            return new ApplicationUser
            {
                UserName = InputUser.Username,
                Email = InputUser.Email,
                EmailConfirmed = true
            };
        }
    }
}
