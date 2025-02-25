using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Web;

namespace Matchy.Controllers
{
    public class SettingsController : Controller
    {
        // Helper method to get the image path and ensure directory exists
        private string GetImagePath()
        {
            string imagePath = Server.MapPath("~/Content/CustomImages");

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            return imagePath;
        }

        // Action method to handle image uploads
        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any(f => f != null))
            {
                return Json(new { success = false, message = "No files selected." });
            }

            string imagePath = GetImagePath();
            List<string> savedFiles = new List<string>();

            // Save each file to the server
            foreach (var file in files.Where(f => f != null))
            {
                string fileName = Path.GetFileName(file.FileName);
                string filePath = Path.Combine(imagePath, fileName);

                try
                {
                    file.SaveAs(filePath);
                    savedFiles.Add(fileName);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Error saving file: {ex.Message}" });
                }
            }

            return Json(new { success = true, files = savedFiles });
        }
        
        // Action method to get the list of custom images
        [HttpGet]
        public ActionResult GetCustomImages()
        {
            string imagePath = GetImagePath(); // Get the path to the CustomImages directory

            try
            {
                // Get the files in the directory
                var customImages = Directory.GetFiles(imagePath)
                    .Select(Path.GetFileName) // Get just the file name (without the full path)
                    .ToList();

                return Json(new { success = true, images = customImages }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error fetching images: {ex.Message}" });
            }
        }


        // Action method to handle resetting the image directory
        [HttpPost]
        public ActionResult Reset()
        {
            string imagePath = GetImagePath();

            try
            {
                var filesToDelete = Directory.GetFiles(imagePath);

                // Check if there are no files to delete
                if (filesToDelete.Length == 0)
                {
                    return Json(new { success = true, message = "No images to delete." });
                }

                // Delete each file in the directory
                foreach (var file in filesToDelete)
                {
                    System.IO.File.Delete(file);
                }

                return Json(new { success = true, message = "All images deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting files: {ex.Message}" });
            }
        }
    }
}
