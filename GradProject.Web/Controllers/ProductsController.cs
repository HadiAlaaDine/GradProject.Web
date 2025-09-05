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
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Products
        // Filters: q (search), categoryId, minPrice, maxPrice
        public ActionResult Index(string q, int? categoryId, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = db.Products
                          .Include(p => p.Category)
                          .AsQueryable();

            // فلترة نصّية على الاسم والوصف
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p =>
                    p.Name.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)));
            }

            // فلترة حسب التصنيف
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // فلترة حسب السعر
            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

            // ترتيب + إحصاء + تقطيع صفحات
            query = query.OrderByDescending(p => p.CreatedAt);

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            // تمرير قيم الفلاتر والـPager للـView
            ViewBag.q = q;
            ViewBag.categoryId = categoryId;
            ViewBag.minPrice = minPrice;
            ViewBag.maxPrice = maxPrice;

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            // للقائمة المنسدلة للتصنيفات
            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "Id", "Name", categoryId);

            return View(items);
        }


        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Price,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.UtcNow; // يحدد من السيرفر
                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Price,CategoryId")] Product input)
        {
            if (ModelState.IsValid)
            {
                var product = db.Products.Find(input.Id);
                if (product == null) return HttpNotFound();

                product.Name = input.Name;
                product.Description = input.Description;
                product.Price = input.Price;
                product.CategoryId = input.CategoryId;
                // CreatedAt يبقى كما هو

                db.SaveChanges();
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "Id", "Name", input.CategoryId);
            return View(input);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
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
