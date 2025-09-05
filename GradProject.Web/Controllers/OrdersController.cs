using GradProject.Web.Models;
using GradProject.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GradProject.Web.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Orders
        // Filters: from,to,minTotal,maxTotal | mineOnly (Admins only)
        // Paging: page,pageSize
        public ActionResult Index(
            string from, string to,
            decimal? minTotal, decimal? maxTotal,
            bool? mineOnly,
            int page = 1, int pageSize = 10)
        {
            var q = db.Orders.AsQueryable();

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
                // إن كان Admin وفعّل "mineOnly"
                q = q.Where(o => o.UserId == userId);
            }

            // فلترة بالتواريخ (من عناصر input type="date")
            DateTime dt;
            if (!string.IsNullOrWhiteSpace(from) && DateTime.TryParse(from, out dt))
            {
                var fromDate = dt.Date;                  // بداية اليوم
                q = q.Where(o => o.CreatedAt >= fromDate);
            }

            if (!string.IsNullOrWhiteSpace(to) && DateTime.TryParse(to, out dt))
            {
                var toExclusive = dt.Date.AddDays(1);    // أول لحظة من اليوم التالي (حد علوي حصري)
                q = q.Where(o => o.CreatedAt < toExclusive);
            }

            // فلترة بالمبلغ
            if (minTotal.HasValue) q = q.Where(o => o.Total >= minTotal.Value);
            if (maxTotal.HasValue) q = q.Where(o => o.Total <= maxTotal.Value);

            // ترتيب افتراضي بالأحدث
            q = q.OrderByDescending(o => o.CreatedAt);

            // ترقيم صفحات
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = q.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var items = q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // قيم إلى الـ View
            ViewBag.from = from;
            ViewBag.to = to;
            ViewBag.minTotal = minTotal;
            ViewBag.maxTotal = maxTotal;
            ViewBag.mineOnly = mineOnly ?? false;

            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.totalCount = totalCount;
            ViewBag.totalPages = totalPages;

            return View(items);
        }


        // GET: /Orders/Details/5
        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();

            var order = db.Orders
                          .Include(o => o.Items.Select(i => i.Product))
                          .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null) return HttpNotFound();

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


        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}