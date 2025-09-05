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
        private string CurrentUserId => User.Identity.GetUserId();

        // GET: /Cart
        public ActionResult Index()
        {
            var items = db.CartItems
                          .Include(c => c.Product)
                          .Where(c => c.UserId == CurrentUserId)
                          .OrderBy(c => c.Id)
                          .ToList();
            return View(items);
        }

        // شارة السلة في الـ Navbar (Partial فقط)
        [ChildActionOnly]
        public PartialViewResult CartBadge()
        {
            if (!Request.IsAuthenticated)
            {
                ViewBag.Count = 0;
                return PartialView("_CartBadge");
            }

            var qty = db.CartItems
                        .Where(c => c.UserId == CurrentUserId)
                        .Select(c => (int?)c.Quantity)
                        .DefaultIfEmpty(0)
                        .Sum() ?? 0;

            ViewBag.Count = qty;
            return PartialView("_CartBadge");
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productId)
        {
            var product = db.Products.Find(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Products");
            }

            var item = db.CartItems.FirstOrDefault(c => c.UserId == CurrentUserId && c.ProductId == productId);
            if (item == null)
            {
                db.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    UserId = CurrentUserId,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow
                });
                TempData["Success"] = $"Added \"{product.Name}\" to cart.";
            }
            else
            {
                if (item.Quantity >= 1000)
                    TempData["Info"] = "Maximum quantity is 1000.";
                else
                {
                    item.Quantity += 1;
                    TempData["Success"] = $"Increased \"{product.Name}\" quantity.";
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index", "Products");
        }

        // POST: /Cart/Increase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Increase(int id)
        {
            var item = db.CartItems.Include(c => c.Product)
                                   .FirstOrDefault(c => c.Id == id && c.UserId == CurrentUserId);
            if (item == null)
            {
                TempData["Error"] = "Cart item not found.";
                return RedirectToAction("Index");
            }

            if (item.Quantity >= 1000)
                TempData["Info"] = "Maximum quantity is 1000.";
            else
                item.Quantity += 1;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: /Cart/Decrease
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Decrease(int id)
        {
            var item = db.CartItems.Include(c => c.Product)
                                   .FirstOrDefault(c => c.Id == id && c.UserId == CurrentUserId);
            if (item == null)
            {
                TempData["Error"] = "Cart item not found.";
                return RedirectToAction("Index");
            }

            if (item.Quantity <= 1)
                TempData["Info"] = "Minimum quantity is 1.";
            else
                item.Quantity -= 1;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            var item = db.CartItems.FirstOrDefault(c => c.Id == id && c.UserId == CurrentUserId);
            if (item == null)
            {
                TempData["Error"] = "Cart item not found.";
                return RedirectToAction("Index");
            }

            db.CartItems.Remove(item);
            db.SaveChanges();
            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clear()
        {
            var myItems = db.CartItems.Where(c => c.UserId == CurrentUserId).ToList();
            if (myItems.Any())
            {
                db.CartItems.RemoveRange(myItems);
                db.SaveChanges();
                TempData["Success"] = "Cart cleared.";
            }
            else
            {
                TempData["Info"] = "Your cart is already empty.";
            }
            return RedirectToAction("Index");
        }

        // GET: /Cart/Checkout
        public ActionResult Checkout()
        {
            var items = db.CartItems
                          .Include(c => c.Product)
                          .Where(c => c.UserId == CurrentUserId)
                          .ToList();

            if (!items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            return View(items);
        }

        // POST: /Cart/ConfirmCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmCheckout()
        {
            var cartItems = db.CartItems
                              .Include(c => c.Product)
                              .Where(c => c.UserId == CurrentUserId)
                              .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var order = new Order
                    {
                        UserId = CurrentUserId,
                        CreatedAt = DateTime.UtcNow
                    };

                    decimal total = 0m;
                    foreach (var ci in cartItems)
                    {
                        var price = ci.Product?.Price ?? 0m; // snapshot
                        order.Items.Add(new OrderItem
                        {
                            ProductId = ci.ProductId,
                            Quantity = ci.Quantity,
                            UnitPrice = price
                        });
                        total += price * ci.Quantity;
                    }
                    order.Total = total;

                    db.Orders.Add(order);
                    db.CartItems.RemoveRange(cartItems);
                    db.SaveChanges();

                    tx.Commit();

                    TempData["Success"] = $"Order #{order.Id} created successfully.";
                    return RedirectToAction("Details", "Orders", new { id = order.Id });
                }
                catch
                {
                    tx.Rollback();
                    TempData["Error"] = "Could not complete checkout. Please try again.";
                    return RedirectToAction("Index");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}