using AvailabilityMonitor.Data;
using AvailabilityMonitor.Models;
using AvailabilityMonitor.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

namespace AvailabilityMonitor.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext _context;
        private BusinessLogic _businessLogic;
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
        }
        public ActionResult Index(string? sortOrder, string name, string index, int? prestashopId, float? priceFrom, float? priceTo, 
            int? quantityFrom, int? quantityTo, int? page, int? pageSize)
        {
            // Check if there are any products in the database.
            if (_businessLogic.IsProductEmpty())
            {
                return View("AddProducts");
            }
            
            ProductSearch searchModel = new ProductSearch()
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
            
            switch (sortOrder)
            {
                case "id_desc":
                    products = products.OrderByDescending(p => p.PrestashopId);
                    break;
                case "name":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price":
                    products = products.OrderBy(p => p.RetailPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.RetailPrice);
                    break;
                case "quantity":
                    products = products.OrderBy(p => p.Quantity);
                    break;
                case "quantity_desc":
                    products = products.OrderByDescending(p => p.Quantity);
                    break;
                case "index":
                    products = products.OrderBy(p => p.Index);
                    break;
                case "index_desc":
                    products = products.OrderByDescending(p => p.Index);
                    break;
                default:
                    products = products.OrderBy(p => p.PrestashopId);
                    break;
            }

            int productsPerPage = pageSize ?? 50;
            int pageNumber = page ?? 1;
            return View(products.ToPagedList(pageNumber, productsPerPage));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id, index, name, wholesalePrice, retailPrice, quantity, isVisible")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);

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
            string priceLabels = DateTime.Now.Day.ToString() + " " + months[DateTime.Now.Month - 2] + " " + DateTime.Now.Year.ToString();
            
            if (_businessLogic.AnyPriceChangesForProduct((int)id))
            {
                var priceChanges = _businessLogic.GetPriceChangesForProduct((int)id);

                prices.Add(priceChanges.First().PreviousPrice);
                foreach (PriceChange change in priceChanges)
                {
                    prices.Add(change.NewPrice);
                    priceLabels += "," + change.DateTime.Day.ToString() + " " + months[change.DateTime.Month - 1] + " " + change.DateTime.Year.ToString();
                } 
            }
            else
            {
                prices.Add(product.SupplierRetailPrice ?? 0);
            }
                string pricesJson = JsonConvert.SerializeObject(prices);
                ViewData["pricesJson"] = pricesJson;
                ViewData["priceLabelsJson"] = priceLabels;


            // Quantity chart data handling
            List<int> quantities = new List<int>();
            string quantityLabels = DateTime.Now.Day.ToString() + " " + months[DateTime.Now.Month - 2] + " " + DateTime.Now.Year.ToString();
            
            if (_businessLogic.AnyQuantityChangesForProduct((int)id))
            {
                var quantityChanges = _businessLogic.GetQuantityChangesForProduct((int)id);
                quantities.Add(quantityChanges.First().PreviousQuantity);
                foreach (QuantityChange change in quantityChanges)
                {
                    quantities.Add(change.NewQuantity);
                    quantityLabels += "," + change.DateTime.Day.ToString() + " " + months[change.DateTime.Month - 1] + " " + change.DateTime.Year.ToString();
                }


            }
            else
            {
                quantities.Add(product.SupplierQuantity ?? 0);
            }
            string quantitiesJson = JsonConvert.SerializeObject(quantities);
            ViewData["quantitiesJson"] = quantitiesJson;
            ViewData["quantityLabelsJson"] = quantityLabels;


            // Passing data
            // ViewData["PriceChanges"] = _context.PriceChange.Where(i => i.ProductId == id).ToList();
            // ViewData["QuantityChanges"] = _context.QuantityChange.Where(i => i.ProductId == id).ToList();
            
            return View(product);
        }
        public ActionResult AddProducts()
        {
            return View();
        }
        public void ImportProducts()
        {
            _businessLogic.ImportNewProductsFromPresta();
        }
        public async Task<ActionResult> AddSupplierInfo()
        {
            return await _businessLogic.UpdateInfoFromXmlFile() ? RedirectToAction(nameof(Index)) :
                RedirectToAction(actionName: "Create", controllerName: "Configuration");
        }
        
        [ActionName("UpdateProductInfo")]
        public async Task<ActionResult> UpdateProductInfo(int productId)
        {
            return await _businessLogic.UpdateInfoFromXmlFile(productId) ? RedirectToAction(nameof(Details), new { id = productId }) : 
                RedirectToAction(actionName: "Create", controllerName: "Configuration");
        }

    }
}
