using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GradProject.Web.Models;

namespace GradProject.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // =========================================================================
        // هام جداً: حط رقم تلفونك هون (مع رمز الدولة) عشان تجيك الرسالة عليه
        // مثال: 96170123456
        private const string StoreOwnerPhone = "96171763234";
        // =========================================================================

        // GET: Orders (للأدمن فقط يشوف كل الطلبات)
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.Orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        // GET: Orders/Create
        // هذه الصفحة بتفتح لما الزبون يقرر يشتري
        public ActionResult Create(string productNames = "", decimal total = 0)
        {
            // بنعبي بيانات وهمية عشان نريح الزبون
            var order = new Order
            {
                OrderDetails = productNames,
                TotalAmount = total,
                PaymentMethod = "Cash On Delivery (COD)" // مثبتة
            };
            return View(order);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CustomerName,CustomerPhone,CustomerAddress,OrderDetails,TotalAmount")] Order order)
        {
            if (ModelState.IsValid)
            {
                // 1. تثبيت البيانات
                order.OrderDate = DateTime.UtcNow;
                order.Status = "Pending";
                order.PaymentMethod = "Cash On Delivery (COD)";

                // 2. حفظ الطلب بالداتابيز
                db.Orders.Add(order);
                db.SaveChanges();

                // 3. تجهيز رسالة الواتساب
                // نص الرسالة: مرحباً، طلبي رقم 5. التفاصيل: برجر. العنوان: بيروت...
                string message = $"Hello, New Order #{order.Id}\n" +
                                 $"Name: {order.CustomerName}\n" +
                                 $"Phone: {order.CustomerPhone}\n" +
                                 $"Address: {order.CustomerAddress}\n" +
                                 $"Items: {order.OrderDetails}\n" +
                                 $"Total: ${order.TotalAmount}\n" +
                                 $"Payment: COD";

                // تشفير الرسالة لتناسب الرابط
                string encodedMessage = Server.UrlEncode(message);

                // رابط الواتساب العالمي
                string whatsappUrl = $"https://wa.me/{StoreOwnerPhone}?text={encodedMessage}";

                // 4. توجيه المستخدم للواتساب فوراً
                return Redirect(whatsappUrl);
            }

            return View(order);
        }

        // GET: Orders/Details/5 (للأدمن)
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();
            return View(order);
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