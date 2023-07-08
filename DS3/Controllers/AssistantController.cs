using DS3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DS3.Controllers
{
    [Authorize(Roles = "Assistant, Developer")]
    public class AssistantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

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
            string fileName = Request.Form["fileName"] + "." + file.FileName.Split('.').Last();
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
                    Name = Request.Form["fileName"],
                    FileName = fileName,
                    TypeDoc = db.TypeDocs.First(p => p.Id == int.Parse(Request.Form["typeDocId"]))
                };

                db.Documents.Add(document);

                foreach (var item in keywords)
                {
                    KeyWord keyWord = new KeyWord() { Word = item.TrimStart(' ').TrimEnd(' '), Document = document };
                    db.KeyWords.Add(keyWord);
                }

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
    }
}
