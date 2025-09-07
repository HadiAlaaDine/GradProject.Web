using GradProject.Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

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

        // ضع هذا الـViewModel داخل CartController (قبل الأفعال/الـactions أو في أعلى الملف)
        public class CheckoutViewModel
        {
            // لعرض عناصر السلة
            public List<CartItem> Items { get; set; } = new List<CartItem>();

            // بيانات الشحن (مطلوبة)
            [Required, StringLength(100)]
            public string ShipFullName { get; set; }

            [Required, StringLength(200)]
            public string ShipAddress1 { get; set; }

            [StringLength(200)]
            public string ShipAddress2 { get; set; }

            [Required, StringLength(100)]
            public string ShipCity { get; set; }

            [Required, StringLength(100)]
            public string ShipCountry { get; set; }

            [StringLength(30)]
            public string ShipPhone { get; set; }

            // طريقة الدفع من الفورم: "COD" أو "ONLINE"
            public string PaymentMethod { get; set; } = "COD";
        }

        // GET: /Cart/Checkout
        [Authorize]
        [HttpGet]
        public ActionResult Checkout()
        {
            var userId = User.Identity.GetUserId();
            var items = db.CartItems
                          .Include(c => c.Product)
                          .Where(c => c.UserId == userId)
                          .ToList();

            if (!items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            // نعبّي نموذج افتراضي
            var vm = new CheckoutViewModel
            {
                Items = items,
                ShipFullName = User.Identity.Name,   // اختياري
                ShipCountry = "Lebanon"              // اختياري
            };

            return View(vm);
        }

        // POST: /Cart/ConfirmCheckout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmCheckout(CheckoutViewModel vm)
        {
            var userId = User.Identity.GetUserId();

            // نعيد جلب العناصر لعرضها لو فيه خطأ فالتحقق
            vm.Items = db.CartItems
                         .Include(c => c.Product)
                         .Where(c => c.UserId == userId)
                         .ToList();

            if (!vm.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            // تحقّق الموديل (لأن حقول الشحن Required)
            if (!ModelState.IsValid)
            {
                // ارجِع لنفس صفحة الـCheckout مع رسائل التحقق
                return View("Checkout", vm);
            }

            // حوّل طريقة الدفع إلى enum آمن
            var method = (vm.PaymentMethod ?? "COD").Equals("ONLINE", StringComparison.OrdinalIgnoreCase)
                ? GradProject.Web.Models.PaymentMethod.Online
                : GradProject.Web.Models.PaymentMethod.CashOnDelivery;

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var order = new Order
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        PaymentMethod = method,

                        // 🟢 حقول الشحن الجديدة
                        ShipFullName = vm.ShipFullName?.Trim(),
                        ShipAddress1 = vm.ShipAddress1?.Trim(),
                        ShipAddress2 = vm.ShipAddress2?.Trim(),
                        ShipCity = vm.ShipCity?.Trim(),
                        ShipCountry = vm.ShipCountry?.Trim(),
                        ShipPhone = vm.ShipPhone?.Trim(),

                        Items = new List<OrderItem>()
                    };

                    decimal total = 0m;

                    foreach (var ci in vm.Items)
                    {
                        var unitPrice = ci.Product?.Price ?? 0m; // snapshot
                        order.Items.Add(new OrderItem
                        {
                            ProductId = ci.ProductId,
                            Quantity = ci.Quantity,
                            UnitPrice = unitPrice
                        });
                        total += unitPrice * ci.Quantity;
                    }

                    order.Total = total;

                    db.Orders.Add(order);
                    db.CartItems.RemoveRange(vm.Items); // تفريغ السلة
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