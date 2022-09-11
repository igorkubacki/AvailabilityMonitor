using AvailabilityMonitor.Data;
using System.ComponentModel.DataAnnotations;

namespace AvailabilityMonitor.Models
{
    public class Config
    {
        private readonly ApplicationDbContext _context;

        [Key]
        public int id { get; set; }
        [DataType(DataType.Url)]
        [Display(Name = "Adress of the XML file")]
        public string supplierFileUrl { get; set; }
        [Display(Name = "PrestaShop adress")]
        [DataType(DataType.Url)]
        public string prestaShopUrl { get; set; }
        [StringLength(60)]
        [Display(Name = "PrestaShop API key")]
        public string prestaApiKey { get; set; }
    }
}