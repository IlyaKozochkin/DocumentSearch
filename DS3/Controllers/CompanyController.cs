using DS3.Models;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;

namespace DS3.Controllers
{
    public class CompanyController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Documents()
        {
            string request = Request.Form["request"];

            IndexingKeyWords search = new IndexingKeyWords();

            return View(search.SearchTextCD(request));
        }

        public FileResult DownloadFile(string fileName)
        {
            // Build the File Path.
            string path = Path.Combine(Directory.GetCurrentDirectory() + "/CompanyDocuments", fileName);

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }
    }
}
