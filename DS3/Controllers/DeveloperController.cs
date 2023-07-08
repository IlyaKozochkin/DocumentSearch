using DS3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DS3.Controllers
{
    [Authorize(Roles = "Developer")]
    public class DeveloperController : Controller
    {
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
                users = await db.Users.Include(p => p.Role).ToListAsync();
            }
            return View(users);
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

                var availableRoles = await db.Roles.ToListAsync();

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
        async public Task<IActionResult> EditRoleUser(EditRoleViewModel model)
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

        // создать пользователя 
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

        // выдать новый пароль пользователю
        [HttpGet]
        async public Task<IActionResult> NewPasswordForUser()
        {
            List<User> users;
            using (UserDbContext db = new UserDbContext())
            {
                users = await db.Users.Where(p => p.Role.Id != 4).ToListAsync();
            }

            return View(users);
        }
        [HttpPost]
        async public Task<IActionResult> NewPasswordForUser(int userId, string newPassword, string confNewPassword)
        {
            if (userId == 0)
            {
                List<User> users;
                using (UserDbContext db = new UserDbContext())
                {
                    users = await db.Users.Where(p => p.Role.Id != 4).ToListAsync();
                }

                ModelState.AddModelError("", "Необходимо выбрать пользователя");
                return View(users);
            }

            if (newPassword != confNewPassword || newPassword == null)
            {
                List<User> users;
                using (UserDbContext db = new UserDbContext())
                {
                    users = await db.Users.Where(p => p.Role.Id != 4).ToListAsync();
                }

                ModelState.AddModelError("", "Пароли не совпадают");
                return View(users);
            }
            else
            {
                using (UserDbContext db = new UserDbContext())
                {
                    var user = await db.Users.Include(p => p.Role).FirstOrDefaultAsync(p => p.Id == userId);
                    string salt = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).Remove(15);

                    user!.Salt = salt;
                    user.Password = HashingClass.HashingSHA256(newPassword, salt);

                    await db.SaveChangesAsync();
                }

                return RedirectToAction("Users");
            }
        }

        ////////////////////////////////////////////////////////////////////////



        // роли 
        async public Task<IActionResult> Roles()
        {
            List<Role> roles;
            using (UserDbContext db = new UserDbContext())
            {
                roles = await db.Roles.ToListAsync();
            }
            return View(roles);
        }
        // создать роль 
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        async public Task<IActionResult> CreateRole(Role role)
        {
            using (UserDbContext db = new UserDbContext())
            {
                db.Roles.Add(role);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Roles");
        }



        ////////////////////////////////////////////////////////////////////////



        //законы
        async public Task<IActionResult> Documents()
        {
            List<Document> documents;
            List<TypeDoc> typeDocs;
            using (DocumentsDbContext db = new DocumentsDbContext())
            {
                typeDocs = await db.TypeDocs.ToListAsync();
                documents = await db.Documents.ToListAsync();
            }
            return View(documents);
        }

        // добавить закон
        [HttpGet]
        async public Task<IActionResult> AddDocument()
        {
            using (DocumentsDbContext db = new DocumentsDbContext())
            {
                var typeDocs = await db.TypeDocs.ToListAsync();
                ViewBag.TypeDocs = new SelectList(typeDocs, "Id", "Name");
            }
            return View();
        }

        [HttpPost]
        async public Task<IActionResult> AddDocument(IFormFile file)
        {
            // ключевые слова
            string formKeywords = Request.Form["keywords"];
            string[] keywords = formKeywords.Split(';');

            // Сохраняем файл на сервере в указанной директории
            string name = Request.Form["fileName"];
            string fileName = name.Length > 200 ? name.Substring(0, 200).TrimEnd(' ') + file.FileName.Split('.').Last() : name + "." + file.FileName.Split('.').Last();
            var filePath = Path.Combine(Directory.GetCurrentDirectory() + @"/Documents", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // сохраняем данные в БД
            using (DocumentsDbContext db = new DocumentsDbContext())
            {
                Document document = new Document()
                {
                    Name = name,
                    FileName = fileName,
                    TypeDoc = db.TypeDocs.First(p => p.Id == int.Parse(Request.Form["typeDocId"]))
                };

                db.Documents.Add(document);

                KeyWord keyWordFileName = new KeyWord() { Word = name, Document = document };
                db.KeyWords.Add(keyWordFileName);

                foreach (var item in keywords)
                {
                    KeyWord keyWord = new KeyWord() { Word = item.TrimStart(' ').TrimEnd(' '), Document = document };
                    db.KeyWords.Add(keyWord);
                }

                await db.SaveChangesAsync();
            }
            return RedirectToAction("Documents");
        }

        [HttpGet]
        async public Task<IActionResult> EditDocument(int id)
        {
            using (DocumentsDbContext db = new DocumentsDbContext())
            {
                var typeDocs = await db.TypeDocs.ToListAsync();
                ViewBag.TypeDocs = new SelectList(typeDocs, "Id", "Name");

                // Получить документ из базы данных
                var document = await db.Documents.Include(p => p.TypeDoc).FirstOrDefaultAsync(p => p.Id == id);

                List<KeyWord>? keywords = await db.KeyWords.Where(p => p.Document == document).ToListAsync();

                string listkeyword = "";

                for (int i = 1; i < keywords.Count; i++)
                {
                    listkeyword += keywords[i].Word + "; ";
                }
                //foreach (var item in keywords)
                //{
                //    listkeyword += item.Word + "; ";
                //}
                listkeyword = listkeyword.TrimEnd(' ').TrimEnd(';');

                // Создать модель представления для редактирования
                var viewModel = new EditDocumentViewModel
                {
                    Id = document.Id,
                    Name = document.Name,
                    TypeDocId = document.TypeDoc.Id,
                    KeyWords = listkeyword
                };

                return View(viewModel);
            }
        }
        [HttpPost]
        async public Task<IActionResult> EditDocument(EditDocumentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Обработать случай, если модель представления не прошла валидацию
                return View("EditDocuments", viewModel);
            }

            using (DocumentsDbContext db = new DocumentsDbContext())
            {
                // Получить документ из базы данных
                var document = await db.Documents.FirstOrDefaultAsync(p => p.Id == viewModel.Id);

                //string newfilename = viewModel.Name.Length > 200 ? viewModel.Name.Substring(0, 200).TrimEnd(' ') + ".pdf" : viewModel.Name + ".pdf";

                // Применить изменения
                document.Name = viewModel.Name;
                document.TypeDoc = db.TypeDocs.First(p => p.Id == int.Parse(Request.Form["typeDocId"]));
                //document.FileName = newfilename;

                var delkeywords = db.KeyWords.Where(p => p.Document.Id == viewModel.Id);
                db.RemoveRange(delkeywords);

                KeyWord keyWordFileName = new KeyWord() { Word = document.Name, Document = document };
                db.KeyWords.Add(keyWordFileName);

                string[] keywords = viewModel.KeyWords.Split(';');
                foreach (var item in keywords)
                {
                    KeyWord keyWord = new KeyWord() { Word = item.TrimStart(' ').TrimEnd(' '), Document = document };
                    db.KeyWords.Add(keyWord);
                }

                await db.SaveChangesAsync();

                // Перенаправить пользователя на страницу с документами или выполнить другую необходимую операцию
                return RedirectToAction("Documents");
            }
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
                    
                    if (System.IO.File.Exists($"{Directory.GetCurrentDirectory()}/Documents/{document.FileName}"))
                    {
                        System.IO.File.Delete($"{Directory.GetCurrentDirectory()}/Documents/{document.FileName}");
                    }
                }
                catch (Exception)
                {
                    return NotFound();
                }
            }

            return RedirectToAction("Documents");
        }



        ////////////////////////////////////////////////////////////////////////



        // типы законов
        async public Task<IActionResult> Types()
        {
            List<TypeDoc> typeDocs;
            using (DocumentsDbContext db = new DocumentsDbContext())
            {
                typeDocs = await db.TypeDocs.ToListAsync();
            }
            return View(typeDocs);
        }

        // добавить тип
        [HttpGet]
        public IActionResult AddType()
        {
            return View();
        }
        [HttpPost]
        async public Task<IActionResult> AddType(TypeDoc type)
        {
            using (DocumentsDbContext db = new DocumentsDbContext())
            {
                await db.TypeDocs.AddAsync(type);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Types");
        }
    }
}
