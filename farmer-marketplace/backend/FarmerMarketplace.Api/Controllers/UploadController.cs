// backend/FarmerMarketplace.Api/Controllers/UploadController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController: ControllerBase
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        // POST /api/upload/product-image
        // Returns base64 + contentType — frontend sends these along with the rest
        // of the product form in the same POST/PUT /products request.
        [HttpPost("product-image")]
        [Authorize(Roles = "Farmer,FpoAdmin")]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> UploadProductImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was uploaded.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException("Only .jpg, .jpeg, .png, or .webp files are allowed.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            return Ok(new
            {
                imageBase64 = Convert.ToBase64String(ms.ToArray()),
                contentType = file.ContentType
            });
        }
    }
}