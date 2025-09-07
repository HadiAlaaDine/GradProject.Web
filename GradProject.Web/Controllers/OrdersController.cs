using GradProject.Web.Models;
using GradProject.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Rotativa;
using Rotativa.Options;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace GradProject.Web.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Orders
        // Filters: from,to,minTotal,maxTotal | mineOnly (Admins only) | status,payment
        // Paging: page,pageSize
        [Authorize]
        public ActionResult Index(
            string from, string to,
            decimal? minTotal, decimal? maxTotal,
            bool? mineOnly,
            string status, string payment,
            int page = 1, int pageSize = 10)
        {
            var q = db.Orders
                      .Include(o => o.Items)
                      .AsQueryable();

            var userId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");

            // غير الأدمن يرى طلباته فقط
            if (!isAdmin)
            {
                q = q.Where(o => o.UserId == userId);
                mineOnly = true; // للعرض في الفورم فقط
            }
            else if (mineOnly == true)
            {
                q = q.Where(o => o.UserId == userId);
            }

            // تواريخ
            DateTime dt;
            if (!string.IsNullOrWhiteSpace(from) && DateTime.TryParse(from, out dt))
                q = q.Where(o => o.CreatedAt >= dt.Date);

            if (!string.IsNullOrWhiteSpace(to) && DateTime.TryParse(to, out dt))
                q = q.Where(o => o.CreatedAt < dt.Date.AddDays(1)); // حد علوي حصري

            // مبالغ
            if (minTotal.HasValue) q = q.Where(o => o.Total >= minTotal.Value);
            if (maxTotal.HasValue) q = q.Where(o => o.Total <= maxTotal.Value);

            // فلتر الحالة (اختياري)
            GradProject.Web.Models.OrderStatus st;
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse(status, true, out st))
            {
                q = q.Where(o => o.Status == st);
            }

            // فلتر طريقة الدفع (اختياري)
            GradProject.Web.Models.PaymentMethod pm;
            if (!string.IsNullOrWhiteSpace(payment) &&
                Enum.TryParse(payment, true, out pm))
            {
                q = q.Where(o => o.PaymentMethod == pm);
            }

            // ترتيب
            q = q.OrderByDescending(o => o.CreatedAt);

            // Paging
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = q.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var items = q.Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();

            // ViewBags
            ViewBag.from = from;
            ViewBag.to = to;
            ViewBag.minTotal = minTotal;
            ViewBag.maxTotal = maxTotal;
            ViewBag.mineOnly = mineOnly ?? false;

            ViewBag.status = status;
            ViewBag.payment = payment;

            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.totalCount = totalCount;
            ViewBag.totalPages = totalPages;

            return View(items);
        }



        // GET: /Orders/Details/5
        [Authorize]
        [HttpGet]
        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();
            var isAdmin = User.IsInRole("Admin");

            var order = db.Orders
                          .Include(o => o.Items.Select(i => i.Product))
                          .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return HttpNotFound();

            // السماح للإدمن يشوف الكل، والمستخدم العادي بس طلباته
            if (!isAdmin && order.UserId != userId)
                return new HttpUnauthorizedResult();

            return View(order);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Dashboard()
        {
            var orders = db.Orders
                           .Include(o => o.Items.Select(i => i.Product))
                           .ToList();

            var model = new OrdersDashboardViewModel
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.Total),
                RecentOrders = orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new OrderRow
                    {
                        Id = o.Id,
                        CreatedAt = o.CreatedAt,
                        Total = o.Total
                    }).ToList(),

                // نحمي حالنا لو في Items قديمة ما فيها Product
                TopProducts = orders
                    .SelectMany(o => o.Items)
                    .GroupBy(i => i.Product != null ? i.Product.Name : "(Unknown)")
                    .Select(g => new TopProductRow
                    {
                        ProductName = g.Key,
                        Quantity = g.Sum(i => i.Quantity)
                    })
                    .OrderByDescending(x => x.Quantity)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();

            // حاول تحوّل النصّ إلى enum بشكل آمن
            OrderStatus newStatus;
            if (!Enum.TryParse<OrderStatus>(status, true, out newStatus))
            {
                TempData["Error"] = "Invalid status value.";
                return RedirectToAction("Details", new { id });
            }

            order.Status = newStatus;
            db.SaveChanges();

            TempData["Success"] = $"Order status updated to {order.Status}.";
            return RedirectToAction("Details", new { id });
        }

        [Authorize]
        public ActionResult Invoice(int id)
        {
            var userId = User.Identity.GetUserId();
            var isAdmin = User.IsInRole("Admin");

            var order = db.Orders
                          .Include(o => o.Items.Select(i => i.Product))
                          .FirstOrDefault(o => o.Id == id);

            if (order == null) return HttpNotFound();
            if (!isAdmin && order.UserId != userId) return new HttpUnauthorizedResult();

            // ✅ توليد PDF مع إعدادات أوضح
            return new Rotativa.ViewAsPdf("Invoice", order)
            {
                FileName = $"Invoice_Order_{order.Id}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageMargins = new Rotativa.Options.Margins(10, 10, 15, 10), // left, right, top, bottom
                IsGrayScale = false
            };
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}