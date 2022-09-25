using AvailabilityMonitor.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Http.Headers;
using System.Xml;
using Microsoft.AspNet.SignalR;
using RealTimeProgressBar;
using System.Web.Mvc;

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
        public Product? GetProductById(int id)
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
        public void DeleteProduct(int? productId)
        {
            if(productId != null)
            {
                _context.Product.Remove(_context.Product.Find(productId));
                Save();
            }
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
            HttpResponseMessage result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument xmlDocument = await ResponseIntoXml(result);

                return int.Parse(xmlDocument.GetElementsByTagName("quantity").Item(0).InnerText);
            }
            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
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

            Task<HttpResponseMessage>? request = client.GetAsync("products/?ws_key=" + config.prestaApiKey);
            request.Wait();
            HttpResponseMessage? result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument AllProductsXML = await ResponseIntoXml(result);

                int i = 0;

                foreach (XmlNode node in AllProductsXML.ChildNodes.Item(1).ChildNodes.Item(0).ChildNodes)
                {
                    i++;

                    int productId = int.Parse(node.Attributes[0].Value);

                    Task<HttpResponseMessage>? productRequest = client.GetAsync("products/" + productId + "?ws_key=" + config.prestaApiKey);
                    productRequest.Wait();
                    HttpResponseMessage? productResult = productRequest.Result;

                    if (productResult.IsSuccessStatusCode)
                    {
                        XmlDocument productInfoXML = await ResponseIntoXml(productResult);

                        // Skip product if it's not displayed in PrestaShop.
                        if (productInfoXML.GetElementsByTagName("active").Item(0).InnerText == "0")
                        {
                            continue;
                        }

                        string index = productInfoXML.GetElementsByTagName("reference").Item(0).InnerText;
                        string name = productInfoXML.GetElementsByTagName("name").Item(0).ChildNodes.Item(0).InnerText;
                        string photoURL = config.prestaShopUrl + "/" + productInfoXML.GetElementsByTagName("id_default_image").Item(0).InnerText
                            + "-home_default/p.jpg";
                        string availabilityLabel = productInfoXML.GetElementsByTagName("available_now").Item(0).ChildNodes.Item(0).InnerText;
                        int stockavailableId = int.Parse(productInfoXML.GetElementsByTagName("stock_available").Item(0).ChildNodes.Item(0).InnerText);
                        int quantity = await GetPrestaQuantity(client, config, stockavailableId);
                        float retailPrice = (float)1.23 * float.Parse(productInfoXML.GetElementsByTagName("price").Item(0).InnerText, CultureInfo.InvariantCulture);
                        retailPrice = Convert.ToSingle(retailPrice);
                        
                        // Checking if product is already added. If it is, add it, in other case, update.
                        if (_context.Product.Any(p => p.PrestashopId == productId))
                        {
                            Product product = _context.Product.Where(p => p.PrestashopId == productId).First();

                            product.Index = index;
                            product.Name = name;
                            product.PhotoURL = photoURL;
                            product.AvailabilityLabel = availabilityLabel;
                            product.StockavailableId = stockavailableId;
                            product.Quantity = quantity;
                            product.RetailPrice = retailPrice;
                            Save();
                        }
                        else
                        {
                            _context.Product.Add(new Product(productId, index, name, photoURL, stockavailableId, quantity, retailPrice, availabilityLabel));
                        }
                    }
                    else
                    {
                        throw new BadHttpRequestException("Status code " + productResult.StatusCode.ToString() + " - " + productResult.ReasonPhrase);
                    }
                    if(i % 10 == 0) Save();
                }
            }

            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
            }
            client.Dispose();
            return;
        }

        public async Task UpdateProductFromPresta(int id)
        {
            Config? config = GetConfig();

            if (config is null)
            {
                return;
            }

            Product? product = GetProductById(id);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(config.prestaShopUrl + "/api/");

            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/xml"));

            Task<HttpResponseMessage>? request = client.GetAsync("products/" + product.PrestashopId + "?ws_key=" + config.prestaApiKey);
            request.Wait();
            HttpResponseMessage? result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument productInfoXML = await ResponseIntoXml(result);

                string index = productInfoXML.GetElementsByTagName("reference").Item(0).InnerText;
                string name = productInfoXML.GetElementsByTagName("name").Item(0).ChildNodes.Item(0).InnerText;
                string photoURL = config.prestaShopUrl + "/" + productInfoXML.GetElementsByTagName("id_default_image").Item(0).InnerText
                    + "-home_default/p.jpg";
                string availabilityLabel = productInfoXML.GetElementsByTagName("available_now").Item(0).ChildNodes.Item(0).InnerText;
                int stockavailableId = int.Parse(productInfoXML.GetElementsByTagName("stock_available").Item(0).ChildNodes.Item(0).InnerText);
                int quantity = await GetPrestaQuantity(client, config, stockavailableId);
                float retailPrice = (float)1.23 * float.Parse(productInfoXML.GetElementsByTagName("price").Item(0).InnerText, CultureInfo.InvariantCulture);
                retailPrice = Convert.ToSingle(retailPrice);

                product.Index = index;
                product.Name = name;
                product.PhotoURL = photoURL;
                product.AvailabilityLabel = availabilityLabel;
                product.StockavailableId = stockavailableId;
                product.Quantity = quantity;
                product.RetailPrice = retailPrice;
            }
            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
            }
            Save();
        }

        public async Task UpdateProductsFromPresta()
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

            Task<HttpResponseMessage>? request = client.GetAsync("products/?ws_key=" + config.prestaApiKey);
            request.Wait();
            HttpResponseMessage? result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument AllProductsXML = await ResponseIntoXml(result);

                int i = 0;

                foreach (XmlNode node in AllProductsXML.ChildNodes.Item(1).ChildNodes.Item(0).ChildNodes)
                {
                    int productId = int.Parse(node.Attributes[0].Value);

                    Product? product = _context.Product.Where(p => p.PrestashopId == productId).First();

                    // Checking if product with specified id exists in db. If not, skip.
                    if (product is null)
                    {
                        continue;
                    }

                    Task<HttpResponseMessage>? productRequest = client.GetAsync("products/" + productId + "?ws_key=" + config.prestaApiKey);
                    productRequest.Wait();
                    HttpResponseMessage? productResult = productRequest.Result;

                    if (productResult.IsSuccessStatusCode)
                    {
                        XmlDocument productInfoXML = await ResponseIntoXml(productResult);

                        string index = productInfoXML.GetElementsByTagName("reference").Item(0).InnerText;
                        string name = productInfoXML.GetElementsByTagName("name").Item(0).ChildNodes.Item(0).InnerText;
                        string photoURL = config.prestaShopUrl + "/" + productInfoXML.GetElementsByTagName("id_default_image").Item(0).InnerText
                            + "-home_default/p.jpg";
                        string availabilityLabel = productInfoXML.GetElementsByTagName("available_now").Item(0).ChildNodes.Item(0).InnerText;
                        int stockavailableId = int.Parse(productInfoXML.GetElementsByTagName("stock_available").Item(0).ChildNodes.Item(0).InnerText);
                        int quantity = await GetPrestaQuantity(client, config, stockavailableId);
                        float retailPrice = (float)1.23 * float.Parse(productInfoXML.GetElementsByTagName("price").Item(0).InnerText, CultureInfo.InvariantCulture);
                        retailPrice = Convert.ToSingle(retailPrice);

                        product.Index = index;
                        product.Name = name;
                        product.PhotoURL = photoURL;
                        product.AvailabilityLabel = availabilityLabel;
                        product.StockavailableId = stockavailableId;
                        product.Quantity = quantity;
                        product.RetailPrice = retailPrice;

                    }
                    else
                    {
                        throw new BadHttpRequestException("Status code " + productResult.StatusCode.ToString() + " - " + productResult.ReasonPhrase);
                    }
                    i++;
                    if (i % 10 == 0) Save();
                }
                Save();
            }

            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
            }
            client.Dispose();
            return;

        }
        public async Task<bool> UpdateSupplierInfo()
        {
            Config? config = GetConfig();

            if (config is null)
            {
                return false;
            }

            HttpClient client = new HttpClient();

            Task<HttpResponseMessage>? request = client.GetAsync(config.supplierFileUrl);
            request.Wait();
            HttpResponseMessage? result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument supplierProducts = await ResponseIntoXml(result);
                XmlNodeList? indexNodes = supplierProducts.GetElementsByTagName("Kod");

                IEnumerable<Product>? products = GetAllProducts();


                foreach (Product product in products)
                {
                    foreach (XmlNode node in indexNodes)
                    {
                        var index = node.InnerText.Trim();
                        if (product.Index == index)
                        {
                            XmlNode? supplierProduct = node.ParentNode;

                            await UpdateProductSupplierInfo(product, supplierProduct);
                            break;
                        }
                    }
                }
                Save();
            }
            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
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
            HttpResponseMessage? result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument supplierProducts = await ResponseIntoXml(result);
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
            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
            }

            return true;
        }
        private async Task UpdateProductSupplierInfo(Product product, XmlNode node)
        {
            int supplierQuantity = int.Parse(node.ChildNodes.Item(8).InnerText.Trim());
            // If value is different than the previous one, create a change object.
            if(product.SupplierQuantity != supplierQuantity && product.SupplierQuantity != null)
            {
                InsertQuantityChange(new QuantityChange(product.ProductId, (int)product.SupplierQuantity, supplierQuantity, DateTime.Now, false));
            }
            // If it's first import, then create a change.
            if(product.SupplierQuantity == null)
            {
                InsertQuantityChange(new QuantityChange(product.ProductId, 0, supplierQuantity, DateTime.Now, true));
                
            }
            product.SupplierQuantity = supplierQuantity;


            float supplierRetailPrice = float.Parse(node.ChildNodes.Item(7).InnerText.Trim(), CultureInfo.InvariantCulture);
            // If value is different than the previous one, create a change object.
            if (product.SupplierRetailPrice != supplierRetailPrice && product.SupplierRetailPrice != null)
            {
                InsertPriceChange(new PriceChange(product.ProductId, (float)product.SupplierRetailPrice, supplierRetailPrice, DateTime.Now, false));
            }
            // If it's first import, then create a change.
            if (product.SupplierRetailPrice == null)
            {
                InsertPriceChange(new PriceChange(product.ProductId, 0, supplierRetailPrice, DateTime.Now, true));
            }
            product.SupplierRetailPrice = supplierRetailPrice;


            product.SupplierWholesalePrice = float.Parse(node.ChildNodes.Item(5).InnerText.Trim(), CultureInfo.InvariantCulture);


            product.IsVisible = node.ChildNodes.Item(10).InnerText.Trim() == "1" ? true : false;

            Save();
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
