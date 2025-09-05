using GradProject.Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace GradProject.Web.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Orders
        // Filters: from,to,minTotal,myOnly  |  Sort: date/total  |  dir: asc/desc  |  Pagination: page,pageSize
        public ActionResult Index(
            DateTime? from, DateTime? to, decimal? minTotal,
            string sort = "date", string dir = "desc",
            int page = 1, int pageSize = 10, bool? myOnly = null)
        {
            var isAdmin = User.IsInRole("Admin");
            var userId = User.Identity.GetUserId();

            var q = db.Orders
                      .Include(o => o.Items)
                      .AsQueryable();

            // أمان: المستخدم العادي بيشوف بس طلباته
            // Admin بيشوف الكل إلا إذا طلب "myOnly=true"
            if (!isAdmin || (myOnly ?? false))
                q = q.Where(o => o.UserId == userId);

            // فلاتر المدة الزمنية
            if (from.HasValue) q = q.Where(o => DbFunctions.TruncateTime(o.CreatedAt) >= DbFunctions.TruncateTime(from.Value));
            if (to.HasValue) q = q.Where(o => DbFunctions.TruncateTime(o.CreatedAt) <= DbFunctions.TruncateTime(to.Value));

            // فلتر حسب الحد الأدنى للمبلغ
            if (minTotal.HasValue) q = q.Where(o => o.Total >= minTotal.Value);

            // ترتيب
            bool asc = (dir ?? "desc").ToLower() == "asc";
            switch ((sort ?? "date").ToLower())
            {
                case "total":
                    q = asc ? q.OrderBy(o => o.Total) : q.OrderByDescending(o => o.Total);
                    break;
                default: // date
                    q = asc ? q.OrderBy(o => o.CreatedAt) : q.OrderByDescending(o => o.CreatedAt);
                    break;
            }

            // Pagination
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = q.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var data = q.Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            // تمرير معلومات للـ View
            ViewBag.from = from?.ToString("yyyy-MM-dd");
            ViewBag.to = to?.ToString("yyyy-MM-dd");
            ViewBag.minTotal = minTotal;
            ViewBag.sort = sort;
            ViewBag.dir = asc ? "asc" : "desc";
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.totalCount = totalCount;
            ViewBag.totalPages = totalPages;
            ViewBag.myOnly = myOnly ?? !isAdmin; // الافتراضي: العادي يشوف طلباته فقط

            return View(data);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}