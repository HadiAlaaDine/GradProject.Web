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

        // GET: /Orders
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var orders = db.Orders
                           .Where(o => o.UserId == userId)
                           .OrderByDescending(o => o.CreatedAt)
                           .ToList();
            return View(orders);
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