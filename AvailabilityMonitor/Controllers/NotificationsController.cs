using AvailabilityMonitor.Data;
using AvailabilityMonitor.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityMonitor.Controllers
{
    public class Notifications : Controller
    {
        private BusinessLogic _businessLogic;
        public Notifications(ApplicationDbContext context)
        {
            this._businessLogic = new BusinessLogic(context);
        }

        // GET: Notifications
        public ActionResult Index()
        {
            ViewData["PriceChanges"] = _businessLogic.GetAllPriceChanges().Where(p => p.IsNotificationRead == false);
            ViewData["QuantityChanges"] = _businessLogic.GetAllQuantityChanges().Where(p => p.IsNotificationRead == false);
            
            return View();
        }

        public void MarkPriceChangeAsRead(int id)
        {
            _businessLogic.GetPriceChangeById(id).IsNotificationRead = true;
            _businessLogic.Save();
        }
        
        public void MarkQuantityChangeAsRead(int id)
        {
            _businessLogic.GetQuantityChangeById(id).IsNotificationRead = true;
            _businessLogic.Save();
        }

        public void MarkAllChangesAsRead()
        {
            IEnumerable<PriceChange>? priceChanges = _businessLogic.GetAllPriceChanges();
            IEnumerable<QuantityChange>? quantityChanges = _businessLogic.GetAllQuantityChanges();

            foreach(PriceChange change in priceChanges)
            {
                change.IsNotificationRead = true;
            }
            foreach(QuantityChange change in quantityChanges)
            {
                change.IsNotificationRead = true;
            }

            _businessLogic.Save();
        }
    }
}
