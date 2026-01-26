using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GradProject.Web.Models;

namespace GradProject.Web.Controllers
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }

    public class CartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(GetCart());
        }

        // دالة الإضافة (معدلة لتدعم AJAX وتمنع الرفرش)
        public ActionResult AddToCart(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                var cart = GetCart();
                var existingItem = cart.FirstOrDefault(x => x.Product.Id == id);

                if (existingItem != null)
                    existingItem.Quantity++;
                else
                    cart.Add(new CartItem { Product = product, Quantity = 1 });

                Session["Cart"] = cart;
            }

            // سحر الـ AJAX: إذا الطلب جاي من الكود السري، رجع بس العدد الجديد
            if (Request.IsAjaxRequest())
            {
                int count = GetCart().Sum(x => x.Quantity);
                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index", "Products");
        }

        // دالة جديدة لزيادة العدد من داخل السلة حصراً
        public ActionResult IncreaseQty(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Product.Id == id);
            if (item != null)
            {
                item.Quantity++;
                Session["Cart"] = cart;
            }
            return RedirectToAction("Index"); // بترجعك لنفس الصفحة (السلة)
        }

        public ActionResult DecreaseQty(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Product.Id == id);
            if (item != null)
            {
                if (item.Quantity > 1)
                    item.Quantity--;
                else
                    cart.Remove(item);
                Session["Cart"] = cart;
            }
            return RedirectToAction("Index");
        }

        public ActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Product.Id == id);
            if (item != null)
            {
                cart.Remove(item);
                Session["Cart"] = cart;
            }
            return RedirectToAction("Index");
        }

        public ActionResult ClearCart()
        {
            Session["Cart"] = null;
            return RedirectToAction("Index");
        }

        public ActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index");

            string ownerPhone = "96170000000"; // عدل رقمك
            string message = "Hello, New Order Request:\n\n";
            decimal grandTotal = 0;

            foreach (var item in cart)
            {
                decimal subTotal = item.Product.Price * item.Quantity;
                message += $"- {item.Product.Name} (x{item.Quantity}) : ${subTotal}\n";
                grandTotal += subTotal;
            }

            message += $"\nTotal Amount: ${grandTotal}\nPayment: Cash on Delivery";
            Session["Cart"] = null;

            string url = $"https://wa.me/{ownerPhone}?text={Server.UrlEncode(message)}";
            return Redirect(url);
        }

        private List<CartItem> GetCart()
        {
            if (Session["Cart"] == null)
                Session["Cart"] = new List<CartItem>();
            return (List<CartItem>)Session["Cart"];
        }
    }
}