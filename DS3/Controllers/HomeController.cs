using DS3.Models;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace DS3.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            List<TypeDoc>? types;
            using (DocumentsDbContext db = new DocumentsDbContext())
            {
                types = db.TypeDocs.ToList();
            }
            return View(types);
        }
        [HttpPost]
        public IActionResult Laws(List<int> ids)
        {
            if (ids.Contains(0))
            {
                ids.Clear();
                using (DocumentsDbContext db = new DocumentsDbContext())
                {
                    ids = db.TypeDocs.Select(td => td.Id).ToList();
                }
            }

            string request = Request.Form["request"];

            IndexingKeyWords search = new IndexingKeyWords();

            return View(search.SearchText(request, ids));
        }

        public FileResult DownloadFile(string fileName)
        {
            // Build the File Path.
            string path = Path.Combine(Directory.GetCurrentDirectory() + "/Documents", fileName);

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }

        public IActionResult PDFopen(string filePath)
        {
            filePath = Path.Combine(Directory.GetCurrentDirectory() + "/Documents", filePath);

            PdfReader reader = new PdfReader(filePath);
            MemoryStream stream = new MemoryStream();
            PdfStamper stamper = new PdfStamper(reader, stream);
            int pageCount = reader.NumberOfPages;
            for (int i = 1; i <= pageCount; i++)
            {
                PdfContentByte pageContent = stamper.GetOverContent(i);
                iTextSharp.text.Rectangle pageRectangle = reader.GetPageSizeWithRotation(i);
                pageContent.AddTemplate(stamper.GetImportedPage(reader, i), 0, 0);
            }
            stamper.Close();
            byte[] pdfData = stream.ToArray();
            return new FileContentResult(pdfData, "application/pdf");
        }

        public IActionResult NoAccess()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}