using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using GradProject.Web.Models;

namespace GradProject.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Categories
        // مسموح للكل يشوفوا قائمة الفئات
        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        // GET: Categories/Details/5
        // مسموح للكل يشوفوا تفاصيل الفئة
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        // ---------------------------------------------------------
        // منطقة الممنوعات: فقط الأدمن (Admin) بيقدر يفوت هون
        // ---------------------------------------------------------

        // GET: Categories/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "Id,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.UtcNow; // set by server
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Id,Name,Description")] Category input)
        {
            if (ModelState.IsValid)
            {
                var category = db.Categories.Find(input.Id);
                if (category == null) return HttpNotFound();

                category.Name = input.Name;
                category.Description = input.Description;

                db.SaveChanges();
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }

            return View(input);
        }

        // GET: Categories/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            db.Categories.Remove(category);
            db.SaveChanges();
            TempData["Success"] = "Category deleted successfully.";
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