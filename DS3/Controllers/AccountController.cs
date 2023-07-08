using Microsoft.AspNetCore.Mvc;
using DS3.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace DS3.Controllers
{
    
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        async public Task<IActionResult> LogIn(User modelUser)
        {
            User? user;
            List<Role> roles;
            using (UserDbContext db = new UserDbContext())
            {
                roles = db.Roles.ToList();
                user = db.Users.FirstOrDefault(p => p.Login == modelUser.Login);

                if (user == null)
                {
                    ModelState.AddModelError("", "Пользователь не найден");
                    return View(modelUser);
                }

                if (string.IsNullOrEmpty(modelUser.Password))
                {
                    ModelState.AddModelError("", "Не указан пароль");
                    return View(modelUser);
                }
                if (HashingClass.HashingSHA256(modelUser.Password, user.Salt) != user.Password)
                {
                    ModelState.AddModelError("", "Неверный пароль");
                    return View(modelUser);
                }
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            // Создаем объект ClaimsIdentity на основе утверждений
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Создаем объект AuthenticationProperties для настройки куки
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            // Устанавливаем куки с помощью метода SignInAsync()
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // выход
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
