using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvailabilityMonitor.Models
{
    public class QuantityChange
    {
        [Key]
        public int QuantityChangeId { get; set; }
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsNotificationRead { get; set; } = false;
        [ForeignKey("ProductId")]
        public int ProductId { get; set; }
        public virtual Product? product { get; set; }

        public QuantityChange(int productId, int previousQuantity, int newQuantity, DateTime dateTime)
        {
            ProductId = productId;
            PreviousQuantity = previousQuantity;
            NewQuantity = newQuantity;
            DateTime = dateTime;
        }
    }
}
