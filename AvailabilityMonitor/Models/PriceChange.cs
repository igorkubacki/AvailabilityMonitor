using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvailabilityMonitor.Models
{
    public class PriceChange
    {
        [Key]
        public int PriceChangeId { get; set; }
        public float PreviousPrice { get; set; }
        public float NewPrice { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsNotificationRead { get; set; } = false;
        [ForeignKey("ProductId")]
        public int ProductId { get; set; }
        public virtual Product? product { get; set; }

        public PriceChange(int productId, float previousPrice, float newPrice, DateTime dateTime, bool isNotificationRead)
        {
            ProductId = productId;
            PreviousPrice = previousPrice;
            NewPrice = newPrice;
            DateTime = dateTime;
            IsNotificationRead = isNotificationRead;
        }
    }
}
