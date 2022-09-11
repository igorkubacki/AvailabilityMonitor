using AvailabilityMonitor.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Http.Headers;
using System.Xml;

namespace AvailabilityMonitor.Models
{
    public partial class BusinessLogic : IDisposable
    {
        private ApplicationDbContext _context;
        public Config completed;

        public BusinessLogic(ApplicationDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Product.ToList();
        }
        public Product GetProductById(int id)
        {
            return _context.Product.Find(id);
        }
        public IQueryable<Product> GetProducts(ProductSearch searchModel)
        {
            IQueryable<Product> products = _context.Product.AsQueryable();
            if(searchModel != null)
            {
                if (searchModel.PrestashopId.HasValue)
                    products = products.Where(p => p.PrestashopId == searchModel.PrestashopId);
                if (!string.IsNullOrEmpty(searchModel.Name))
                    products = products.Where(p => p.Name.ToLower().Contains(searchModel.Name.ToLower()));
                if (!string.IsNullOrEmpty(searchModel.Index))
                    products = products.Where(p => p.Index.ToLower().Contains(searchModel.Index.ToLower()));
                if (searchModel.PriceFrom.HasValue)
                    products = products.Where(p => p.RetailPrice >= searchModel.PriceFrom);
                if (searchModel.PriceTo.HasValue)
                    products = products.Where(p => p.RetailPrice <= searchModel.PriceTo);
                if (searchModel.QuantityFrom.HasValue)
                    products = products.Where(p => p.Quantity >= searchModel.QuantityFrom);
                if (searchModel.QuantityTo.HasValue)
                    products = products.Where(p => p.Quantity <= searchModel.QuantityTo);
            }
            return products;
        }
        public void InsertProduct(Product product)
        {
            _context.Add(product);
        }
        public void DeleteProduct(int productId)
        {
            Product? product = _context.Product.Find(productId);
            if(product != null)
                _context.Product.Remove(product);
        }
        public void UpdateProduct(Product product)
        {
            
        }
        public void Save()
        {
            _context.SaveChanges();
        }
        public bool IsProductEmpty()
        {
            return !_context.Product.Any();
        }
        private Config? GetConfig()
        {
            return _context.Config.First();
        }
        private async Task<XmlDocument> ResponseIntoXml(HttpResponseMessage result)
        {
            string? resultContent = await result.Content.ReadAsStringAsync();

            XmlDocument XmlFile = new XmlDocument();
            XmlFile.LoadXml(resultContent);

            return XmlFile;
        }
        private async Task<int> GetPrestaQuantity(HttpClient client, Config config, int stockavailableId)
        {
            Task<HttpResponseMessage>? request = client.GetAsync("stock_availables/" + stockavailableId + "?ws_key=" + config.prestaApiKey);
            request.Wait();
            HttpResponseMessage response = request.Result;

            if (response.IsSuccessStatusCode)
            {
                XmlDocument xmlDocument = await ResponseIntoXml(response);

                return int.Parse(xmlDocument.GetElementsByTagName("quantity").Item(0).InnerText);
            }
            else
            {
                return -1;
            }
        }
        public async Task ImportNewProductsFromPresta()
        {

            Config? config = GetConfig();

            if (config is null)
            {
                return;
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(config.prestaShopUrl + "/api/");

            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/xml"));

            // getting all products id's

            Task<HttpResponseMessage>? response = client.GetAsync("products/?ws_key=" + config.prestaApiKey);
            response.Wait();
            HttpResponseMessage? result = response.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument AllProductsXML = await ResponseIntoXml(result);

                int i = 0;

                foreach (XmlNode node in AllProductsXML.ChildNodes.Item(1).ChildNodes.Item(0).ChildNodes)
                {
                    int productId = int.Parse(node.Attributes[0].Value);

                    // Checking if product is already added.
                    if (_context.Product.Any(p => p.PrestashopId == productId))
                    {
                        continue;
                    }
                    Task<HttpResponseMessage>? productResponse = client.GetAsync("products/" + productId + "?ws_key=" + config.prestaApiKey);
                    productResponse.Wait();
                    HttpResponseMessage? productResponseResult = productResponse.Result;

                    if (productResponseResult.IsSuccessStatusCode)
                    {
                        XmlDocument productInfoXML = await ResponseIntoXml(productResponseResult);

                                                // Skip product if it's not displayed in PrestaShop.
                        if (productInfoXML.GetElementsByTagName("active").Item(0).InnerText == "0")
                        {
                            continue;
                        }

                        string index = productInfoXML.GetElementsByTagName("reference").Item(0).InnerText;
                        string name = productInfoXML.GetElementsByTagName("name").Item(0).ChildNodes.Item(0).InnerText;
                        string photoURL = config.prestaShopUrl + "/" + productInfoXML.GetElementsByTagName("id_default_image").Item(0).InnerText
                            + "-home_default/p.jpg";
                        int stockavailableId = int.Parse(productInfoXML.GetElementsByTagName("stock_available").Item(0).ChildNodes.Item(0).InnerText);
                        int quantity = await GetPrestaQuantity(client, config, stockavailableId);
                        float retailPrice = (float)1.23 * float.Parse(productInfoXML.GetElementsByTagName("price").Item(0).InnerText, CultureInfo.InvariantCulture);
                        retailPrice = Convert.ToSingle(retailPrice);
                        _context.Product.Add(new Product(productId, index, name, photoURL, stockavailableId, quantity, retailPrice));
                    }
                    else
                    {
                        throw new NotImplementedException();
                        // Console.WriteLine("{0} ({1})", (int)request.StatusCode, request.ReasonPhrase);
                    }
                    i++;
                    if (i % 10 == 0) Save();
                }
                Save();
            }

            else
            {
                throw new NotImplementedException();
                // Console.WriteLine("{0} ({1})", (int)request.StatusCode, request.ReasonPhrase);
            }
            client.Dispose();
            return;
        }
        public async Task<bool> UpdateInfoFromXmlFile()
        {
            Config? config = GetConfig();

            if (config is null)
            {
                return false;
            }

            HttpClient client = new HttpClient();

            Task<HttpResponseMessage>? request = client.GetAsync(config.supplierFileUrl);
            request.Wait();
            HttpResponseMessage? response = request.Result;

            if (response.IsSuccessStatusCode)
            {
                XmlDocument supplierProducts = await ResponseIntoXml(response);
                XmlNodeList? indexes = supplierProducts.GetElementsByTagName("Kod");

                IEnumerable<Product>? products = GetAllProducts();


                foreach (Product product in products)
                {
                    foreach (XmlNode node in indexes)
                    {
                        var index = node.InnerText.Trim();
                        if (product.Index == index)
                        {
                            XmlNode? supplierProduct = node.ParentNode;

                            UpdateProductSupplierInfo(product, supplierProduct);
                        }
                    }
                }
                Save();
            }
            return true;
        }
        public async Task<bool> UpdateInfoFromXmlFile(int productId)
        {
            Config? config = GetConfig();

            if (config is null)
            {
                return false;
            }

            HttpClient client = new HttpClient();

            Task<HttpResponseMessage>? request = client.GetAsync(config.supplierFileUrl);
            request.Wait();
            HttpResponseMessage? response = request.Result;

            if (response.IsSuccessStatusCode)
            {
                XmlDocument supplierProducts = await ResponseIntoXml(response);
                XmlNodeList? indexes = supplierProducts.GetElementsByTagName("Kod");

                Product? product = GetProductById(productId);

                if (product == null) return false;

                foreach (XmlNode node in indexes)
                {
                    var index = node.InnerText.Trim();
                    if (product.Index == index)
                    {
                        XmlNode supplierProduct = node.ParentNode;

                        await UpdateProductSupplierInfo(product, supplierProduct);
                        Save();
                    }
                }
            }

            return true;
        }
        private async Task UpdateProductSupplierInfo(Product product, XmlNode node)
        {
            product.SupplierQuantity = int.Parse(node.ChildNodes.Item(8).InnerText.Trim());

            product.SupplierWholesalePrice = float.Parse(node.ChildNodes.Item(5).InnerText.Trim(), CultureInfo.InvariantCulture);

            float supplierRetailPrice = float.Parse(node.ChildNodes.Item(7).InnerText.Trim(), CultureInfo.InvariantCulture);
            if (product.SupplierRetailPrice != supplierRetailPrice && product.SupplierRetailPrice != null)
            {
                InsertPriceChange(new PriceChange(product.ProductId, (float)product.SupplierRetailPrice, supplierRetailPrice, DateTime.Now));
            }
            product.SupplierRetailPrice = supplierRetailPrice;

            product.IsVisible = node.ChildNodes.Item(10).InnerText.Trim() == "1" ? true : false;
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
