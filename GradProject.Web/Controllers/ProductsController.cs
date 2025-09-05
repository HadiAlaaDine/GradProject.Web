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
        public ActionResult Index(string q, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var products = db.Products
                             .Include(p => p.Category)
                             .AsQueryable();

            // فلترة نصّية على الاسم والوصف
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                products = products.Where(p =>
                    p.Name.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)));
            }

            // فلترة حسب التصنيف
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // فلترة حسب السعر
            if (minPrice.HasValue) products = products.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) products = products.Where(p => p.Price <= maxPrice.Value);

            // ترتيب افتراضي
            products = products.OrderBy(p => p.Name);

            // DropDown التصنيفات
            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "Id", "Name", categoryId);

            // إعادة القيم الحالية للـ View لعرضها في الفورم
            ViewBag.q = q;
            ViewBag.minPrice = minPrice;
            ViewBag.maxPrice = maxPrice;

            return View(products.ToList());
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
