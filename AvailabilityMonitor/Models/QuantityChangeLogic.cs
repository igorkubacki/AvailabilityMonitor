namespace AvailabilityMonitor.Models
{
    public partial class BusinessLogic
    {
        public IEnumerable<QuantityChange> GetAllQuantityChanges()
        {
            return _context.QuantityChange.ToList();
        }
        public QuantityChange GetQuantityChangeById(int id)
        {
            return _context.QuantityChange.Find(id);
        }
        public IEnumerable<QuantityChange> GetQuantityChangesForProduct(int productId)
        {
            return _context.QuantityChange.Where(i => i.ProductId == productId);
        }
        public void InsertQuantityChange(QuantityChange quantityChange)
        {
            _context.QuantityChange.Add(quantityChange);
        }
        public bool IsQuantityChangeEmpty()
        {
            return _context.QuantityChange.Any();
        }
        public bool AnyQuantityChangesForProduct(int productId)
        {
            return _context.QuantityChange.Any(i => i.ProductId == productId);
        }
    }
}
