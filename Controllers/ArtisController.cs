using ArtGallery.Data;
using ArtGallery.Models;
using ArtGallery.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArtGallery.Controllers
{
    public class ArtisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArtisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- DASHBOARD ---
        [Route("Artist/Dashboard")]
        public IActionResult ArtisDashboard()
        {
            // Security Check
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
                return RedirectToAction("Login", "User");

            return View();
        }

        // --- ARTWORK LIST (Sirf apni dikhayi degi) ---
        [Route("Artistwork")]
        public IActionResult Artistwork()
        {
            // 1. Login artist ki ID lein
            int? loggedInUserId = HttpContext.Session.GetInt32("UserId");

            if (loggedInUserId == null) return RedirectToAction("Login", "User");
            ViewBag.CategoryList = _context.Categories.ToList();

            // 2. 🚩 Filter: Sirf is artist ki artworks mangwao
            var artworks = _context.Artist
                .Where(a => a.UserId == loggedInUserId) // Sirf apna data dikhega
                .Select(a => new ArtistView
                {
                    ArtworkId = a.ArtworkId,
                    Title = a.Title,
                    Artists = a.Artists,
                    Category = a.Category,
                    Type = a.Type,
                    Price = a.Price,
                    Status = a.Status,
                    ImageUrl = a.ImageUrl,
                    StartingBid = a.StartingBid // Auction ke liye zaroori hai
                })
                .ToList();

            return View(artworks);
        }

        // --- UPLOAD ARTWORK ---
        [HttpGet]
        [Route("ArtisUpload")]
        public async Task<IActionResult> ArtisUpload()
        {
            ViewBag.CategoryList = await _context.Categories.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ArtisUpload")]
        public async Task<IActionResult> ArtisUpload(ArtistView artwork, IFormFile ImageFile)
        {
            int? loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return RedirectToAction("Login", "User");

            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Please upload an artwork image.");
                ViewBag.CategoryList = await _context.Categories.ToListAsync();
                return View(artwork);
            }

            // Image Upload Logic
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            // Mapping to Model
            Artist art = new Artist()
            {
                Artists = artwork.Artists,
                Category = artwork.Category,
                ImageUrl = "/uploads/" + fileName,
                Price = artwork.Price,
                Status = "Pending",
                Title = artwork.Title,
                Type = artwork.Type,
                UserId = loggedInUserId.Value,
                StartingBid = artwork.Type == "Auction" ? artwork.StartingBid : null,
                AuctionStartTime = artwork.Type == "Auction" ? artwork.AuctionStartTime : null,
                AuctionEndTime = artwork.Type == "Auction" ? artwork.AuctionEndTime : null

            };

            _context.Artist.Add(art);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Artwork uploaded successfully!";
            return RedirectToAction("ArtisDashboard");
        }

        // --- LOGOUT ---
        [Route("ArtistLogout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // 👈 Sab khatam
            return RedirectToAction("Login", "User");
        }


        //[Route("ArtistExhibition")]
        //public IActionResult ArtistExhibition()
        //{
        //    return View();
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]

        //[Route("Artis/RequestExhibition/{id}")]
        //public JsonResult RequestExhibition(int id)
        //{
        //    var art = _context.Artist.FirstOrDefault(a => a.ArtworkId == id);

        //    if (art != null && art.Status.ToLower() != "requested") // Agar already requested nahi
        //    {
        //        art.Status = "Requested"; // Status update
        //        _context.SaveChanges();
        //        return Json(new { success = true });
        //    }

        //    return Json(new { success = false, message = "Artwork not found or already requested." });
        //}
        // --- EDIT ARTWORK ---
        [HttpPost]
        [Route("Artis/EditArtist")] // <--- YEH LINE ZAROORI HAI
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArtist(ArtistView artwork, IFormFile? ImageFile)
        {
            var existingArt = await _context.Artist.FindAsync(artwork.ArtworkId);
            if (existingArt == null) return NotFound();

            existingArt.Title = artwork.Title;
            existingArt.Price = artwork.Price;
            existingArt.Category = artwork.Category;
            existingArt.Artists = artwork.Artists;

            if (existingArt.Type == "Auction")
            {
                existingArt.StartingBid = artwork.StartingBid;
            }

            // Image update logic agar naye image select ki ho
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                existingArt.ImageUrl = "/uploads/" + fileName;
            }

            _context.Update(existingArt);
            await _context.SaveChangesAsync();
            return RedirectToAction("Artistwork");
        }

        // --- DELETE ARTWORK ---
        [HttpPost]
        public async Task<JsonResult> DeleteArt(int id)
        {
            var art = await _context.Artist.FindAsync(id);
            if (art == null) return Json(new { success = false });

            _context.Artist.Remove(art);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

    }
}