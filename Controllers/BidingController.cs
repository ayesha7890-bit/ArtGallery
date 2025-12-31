using ArtGallery.Data;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArtGallery.Controllers
{
    public class BidingController : Controller
    {
      
            private readonly ApplicationDbContext _context;

            public BidingController(ApplicationDbContext context)
            {
                _context = context;
            }

            // --- Show all bids for an artwork ---
            [HttpGet]
            [Route("Artwork/Bids/{artworkId}")]
            public IActionResult Index(int artworkId)
            {
                var bids = _context.Biding
                    .Where(b => b.ArtworkId == artworkId)
                    
                    .Include(b => b.User)
                    .OrderByDescending(b => b.BidAmount)
                    .ToList();

                return View(bids);
            }

            // --- Place a new bid ---
            [HttpPost]
            [Route("Artwork/PlaceBid")]
            public async Task<JsonResult> PlaceBid(int artworkId, decimal bidAmount)
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return Json(new { success = false, message = "User not logged in." });

                var artwork = await _context.Artist.FindAsync(artworkId);
                if (artwork == null) return Json(new { success = false, message = "Artwork not found." });

                var highestBid = _context.Biding
                    .Where(b => b.ArtworkId == artworkId)
                    .OrderByDescending(b => b.BidAmount)
                    .FirstOrDefault();

                if (highestBid != null && bidAmount <= highestBid.BidAmount)
                    return Json(new { success = false, message = "Bid must be higher than current highest bid." });

                var bid = new Biding
                {
                    ArtworkId = artworkId,
                    UserId = userId.Value,
                    BidAmount = bidAmount,
                    BidTime = DateTime.Now
                };

                _context.Biding.Add(bid);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bid placed successfully!" });
            }
        }
    }

 