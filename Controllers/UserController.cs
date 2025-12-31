using ArtGallery.Data;
using ArtGallery.Migrations;
using ArtGallery.Models;

using BCrypt.Net; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        
        {
            _context = context;
        }

        [Route("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already in use.");
                    return View(model);
                }

                // Password Hashing
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

                _context.Users.Add(model);
                await _context.SaveChangesAsync();

                // Session set karein
                HttpContext.Session.SetInt32("TempUserId", model.UserId);
                HttpContext.Session.SetString("TempUserRole", model.Role);

                // 🚩 REDIRECTION LOGIC 🚩
                if (model.Role == "Artist")
                {
                    // Agar Artist hai toh Profile ki zaroorat nahi, direct dashboard
                    // Pehle main session bhi set kar dete hain
                    HttpContext.Session.SetInt32("UserId", model.UserId);
                    HttpContext.Session.SetString("UserRole", "Artist");
                    HttpContext.Session.SetString("UserName", model.FullName);

                    return RedirectToAction("ArtisDashboard", "Artis");
                }

                // Sirf Normal User (Buyer) ko Profile page par bhejein
                return RedirectToAction("CreateProfile");
            }
            return View(model);
        }

        [Route("Login")]
        public IActionResult Login()
        {

            return View();
        }

      
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromForm] string Email, [FromForm] string Password)
        {
            // 1. Database se user nikalna (Email matching)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);

            // 2. User validation aur Password verification
            if (user != null && BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                // Session Values set karna
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserRole", user.Role);
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (user.Role == "Artist")
                // --- UPDATE END ---
                {
                    return RedirectToAction("ArtisDashboard", "Artis");
                }
               
                else
                {
                    var profile = await _context.Profile.FirstOrDefaultAsync(p => p.UserId == user.UserId);
                    if (profile == null)
                    {
                        HttpContext.Session.SetInt32("TempUserId", user.UserId);
                        return RedirectToAction("CreateProfile");
                    }

                    HttpContext.Session.SetString("UserInterest", profile.Interest);

                    // Sabse safe redirection
                    return RedirectToAction("User", "User");
                }
            }

            // 3. Agar login fail hua toh Error message dena
            ViewBag.Error = "Invalid Email or Password. Please try again.";
            return View();
        }


        //        [Route("/")]
        //[HttpGet]
        //        public async Task<IActionResult> User(string categoryName = "All")

        //        {
        //            int? userId = HttpContext.Session.GetInt32("UserId");

        //    // 1. Categories fetch karein tabs ke liye
        //    ViewBag.Categories = await _context.Categories.ToListAsync();

        //    // 2. ⭐ YAHAN FILTER LAGAYEIN ⭐
        //    // Sirf wahi artworks uthayein jin ka Status 'Approved' ho
        //    var artsQuery = _context.Artist
        //        .Include(a => a.Biding)
        //        .Where(a => a.Status == "Approved") // <--- Ye line Admin approval ko handle karegi
        //        .AsQueryable();

        //    // 3. Recommendation Logic (Jo aapne pehle likha tha)
        //    if (userId != null)
        //    {
        //        var profile = await _context.Profile.FirstOrDefaultAsync(p => p.UserId == userId);
        //        if (profile != null)
        //        {
        //            // User ke interest wali artworks pehle dikhayen
        //            artsQuery = artsQuery.OrderByDescending(a => a.Category == profile.Interest)
        //                                 .ThenByDescending(a => a.ArtworkId);
        //        }
        //    }

        //    // 4. Category Filter (Agar user tabs par click kare)
        //    if (!string.IsNullOrEmpty(categoryName) && categoryName != "All")
        //    {
        //        artsQuery = artsQuery.Where(a => a.Category == categoryName);
        //    }

        //    // 5. Final Result
        //    ViewBag.ArtWorks = await artsQuery.ToListAsync();

        //    // Exhibitions ka filter bhi approve status par lagayein
        //    var exhibitions = await _context.Exhibitions
        //        .Where(e => e.IsApproved == true)
        //        .Take(4)
        //        .ToListAsync();

        //    return View(exhibitions);
        //}
        [Route("/")]
        [HttpGet]
        public async Task<IActionResult> User(string categoryName = "All")
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            // 1. Categories fetch karein tabs ke liye
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.SelectedCategory = categoryName;

            // 2. Base Query (Approved Artworks only)
            var artsQuery = _context.Artist
                .Include(a => a.Biding)
                .ThenInclude(b => b.User) // Bidding ke sath user include karein taake winner nikal saken
                .Where(a => a.Status == "Approved")
                .AsQueryable();

            // 3. Recommendation Logic
            if (userId != null)
            {
                var profile = await _context.Profile.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile != null)
                {
                    artsQuery = artsQuery.OrderByDescending(a => a.Category == profile.Interest)
                                         .ThenByDescending(a => a.ArtworkId);
                }
            }

            // 4. Category Filter
            if (!string.IsNullOrEmpty(categoryName) && categoryName != "All")
            {
                artsQuery = artsQuery.Where(a => a.Category == categoryName);
            }

            // 5. ⭐ FINAL DATA SEPARATION (Ye tabdeeli zaroori hai) ⭐
            var allArtworks = await artsQuery.ToListAsync();

            // Auction artworks ko alag ViewBag mein dalein


            ViewBag.BiddingArtWorks = allArtworks.Where(a => a.Type == "Auction").ToList();


            // Normal (Buy Now) artworks ko alag ViewBag mein dalein
            ViewBag.ArtWorks = allArtworks.Where(a => a.Type != "Auction").ToList();

            // 6. Exhibitions
            var exhibitions = await _context.Exhibitions
                .Where(e => e.IsApproved == true)
                .Take(4)
                .ToListAsync();

            return View(exhibitions);
        }
        [HttpGet]
        public IActionResult CreateProfile()
        {
            // Check agar user register ho kar aaya hai
            if (HttpContext.Session.GetInt32("TempUserId") == null) return RedirectToAction("Register");
            ViewBag.AdminCategories =  _context.Categories.ToList();

            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateProfile(Profile profile)
        //{
        //    // 1. Session se Temp data nikalna
        //    int? userId = HttpContext.Session.GetInt32("TempUserId");
        //    string role = HttpContext.Session.GetString("TempUserRole");

        //    if (userId == null) return RedirectToAction("Register");

        //    // Profile link karna
        //    profile.UserId = userId.Value;

        //    if (ModelState.IsValid)
        //    {
        //        _context.Profile.Add(profile);
        //        await _context.SaveChangesAsync();

        //        // 2. User ka naam nikalna Database se (Navbar ke liye zaroori hai)
        //        var user = await _context.Users.FindAsync(userId.Value);

        //        // 🚩 Login Session set karna - AB NAVBAR SAHI DIKHEGA
        //        HttpContext.Session.SetInt32("UserId", userId.Value);
        //        HttpContext.Session.SetString("UserRole", role);
        //        if (user != null)
        //        {
        //            HttpContext.Session.SetString("UserName", user.FullName); // Isse "Hi, Name" dikhega
        //        }

        //        // Recommendation ke liye interest save karein
        //        HttpContext.Session.SetString("UserInterest", profile.Interest);

        //        // Temp session clear karna
        //        HttpContext.Session.Remove("TempUserId");
        //        HttpContext.Session.Remove("TempUserRole");

        //        // Role based redirection
        //        if (role == "Artist") return RedirectToAction("ArtisDashboard", "Artis");

        //        return RedirectToAction("User", "User");
        //    }

        //    return View(profile);
        //}
        [HttpPost]
        public async Task<IActionResult> CreateProfile(Profile profile)
        {
            int? userId = HttpContext.Session.GetInt32("TempUserId");
            string role = HttpContext.Session.GetString("TempUserRole");

            if (userId == null) return RedirectToAction("Register");

            profile.UserId = userId.Value;

            if (ModelState.IsValid)
            {
                _context.Profile.Add(profile);
                await _context.SaveChangesAsync();

                // 🚩 LOGIN SESSION SET KARNA (Bohat Zaroori)
                var user = await _context.Users.FindAsync(userId.Value);

                HttpContext.Session.SetInt32("UserId", userId.Value);
                HttpContext.Session.SetString("UserRole", role);

                if (user != null)
                {
                    // Navbar isi Name ko dekh kar "Logout" button dikhata hai
                    HttpContext.Session.SetString("UserName", user.FullName);
                }

                // Temp sessions delete karein
                HttpContext.Session.Remove("TempUserId");
                HttpContext.Session.Remove("TempUserRole");

                // Home page (User action) par bhejien
                return RedirectToAction("User", "User");
            }
            return View(profile);
        }
        [HttpPost]
        public async Task<IActionResult> PlaceBid(int ArtworkId, decimal BidAmount)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // 1. Agar login nahi hai to Login par bhejein
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            // 2. Artwork check karein aur price validate karein
            var artwork = await _context.Artist.FindAsync(ArtworkId);
            if (artwork == null) return NotFound();

            if (BidAmount <= artwork.StartingBid)
            {
                TempData["Error"] = "Bid starting price se zyada honi chahiye!";
                return RedirectToAction("User");
            }

            // 3. Bid save karein (Aapke 'Biding' model ke mutabiq)
            var newBid = new Biding
            {
                ArtworkId = ArtworkId,
                UserId = userId.Value,
                BidAmount = BidAmount,
                BidTime = DateTime.Now
            };

            _context.Biding.Add(newBid); // Context mein agar DbSet ka naam Biding hai
            await _context.SaveChangesAsync();

            TempData["Success"] = "Aapki bid lag chuki hai!";
            return RedirectToAction("User");
        }
        public async Task<IActionResult> CheckAuctionWinner(int artworkId)
        {
            var artwork = await _context.Artist
                .Include(a => a.Biding)
                .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(a => a.ArtworkId == artworkId);

            if (artwork != null && DateTime.Now >= artwork.AuctionEndTime)
            {
                var winningBid = artwork.Biding
                    .OrderByDescending(b => b.BidAmount)
                    .FirstOrDefault();

                if (winningBid != null)
                {
                    // Yahan aap status update kar sakte hain
                    artwork.Status = "Sold";
                    // winningBid.User.UserId aapka winner hai
                    ViewBag.WinnerName = winningBid.User.FullName;
                }
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> BuyNow(int artworkId)
        {
            // 1. Session se UserId nikalna
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            // 2. Artwork ki price nikalna
            var art = await _context.Artist.FindAsync(artworkId);
            if (art == null) return NotFound();

            // 3. Order create karna (As per Doc: AdminStatus "Pending" aur PaymentStatus "Unpaid")
            var order = new Order
            {
                UserId = userId.Value,
                ArtworkId = artworkId,
                TotalAmount = art.Price,
                OrderDate = DateTime.Now,
                AdminStatus = "Pending", // Admin ki approval ka wait
                PaymentStatus = "Unpaid",
                PaymentMode = "Pending",
                ShippingAddress = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 4. Message dikhana
            TempData["Success"] = "Order Request Sent! Admin ke approve karne ke baad aap payment kar sakenge.";

            return RedirectToAction("User");
        }
        [Route("MyOrders")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var myOrders = await _context.Orders
                .Include(o => o.Artwork)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(myOrders);
        }
        //[HttpGet]
        //[Route("Checkout/{id}")]
        //public async Task<IActionResult> Checkout(int id)
        //{
        //    var order = await _context.Orders
        //        .Include(o => o.Artwork)
        //        .FirstOrDefaultAsync(o => o.OrderId == id);

        //    if (order == null || order.AdminStatus != "Approved")
        //        return RedirectToAction("MyOrders");

        //    return View(order);
        //}
        // UserController.cs mein Checkout action ko aise update karein
        [HttpGet]
        // [Route("Checkout/{id}")]  <-- Is line ko hata dein ya comment kar dein
        public async Task<IActionResult> Checkout(int orderId) // Parameter ka naam 'orderId' rakhein
        {
            var order = await _context.Orders
                .Include(o => o.Artwork)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            // Document ke mutabiq sirf Approved orders hi checkout ho sakte hain
            if (order == null || order.AdminStatus != "Approved")
            {
                TempData["Error"] = "Order is either not found or not approved by admin yet.";
                return RedirectToAction("MyOrders");
            }

            return View(order);
        }
        [HttpPost]
        [Route("ProcessPayment")]
        public async Task<IActionResult> ProcessPayment(int OrderId, string ShippingAddress, string PaymentMode)
        {
            var order = await _context.Orders.Include(o => o.Artwork).FirstOrDefaultAsync(o => o.OrderId == OrderId);

            if (order != null)
            {
                // 1. Order status update karein
                order.PaymentStatus = "Paid";
                order.AdminStatus = "Delivering"; // Status change kar dein
                order.ShippingAddress = ShippingAddress;
                order.PaymentMode = PaymentMode;

                // 2. Artwork ko SOLD mark karein taake koi aur na kharid sake
                var artwork = await _context.Artist.FindAsync(order.ArtworkId);
                if (artwork != null)
                {
                    artwork.Status = "Sold"; // Database mein Status 'Sold' ho jaye ga
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Payment Successful! Your artwork will be delivered soon.";
            }

            return RedirectToAction("MyOrders");
        }
        //[HttpPost]
        //public IActionResult AddToWishlist(int artworkId)
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");

        //    // login check
        //    if (userId == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    var already = _context.Wishlist
        //        .FirstOrDefault(w => w.UserId == userId.ToString() && w.ArtworkId == artworkId);

        //    if (already == null)
        //    {
        //        Wishing w = new Wishing
        //        {
        //            UserId = userId.Value.ToString(), // 👈 string mein convert
        //            ArtworkId = artworkId,
        //            AddedOn = DateTime.Now
        //        };

        //        _context.Wishlist.Add(w);
        //        _context.SaveChanges();
        //    }

        //    return RedirectToAction("User"); // 👈 tumhara home page
        //




        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");

        }

        public IActionResult Contactus()
        {
            return View();
        }
    }
}