using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Drawing;
using UploadFile.Models;

namespace UploadFile.Controllers
{
    public class FileUploadController : Controller
    {
        private ImageModel _image;

        public FileUploadController(ImageModel image)
        {
            _image=image;
        }
                
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("FileUpload")]
        public async Task<ActionResult> Index(IFormFile file, ImageModel image)
        {
            TempData["path"] = await SaveFile(file);            
            image =await UploadFile(file);
            TempData["msg"] = "File Uploaded successfully.";
            return View(image);
        }
        // Upload file on server
        public async Task<ImageModel> UploadFile(IFormFile file)
        {
            var sortedDict = new List<string>();
            
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                using (var img = Image.FromStream(memoryStream))
                {
                    var bitmap = new Bitmap(img);
                    _image.pixelCount = bitmap.Height * bitmap.Width;
                    _image.dic = new Dictionary<string, double>();
                    _image.DominantColors = new Dictionary<string, int>();
                    _image.image64 = Convert.ToBase64String(memoryStream.ToArray());
                    for (int i = 0; i < bitmap.Width; i++)
                    {
                        for (int j = 0; j < bitmap.Height; j++)
                        {
                            Color pixel = bitmap.GetPixel(i, j);
                            
                            var hex = string.Format("{0:X2}{1:X2}{2:X2}", pixel.R, pixel.G, pixel.B);
                            if (_image.dic.ContainsKey(hex))
                            {
                                _image.dic[hex] += 1;
                            }
                            else
                            {
                                _image.dic.Add(hex, 1);
                            }  
                        }
                    }
                    sortedDict = _image.dic.OrderByDescending(kp => kp.Value)
                                      .Select(kp => kp.Key)
                                      .Take(5)                                       
                                      .ToList();
                }
            }

            Dictionary<string, int> res = new Dictionary<string, int>() { };
            
            foreach (var x in sortedDict)
            {
                var calc= _image.dic[x]/_image.pixelCount*100;
                int r = Convert.ToInt32(x.Substring(0, 2), 16);
                int g = Convert.ToInt32(x.Substring(2, 2), 16);
                int b = Convert.ToInt32(x.Substring(4, 2), 16);
                _image.DominantColors["rgb("+r+","+g+","+b+")"] = (int)Math.Round(calc,0);

            }
           
            return _image;
        }

        public async Task<String> SaveFile(IFormFile file)
        {
            
            string filename = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Upload"));
            var fullPath = Path.Combine(path, filename);
            using (var filestream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(filestream);
            }
        
            return "~/Upload/"+filename; 
        }
    }
}
