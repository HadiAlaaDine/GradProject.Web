using GradProject.Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace GradProject.Web.Controllers
{
    [Authorize] // السلة للمستخدمين المسجّلين فقط
    public class CartController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // (اختياري) Constructor صحيح بدون نوع إرجاع
        public CartController()
        {
        }

        // GET: /Cart
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var items = db.CartItems
                          .Include(c => c.Product)
                          .Where(c => c.UserId == userId)
                          .ToList();

            ViewBag.Total = items.Sum(i => i.Product.Price * i.Quantity);
            return View(items);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productId, int qty = 1)
        {
            var userId = User.Identity.GetUserId();
            var product = db.Products.Find(productId);
            if (product == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var existing = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
            if (existing == null)
            {
                db.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    UserId = userId,
                    Quantity = Math.Max(1, qty)
                });
            }
            else
            {
                existing.Quantity += Math.Max(1, qty);
            }

            db.SaveChanges();
            TempData["Success"] = "Added to cart.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int id, int qty)
        {
            var userId = User.Identity.GetUserId();
            var item = db.CartItems.Include(c => c.Product)
                                   .FirstOrDefault(c => c.Id == id && c.UserId == userId);
            if (item == null) return HttpNotFound();

            item.Quantity = Math.Max(1, qty);
            db.SaveChanges();
            TempData["Success"] = "Cart updated.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            var userId = User.Identity.GetUserId();
            var item = db.CartItems.FirstOrDefault(c => c.Id == id && c.UserId == userId);
            if (item == null) return HttpNotFound();

            db.CartItems.Remove(item);
            db.SaveChanges();
            TempData["Success"] = "Item removed.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clear()
        {
            var userId = User.Identity.GetUserId();
            var items = db.CartItems.Where(c => c.UserId == userId);
            db.CartItems.RemoveRange(items);
            db.SaveChanges();
            TempData["Success"] = "Cart cleared.";
            return RedirectToAction("Index");
        }

        // يظهر عدّاد العناصر في السلة داخل الـ Navbar
        [AllowAnonymous]
        [ChildActionOnly]
        public PartialViewResult CartBadge()
        {
            int count = 0;
            if (Request.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                // مجموع الكميات في السلة
                count = db.CartItems.Where(c => c.UserId == userId)
                                        .Select(c => (int?)c.Quantity)
                                        .DefaultIfEmpty(0)
                                        .Sum() ?? 0;
            }
            
            ViewBag.Count = count;
            return PartialView("_CartBadge");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }


    }
}