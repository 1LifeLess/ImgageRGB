using Microsoft.AspNetCore.Mvc;

namespace UploadFile.Models
{
    [BindProperties]
    public class ImageModel
    {
        public Dictionary<string, int>? DominantColors { get; set; }
        public Dictionary<string, double> dic { get; set; }
        public int pixelCount { get; set; }

        public string image64 { get; set; }
    }
}