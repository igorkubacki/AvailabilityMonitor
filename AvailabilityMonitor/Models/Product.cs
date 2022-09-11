using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvailabilityMonitor.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public int PrestashopId { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public string PhotoURL { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public float? SupplierWholesalePrice { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public float RetailPrice { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public float? SupplierRetailPrice { get; set; }
        public int Quantity { get; set; }
        public int? SupplierQuantity { get; set; }
        public int StockavailableId { get; set; }
        public bool IsVisible { get; set; }
        [ForeignKey("ProductId")]
        public virtual ICollection<PriceChange>? PriceChanges { get; set; }
        [ForeignKey("ProductId")]
        public virtual ICollection<QuantityChange>? QuantityChanges { get; set; }


        public Product(int prestashopId, string index, string name, string photoURL, int stockavailableId, int quantity, float retailPrice)
        {
            PrestashopId = prestashopId;
            Index = index;
            Name = name;
            PhotoURL = photoURL;
            StockavailableId = stockavailableId;
            Quantity = quantity;
            RetailPrice = retailPrice;
        }

        

    }
}
