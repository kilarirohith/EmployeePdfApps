using BCrypt.Net;
using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Models;
using EmployeeCrudPdf.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace EmployeeCrudPdf.Controllers
{
    public class AccountMvcController : Controller
    {
        private readonly IUserRepository _users;
        private readonly IJwtService _jwt;

        public AccountMvcController(IUserRepository users, IJwtService jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginViewModel { ReturnUrl = returnUrl });

        [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _users.GetByUsernameOrEmailAsync(vm.UsernameOrEmail);
            if (user == null || !BCrypt.Net.BCrypt.Verify(vm.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View(vm);
            }

            HttpContext.Session.SetInt32("auth_user_id", user.Id);
            HttpContext.Session.SetString("auth_user", user.Username);
            HttpContext.Session.SetString("auth_email", user.Email);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            var token = _jwt.CreateToken(user);
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // set true on HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8),
                IsEssential = true
            });

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Employees");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var byUsername = await _users.GetByUsernameOrEmailAsync(vm.Username);
            var byEmail = await _users.GetByUsernameOrEmailAsync(vm.Email);
            if (byUsername != null || byEmail != null)
            {
                ModelState.AddModelError("", "User already exists");
                return View(vm);
            }

            var user = new User
            {
                Username = vm.Username,
                Email = vm.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                CreatedAt = DateTime.UtcNow
            };
            user.Id = await _users.CreateAsync(user);

            HttpContext.Session.SetInt32("auth_user_id", user.Id);
            HttpContext.Session.SetString("auth_user", user.Username);
            HttpContext.Session.SetString("auth_email", user.Email);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            var token = _jwt.CreateToken(user);
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true, Secure = false, SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8), IsEssential = true
            });

            return RedirectToAction("Index", "Employees");
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            if (Request.Cookies.ContainsKey("access_token"))
                Response.Cookies.Delete("access_token");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}