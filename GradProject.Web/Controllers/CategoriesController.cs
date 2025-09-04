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
        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // CreatedAt لا نستقبلها من المستخدم – نحدّدها من السيرفر
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        // نحافظ على CreatedAt كما هو، ونعدّل فقط الحقول المسموح بها
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description")] Category input)
        {
            if (ModelState.IsValid)
            {
                var category = db.Categories.Find(input.Id);
                if (category == null) return HttpNotFound();

                category.Name = input.Name;
                category.Description = input.Description;
                // CreatedAt remains unchanged

                db.SaveChanges();
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }

            return View(input);
        }

        // GET: Categories/Delete/5
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