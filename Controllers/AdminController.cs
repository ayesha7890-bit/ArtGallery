using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Data;
using ArtGallery.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ArtGallery.Controllers
{
    [Route("Dashboard")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= Dashboard =======================
        public IActionResult Dashboard()

        {
            return View();
        }

        // ================= Categories Page ==================
        [Route("Categries")]

        public IActionResult Categries()



        {
            var categories = _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.SubCategories)
                .ToList();

            return View(categories);
        }

        // ================= Add Parent Category =================
        [HttpPost]
        [Route("AddCategory")]
        public IActionResult AddCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(model);
                _context.SaveChanges();
            }

            return RedirectToAction("Categries");
        }

        // ================= Add Sub Category ====================
        [HttpPost]
        [Route("AddSubCategory")]
        public IActionResult AddSubCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(model);
                _context.SaveChanges();
            }

            return RedirectToAction("Categries");
        }

        [HttpGet]
        [Route("GetCategory/{id}")]
        public IActionResult GetCategory(int id)
        {
            var cat = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (cat == null)
                return NotFound();

            return Json(new
            {
                id = cat.Id,
                name = cat.Name,
                description = cat.Description
            });
        }

        // ================= POST Edit Category (Update) ==================
        [HttpPost]
        [Route("EditCategoryPost")]
        public IActionResult EditCategoryPost(Category model)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(model);
                _context.SaveChanges();
            }

            return RedirectToAction("Categries");
        }

        // ================= Delete Category / Subcategory =================
        [HttpPost]

        [Route("DeleteCategory/{id}")]


        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                return NotFound();

            // Subcategories ko bhi delete karein
            if (category.SubCategories != null && category.SubCategories.Any())
            {
                _context.Categories.RemoveRange(category.SubCategories);
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction("Categries");
        }

        [Route("ExhibitionAdd")]
        public IActionResult ExhibitionAdd()
        {
            var exhibitions = _context.Exhibitions.ToList();
            return View("~/Views/Admin/Exhibitions/ExhibitionAdd.cshtml", exhibitions);
        }

        // ================= Create Exhibition POST =================
        [HttpPost]
        [Route("CreateExhibition")]
        public async Task<IActionResult> CreateExhibition(Exhibition model, IFormFile BannerImage)
        {
            // Date validation
            if (model.EndDate < model.StartDate)
            {
                TempData["Error"] = "End Date cannot be before Start Date!";
                return RedirectToAction("ExhibitionAdd");
            }

            if (ModelState.IsValid)
            {
                // ================= Banner Image Upload =================
                if (BannerImage != null && BannerImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BannerImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await BannerImage.CopyToAsync(stream);
                    }

                    model.BannerImage = "/uploads/" + fileName;
                }

                // ================= ADMIN AUTO APPROVAL =================
                model.IsApproved = true;
                model.ApprovalStatus = "Approved";

                _context.Exhibitions.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Exhibition created & approved successfully!";
                return RedirectToAction("ExhibitionAdd");
            }

            // ================= ModelState Invalid =================
            var exhibitions = _context.Exhibitions.ToList();
            return View("~/Views/Admin/Exhibitions/ExhibitionAdd.cshtml", exhibitions);
        }

        [HttpGet]
        [Route("GetExhibition/{id}")]
        public IActionResult GetExhibition(int id)
        {
            var ex = _context.Exhibitions.FirstOrDefault(e => e.ExhibitionId == id);
            if (ex == null) return NotFound();

            return Json(new
            {
                exhibitionId = ex.ExhibitionId,
                title = ex.Title,
                venue = ex.Venue,
                description = ex.Description,
                startDate = ex.StartDate.ToString("yyyy-MM-dd"),
                endDate = ex.EndDate.ToString("yyyy-MM-dd")
            });
        }
        [HttpPost]
        [Route("EditExhibitionPost")]
        public async Task<IActionResult> EditExhibitionPost(Exhibition model, IFormFile BannerImage)
        {
            var ex = _context.Exhibitions.FirstOrDefault(e => e.ExhibitionId == model.ExhibitionId);
            if (ex == null) return NotFound();

            ex.Title = model.Title;
            ex.Venue = model.Venue;
            ex.Description = model.Description;
            ex.StartDate = model.StartDate;
            ex.EndDate = model.EndDate;

            if (BannerImage != null && BannerImage.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(BannerImage.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await BannerImage.CopyToAsync(stream);

                ex.BannerImage = "/uploads/" + fileName;
            }

            _context.SaveChanges();
            TempData["Success"] = "Exhibition updated successfully!";
            return RedirectToAction("ExhibitionAdd");
        }

        // ================= Delete Exhibition =================
        [HttpPost]
        [Route("DeleteExhibition/{id}")]
        public IActionResult DeleteExhibition(int id)
        {
            var exhibition = _context.Exhibitions.FirstOrDefault(e => e.ExhibitionId == id);

            if (exhibition == null)
                return NotFound();

            // Agar banner image delete karni ho
            if (!string.IsNullOrEmpty(exhibition.BannerImage))
            {
                var imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    exhibition.BannerImage.TrimStart('/')
                );

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Exhibitions.Remove(exhibition);
            _context.SaveChanges();

            TempData["Success"] = "Exhibition deleted successfully!";
            return RedirectToAction("ExhibitionAdd");
        }


        [Route("Artwork")]
        public IActionResult Artwork()
        {
            var artworks = _context.Artist.OrderByDescending(a => a.ArtworkId).ToList();
            return View(artworks);
        }

        [HttpPost]
        [Route("ApproveArtwork/{id}")]
        public async Task<IActionResult> ApproveArtwork(int id)
        {
            var art = await _context.Artist.FindAsync(id);
            if (art != null)
            {
                art.Status = "Approved"; 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Artworks");
        }

        // 3. Reject karne ke liye
        [HttpPost]
        [Route("RejectArtwork/{id}")]
        public async Task<IActionResult> RejectArtwork(int id)
        {
            var art = await _context.Artist.FindAsync(id);
            if (art != null)
            {
                art.Status = "Rejected";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Artworks");
        }
        [HttpPost] // Delete hamesha Post hona chahiye safety ke liye
        [Route("DeleteArtwork/{id}")]
        public async Task<IActionResult> DeleteArtwork(int id)
        {
            var art = await _context.Artist.FindAsync(id);
            if (art != null)
            {
                _context.Artist.Remove(art);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Artwork");
        }
        //[HttpGet("ExhibitionRequests")]



        //public IActionResult ExhibitionRequests()
        //{
        //    // Yahan "Requested" ki bajaye "Pending" likhein kyunke model mein default wahi hai
        //    var pendingRequests = _context.Artist
        //                          .Include(a => a.User)
        //                          .Where(a => a.Status == "Pending")
        //                          .ToList();

        //    return View("~/Views/Admin/Exhibitions/ExhibitionRequests.cshtml", pendingRequests);
        //}
        //[HttpPost("ApproveRequest")]

        //public IActionResult ApproveRequest(int id)
        //{
        //    var art = _context.Artist.FirstOrDefault(a => a.ArtworkId == id);
        //    if (art != null)
        //    {
        //        art.Status = "Approved"; // ya "Exhibited"
        //        _context.SaveChanges();
        //    }
        //    return RedirectToAction("ExhibitionRequests");
        //}


        //[HttpPost("RejectRequest")]

        //public IActionResult RejectRequest(int id)
        //{
        //    var art = _context.Artist.FirstOrDefault(a => a.ArtworkId == id);
        //    if (art != null)
        //    {
        //        art.Status = "Rejected";
        //        _context.SaveChanges();
        //    }
        //    return RedirectToAction("ExhibitionRequests");
        //}
        // AdminController.cs


        [Route("ManageOrders")]
        public async Task<IActionResult> ManageOrders()
        {
            // Orders ke saath User aur Artist (Artwork) ka data include karna zaroori hai
            var orders = await _context.Orders
                .Include(o => o.User)

                                .Include(o => o.Artwork)

                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
        // Controller ke andar in methods ko update karein
        [HttpPost]
        [Route("ApproveOrder")]


        public async Task<IActionResult> ApproveOrder(int orderId) // Parameter ka naam wahi rakhein jo view mein hai

        {

            var order = await _context.Orders.FindAsync(orderId);

            if (order != null)
            {
                order.AdminStatus = "Approved";
                await _context.SaveChangesAsync();

            }
            return RedirectToAction("ManageOrders");
        }

        [HttpPost]
        [Route("RejectOrder")]

        public async Task<IActionResult> RejectOrder(int orderId)

        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.AdminStatus = "Rejected";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageOrders");
        }
    }
}
