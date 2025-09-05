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
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var items = db.CartItems.Include(c => c.Product)
                                    .Where(c => c.UserId == userId)
                                    .ToList();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productId, int quantity = 1)
        {
            var userId = User.Identity.GetUserId();

            var item = db.CartItems.SingleOrDefault(c => c.UserId == userId && c.ProductId == productId);
            if (item == null)
                db.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = quantity });
            else
                item.Quantity += quantity;

            db.SaveChanges();
            TempData["Success"] = "Added to cart.";
            return RedirectToAction("Index", "Products");
        }

        // GET: /Cart/Increase/5
        [Authorize]
        public ActionResult Increase(int id)
        {
            var userId = User.Identity.GetUserId();
            var item = db.CartItems.FirstOrDefault(c => c.Id == id && c.UserId == userId);
            if (item == null) return HttpNotFound();

            item.Quantity = Math.Min(item.Quantity + 1, 2000);
            db.SaveChanges();
            TempData["Success"] = "Quantity increased.";
            return RedirectToAction("Index");
        }

        // GET: /Cart/Decrease/5
        [Authorize]
        public ActionResult Decrease(int id)
        {
            var userId = User.Identity.GetUserId();
            var item = db.CartItems.FirstOrDefault(c => c.Id == id && c.UserId == userId);
            if (item == null) return HttpNotFound();

            if (item.Quantity > 1)
                item.Quantity -= 1;
            else
                db.CartItems.Remove(item);

            db.SaveChanges();
            TempData["Success"] = "Quantity updated.";
            return RedirectToAction("Index");
        }

        // GET: /Cart/Remove/5
        [Authorize]
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

        // GET: /Cart/Clear
        [Authorize]
        public ActionResult Clear()
        {
            var userId = User.Identity.GetUserId();
            var items = db.CartItems.Where(c => c.UserId == userId).ToList();
            if (items.Any())
            {
                db.CartItems.RemoveRange(items);
                db.SaveChanges();
                TempData["Success"] = "Cart cleared.";
            }
            return RedirectToAction("Index");
        }


        // CartBadge موجودة فوق


        // CartController
        [ChildActionOnly] // مهم: يُستدعى كـ Partial فقط
        public PartialViewResult CartBadge()
        {
            // لو مش عامل تسجيل دخول، رجّع 0 وما تعمل Redirect
            if (!Request.IsAuthenticated)
            {
                ViewBag.Count = 0;
                return PartialView("_CartBadge");
            }

            var userId = User.Identity.GetUserId();
            var count = db.CartItems
                          .Where(c => c.UserId == userId)
                          .Select(c => (int?)c.Quantity)
                          .DefaultIfEmpty(0)
                          .Sum() ?? 0;

            ViewBag.Count = count;
            return PartialView("_CartBadge");
        }




        protected override void Dispose(bool disposing)
        {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
        }
    }
}