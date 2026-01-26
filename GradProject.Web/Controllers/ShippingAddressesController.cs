using GradProject.Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;

namespace GradProject.Web.Controllers
{
    [Authorize]
    public class ShippingAddressesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private string CurrentUserId => User.Identity.GetUserId();

        // GET: /ShippingAddresses
        public ActionResult Index()
        {
            var uid = CurrentUserId;
            var list = db.ShippingAddresses
                         .Where(a => a.UserId == uid)
                         .OrderByDescending(a => a.IsDefault)
                         .ThenByDescending(a => a.CreatedAt)
                         .ToList();
            return View(list);
        }

        // GET: /ShippingAddresses/Create
        public ActionResult Create()
        {
            return View(new ShippingAddress());
        }

        // POST: /ShippingAddresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ShippingAddress model)
        {
            // عبّي الحقول السيرفرية قبل التحقق
            model.UserId = CurrentUserId;
            model.CreatedAt = DateTime.UtcNow;

            // هدول ما بيجوا من الفورم
            ModelState.Remove("UserId");
            ModelState.Remove("CreatedAt");

            if (!ModelState.IsValid) return View(model);

            // إذا أول عنوان للمستخدم، عيّنه افتراضي
            bool hasAny = db.ShippingAddresses.Any(a => a.UserId == model.UserId);
            if (!hasAny) model.IsDefault = true;

            // لو اختار افتراضي، شيل الافتراضي عن غيره
            if (model.IsDefault)
            {
                var others = db.ShippingAddresses.Where(a => a.UserId == model.UserId && a.IsDefault);
                foreach (var a in others) a.IsDefault = false;
            }

            db.ShippingAddresses.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Address created.";
            return RedirectToAction("Index");
        }

        // GET: /ShippingAddresses/Edit/5
        public ActionResult Edit(int id)
        {
            var a = db.ShippingAddresses.FirstOrDefault(x => x.Id == id && x.UserId == CurrentUserId);
            if (a == null) return HttpNotFound();
            return View(a);
        }

        // POST: /ShippingAddresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ShippingAddress model)
        {
            // هدول ما بيجوا من الفورم
            ModelState.Remove("UserId");
            ModelState.Remove("CreatedAt");

            if (!ModelState.IsValid) return View(model);

            var uid = CurrentUserId;
            var a = db.ShippingAddresses.FirstOrDefault(x => x.Id == model.Id && x.UserId == uid);
            if (a == null) return HttpNotFound();

            a.FullName = model.FullName;
            a.Phone = model.Phone;
            a.AddressLine1 = model.AddressLine1;
            a.AddressLine2 = model.AddressLine2;
            a.City = model.City;
            a.State = model.State;
            a.PostalCode = model.PostalCode;
            a.Country = model.Country;

            // إدارة الافتراضي
            if (model.IsDefault && !a.IsDefault)
            {
                var others = db.ShippingAddresses.Where(x => x.UserId == uid && x.IsDefault);
                foreach (var o in others) o.IsDefault = false;
                a.IsDefault = true;
            }
            else if (!model.IsDefault && a.IsDefault)
            {
                a.IsDefault = false;
            }

            db.SaveChanges();
            TempData["Success"] = "Address updated.";
            return RedirectToAction("Index");
        }

        // GET: /ShippingAddresses/Delete/5
        public ActionResult Delete(int id)
        {
            var a = db.ShippingAddresses.FirstOrDefault(x => x.Id == id && x.UserId == CurrentUserId);
            if (a == null) return HttpNotFound();
            return View(a);
        }

        // POST: /ShippingAddresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var uid = CurrentUserId;
            var a = db.ShippingAddresses.FirstOrDefault(x => x.Id == id && x.UserId == uid);
            if (a == null) return HttpNotFound();

            bool wasDefault = a.IsDefault;
            db.ShippingAddresses.Remove(a);
            db.SaveChanges();

            // لو حذفنا الافتراضي، عيّن آخر واحد كافتراضي (إن وُجد)
            if (wasDefault)
            {
                var next = db.ShippingAddresses
                             .Where(x => x.UserId == uid)
                             .OrderByDescending(x => x.CreatedAt)
                             .FirstOrDefault();
                if (next != null)
                {
                    next.IsDefault = true;
                    db.SaveChanges();
                }
            }

            TempData["Success"] = "Address deleted.";
            return RedirectToAction("Index");
        }

        // POST: /ShippingAddresses/MakeDefault/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MakeDefault(int id)
        {
            var uid = CurrentUserId;
            var addr = db.ShippingAddresses.FirstOrDefault(x => x.Id == id && x.UserId == uid);
            if (addr == null) return HttpNotFound();

            var all = db.ShippingAddresses.Where(x => x.UserId == uid).ToList();
            foreach (var a in all) a.IsDefault = false;

            addr.IsDefault = true;
            db.SaveChanges();

            TempData["Success"] = "Default address set.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}