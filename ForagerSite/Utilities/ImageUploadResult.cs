using Microsoft.AspNetCore.Components.Forms;

namespace ForagerSite.Utilities
{
    public class ImageUploadResult
    {
        public List<string> Previews { get; set; } = new();
        public List<IBrowserFile> Files { get; set; } = new();
    }
}
