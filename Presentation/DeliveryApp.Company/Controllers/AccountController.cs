using DeliveryApp.Application.Abstractions.Services;
using DeliveryApp.Application.DTOs.User;
using DeliveryApp.Application.ViewModels;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Infrastructure.Enums;
using DeliveryApp.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DeliveryApp.Company.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IAuthService _authService;
        readonly UserManager<AppUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        public AccountController(ICompanyService userService, IAuthService authService, 
            UserManager<AppUser> userManager,  RoleManager<IdentityRole> roleManager)
        {
            _companyService = userService;
            _authService = authService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Login()
        {
            return View();
        }


        public IActionResult Register()
        {
            return View();
        }

		[HttpPost]
        public async Task<IActionResult> Register(RegisterCompanyVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);
            try
            {
                CreateUserResponse response = await _companyService.CreateAsync(registerVM);
                 return RedirectToAction("login");
            }
            catch (Exception)
            {

                return View(registerVM);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto,string returnUrl)
        {

            try
            {
                await _authService.LoginAsync(loginDto);
                if (returnUrl != null) return Redirect(returnUrl);
                return RedirectToAction("index", "dashboard");
            }
            catch (Exception)
            {
                return View(loginDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult>SetAddress([FromBody]AddressVM address)
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            var company = _companyService.GetCompany(user.Id);

            try
            {
                await _companyService.SetAddress(address, company.Id);
                return Ok();
               
            }
            catch (Exception)
            {
                return RedirectToAction("index", "dashboard");
            }
            

        }



        public async Task<IActionResult> Logout()
        {
            if (!await _authService.Logout()) return BadRequest();

            return RedirectToAction("index", "dashboard");
        }

        public  async Task<IActionResult> Profile()
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);

            ProfileVM profile = new()
            {
                User = user,
                Company = _companyService.GetCompany(user.Id)
            };


            return View(profile);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateCompanyDto updateCompany)
        {

            if (!ModelState.IsValid) return RedirectToAction("profile", updateCompany);
            try
            {
                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                updateCompany.Id = userid;
                await _companyService.UpdateAsync(updateCompany);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

            return RedirectToAction("profile");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhoto(IFormFile Photo)
        {
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid) return BadRequest();

            try
            {
                await _companyService.UpdatePhotoAsync(Photo, userid);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return RedirectToAction("profile");
        }

        public async Task CreateRole()
        {
            foreach (var item in Enum.GetValues(typeof(AppRole)))
            {
                if (!await _roleManager.RoleExistsAsync(item.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = item.ToString() });
                }
            };

        }

        public async Task<IActionResult> Balance()
        {
            return View();
        }
    }
}
