using SitemapGenerator.Infrastructure;
using SitemapGenerator.Models;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace SitemapGenerator.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Homepage()
        {
            return View();
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Generate(GenerateContext model)
		{
			if (ModelState.IsValid)
			{
				Robot robot = null;
				try
				{
					var url = new Uri(model.Path);
					robot = new Robot(url);
					robot.Work();
					ViewBag.Clear = true;
				}
				catch (Exception ex)
				{
					ViewBag.TestError = ex.ToString();
					ViewBag.Clear = false;
				}

				return View("ResultView", (object)robot.ResultFileName);
			}

			return View("Homepage", model);
		}

		public FileResult Download(string fileName)
		{
			var path = SiteMapModel.FullPathByName(fileName);
			
			if (!System.IO.File.Exists(path))
			{
				throw new HttpException(404, "Not found");
			}

			return File(path, "application/octet-stream", fileName += ".xml");
		}
    }
}