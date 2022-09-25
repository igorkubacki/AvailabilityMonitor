using AvailabilityMonitor.Data;
using AvailabilityMonitor.Models;
using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

namespace AvailabilityMonitor.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BusinessLogic _businessLogic;
        private FirebaseAuthProvider _auth;
        private static readonly string[] months = {
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sept",
            "Oct",
            "Nov",
            "Dec"
        };
        public ProductsController(ApplicationDbContext context)
        {
            _businessLogic = new BusinessLogic(context);
            _auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyDV25Lg8hXe7-85OCyuFySDPboHIvBIhws"));
        }
        public ActionResult Index(string? sortOrder, string name, string index, int? prestashopId, float? priceFrom, float? priceTo, 
            int? quantityFrom, int? quantityTo, int? page, int? pageSize)
        {
            // Check if there are any products in the database.
            if (_businessLogic.IsProductEmpty())
            {
                return View("AddProducts");
            }
            
            ProductSearch searchModel = new()
            {
                Name = name ?? "",
                Index = index ?? "",
                PrestashopId = prestashopId,
                PriceFrom = priceFrom,
                PriceTo = priceTo,
                QuantityFrom = quantityFrom,
                QuantityTo = quantityTo
            };

            IQueryable<Product> products = _businessLogic.GetProducts(searchModel);

            products = sortOrder switch
            {
                "id_desc" => products.OrderByDescending(p => p.PrestashopId),
                "name" => products.OrderBy(p => p.Name),
                "name_desc" => products.OrderByDescending(p => p.Name),
                "price" => products.OrderBy(p => p.RetailPrice),
                "price_desc" => products.OrderByDescending(p => p.RetailPrice),
                "quantity" => products.OrderBy(p => p.Quantity),
                "quantity_desc" => products.OrderByDescending(p => p.Quantity),
                _ => products.OrderBy(p => p.PrestashopId),
            };
            int productsPerPage = pageSize ?? 50;
            int pageNumber = page ?? 1;
            return View(products.ToPagedList(pageNumber, productsPerPage));
        }
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product? product = _businessLogic.GetProductById((int)id);

            if (product == null)
            {
                return NotFound();
            }
            
            // Price chart data handling
            List<float> prices = new List<float>();
            string priceLabels = "";
            
            if (_businessLogic.AnyPriceChangesForProduct((int)id))
            {
                IEnumerable<PriceChange> priceChanges = _businessLogic.GetPriceChangesForProduct((int)id);

                foreach (PriceChange change in priceChanges)
                {
                    prices.Add(change.NewPrice);
                    priceLabels += "," + change.DateTime.Day.ToString() + " " + months[change.DateTime.Month - 1] + " " + change.DateTime.Year.ToString();
                } 
            }

            string pricesJson = JsonConvert.SerializeObject(prices);
            ViewData["pricesJson"] = pricesJson;
            ViewData["priceLabelsJson"] = priceLabels;


            // Quantity chart data handling
            List<int> quantities = new List<int>();
            string quantityLabels = "";
            
            if (_businessLogic.AnyQuantityChangesForProduct((int)id))
            {
                IEnumerable<QuantityChange> quantityChanges = _businessLogic.GetQuantityChangesForProduct((int)id);

                foreach (QuantityChange change in quantityChanges)
                {
                    quantities.Add(change.NewQuantity);
                    quantityLabels += "," + change.DateTime.Day.ToString() + " " + months[change.DateTime.Month - 1] + " " + change.DateTime.Year.ToString();
                }
            }
            
            string quantitiesJson = JsonConvert.SerializeObject(quantities);
            ViewData["quantitiesJson"] = quantitiesJson;
            ViewData["quantityLabelsJson"] = quantityLabels;


            // Passing data
            // ViewData["PriceChanges"] = _context.PriceChange.Where(i => i.ProductId == id).ToList();
            // ViewData["QuantityChanges"] = _context.QuantityChange.Where(i => i.ProductId == id).ToList();
            
            return View(product);
        }

        public void Delete(int? id)
        {
            _businessLogic.DeleteProduct(id);
        }
        public async Task UpdateAllProductsFromPresta()
        {
            await _businessLogic.ImportNewProductsFromPresta();
        }
        public void UpdateProductFromPresta(int id)
        {
            _businessLogic.UpdateProductFromPresta(id);
        }
        public async Task UpdateAllProductsSupplierInfo()
        {
            await _businessLogic.UpdateSupplierInfo();
        }
        
        public async Task UpdateProductSupplierInfo(int id)
        {
            await _businessLogic.UpdateInfoFromXmlFile(id);
        }
    }
}
