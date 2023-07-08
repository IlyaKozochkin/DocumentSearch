using DS3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DS3.Controllers
{
    [Authorize(Roles = "Admin, Developer")]
    public class AdminController : Controller
    {
        // меню
        public IActionResult Index()
        {
            return View();
        }

        // пользователи
        async public Task<IActionResult> Users()
        {
            List<User> users;
            using (UserDbContext db = new UserDbContext())
            {
                users = await db.Users.Include(p => p.Role).Where(p => p.Role.Name != "Developer").ToListAsync();
            }
            return View(users);
        }

        // добавление пользователя
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }
        [HttpPost]
        async public Task<IActionResult> CreateUser(RegisterViewModel modelUser)
        {
            if (ModelState.IsValid)
            {
                using (UserDbContext db = new UserDbContext())
                {
                    User? usercheck = await db.Users.FirstOrDefaultAsync(p => p.Login == modelUser.Login);
                    if (usercheck != null)
                    {
                        ModelState.AddModelError("", "Логин занят");
                        return View(modelUser);
                    }

                    Role roleUser = db.Roles.First(p => p.Name == "User");
                    string salt = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).Remove(15);
                    User user = new User()
                    {
                        FirstName = modelUser.FirstName,
                        LastName = modelUser.LastName,
                        Login = modelUser.Login,
                        Password = HashingClass.HashingSHA256(modelUser.Password, salt),
                        Role = roleUser,
                        Salt = salt
                    };

                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Users");
            }

            ModelState.AddModelError("", "Не все поля заполнены");
            return View(modelUser);
        }

        // изменение ролей
        [HttpGet]
        async public Task<IActionResult> EditRoleUser(int userId)
        {
            EditRoleViewModel model;
            using (UserDbContext db = new UserDbContext())
            {
                var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    // Обработка случая, когда пользователь не найден
                    return NotFound();
                }

                var availableRoles = await db.Roles.Where(p => p.Name != "Developer").ToListAsync();

                model = new EditRoleViewModel
                {
                    UserId = user.Id,
                    Login = user.Login,
                    CurrentRoleId = user.Role?.Id,
                    AvailableRoles = availableRoles
                };

            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoleUser(EditRoleViewModel model)
        {
            using (UserDbContext db = new UserDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
                if (user == null)
                {
                    // Обработка случая, когда пользователь не найден
                    return NotFound();
                }

                if (model.SelectedRoleId == 0)
                {
                    return View(model);
                }

                var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == model.SelectedRoleId);
                if (role == null)
                {
                    // Обработка случая, когда выбранная роль не найдена
                    return NotFound();
                }

                user.Role = role;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Users");
        }

        // удаление пользователя
        async public Task<IActionResult> DeleteUser(int userId)
        {
            using (UserDbContext db = new UserDbContext())
            {
                User? user = await db.Users.FirstOrDefaultAsync(p => p.Id == userId);
                if (user != null)
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                }
            }
            return RedirectToAction("Users");
        }

        ////////////////////////////////////////////////////////////////////////

        //законы
        async public Task<IActionResult> Documents()
        {
            List<CompanyDocument> documents;
            using (CompanyDbContext db = new CompanyDbContext())
            {
                documents = await db.Documents.ToListAsync();
            }
            return View(documents);
        }

        // добавить закон
        [HttpGet]
        public IActionResult AddDocument()
        {
            return View();
        }

        [HttpPost]
        async public Task<IActionResult> AddDocument(IFormFile file)
        {
            // Сохраняем файл на сервере в указанной директории
            string fileName = Request.Form["fileName"] + "." + file.FileName.Split('.').Last();
            var filePath = Path.Combine(Directory.GetCurrentDirectory() + @"/CompanyDocuments", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // сохраняем данные в БД
            using (CompanyDbContext db = new CompanyDbContext())
            {
                CompanyDocument document = new CompanyDocument()
                {
                    Name = Request.Form["fileName"],
                    FileName = fileName,
                };

                db.Documents.Add(document);

                await db.SaveChangesAsync();
            }
            return RedirectToAction("Documents");
        }

        async public Task<IActionResult> DeleteDocument(int id)
        {
            using (DocumentsDbContext db = new DocumentsDbContext())
            {
                try
                {
                    var document = await db.Documents.FirstOrDefaultAsync(p => p.Id == id);
                    db.Documents.Remove(document!);
                    await db.SaveChangesAsync();

                    if (System.IO.File.Exists($"{Directory.GetCurrentDirectory()}/CompanyDocuments/{document.FileName}"))
                    {
                        System.IO.File.Delete($"{Directory.GetCurrentDirectory()}/CompanyDocuments/{document.FileName}");
                    }
                }
                catch (Exception)
                {
                    return NotFound();
                }
            }

            return RedirectToAction("Documents");
        }
    }
}
