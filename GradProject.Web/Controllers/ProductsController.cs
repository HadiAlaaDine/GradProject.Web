using System;
using System.Data.Entity;
using System.IO;
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
        // دالة العرض الرئيسية مع ميزات البحث والفلترة الكاملة
        public ActionResult Index(string searchString, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var products = db.Products.Include(p => p.Category);

            // 1. بحث بالاسم (أي حرف بيكتبه بيطلع النتيجة)
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.Name.Contains(searchString) || s.Description.Contains(searchString));
            }

            // 2. فلترة بالفئة
            if (categoryId != null && categoryId != 0)
            {
                products = products.Where(x => x.CategoryId == categoryId);
            }

            // 3. أقل سعر (Min Price)
            if (minPrice != null)
            {
                products = products.Where(x => x.Price >= minPrice);
            }

            // 4. أعلى سعر (Max Price)
            if (maxPrice != null)
            {
                products = products.Where(x => x.Price <= maxPrice);
            }

            // إرسال قائمة الفئات للقائمة المنسدلة
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");

            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Product product, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                // كود رفع الصورة مع إنشاء المجلد تلقائياً
                if (upload != null && upload.ContentLength > 0)
                {
                    // 1. تحديد مسار المجلد
                    string uploadDir = Server.MapPath("~/Content/Images");

                    // 2. التأكد من وجود المجلد، وإنشاؤه إذا لم يكن موجوداً
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // 3. حفظ الصورة
                    var fileName = DateTime.Now.Ticks + "_" + Path.GetFileName(upload.FileName);
                    var path = Path.Combine(uploadDir, fileName);
                    upload.SaveAs(path);

                    // 4. حفظ المسار في الداتابيز
                    product.ImageUrl = "/Content/Images/" + fileName;
                }

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Product product, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                // إذا رفع صورة جديدة، بنحدثها
                if (upload != null && upload.ContentLength > 0)
                {
                    string uploadDir = Server.MapPath("~/Content/Images");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    var fileName = DateTime.Now.Ticks + "_" + Path.GetFileName(upload.FileName);
                    var path = Path.Combine(uploadDir, fileName);
                    upload.SaveAs(path);
                    product.ImageUrl = "/Content/Images/" + fileName;
                }
                else
                {
                    // إذا ما رفع صورة جديدة، بنحافظ على القديمة
                    var oldProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.Id == product.Id);
                    if (oldProduct != null)
                    {
                        product.ImageUrl = oldProduct.ImageUrl;
                    }
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
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