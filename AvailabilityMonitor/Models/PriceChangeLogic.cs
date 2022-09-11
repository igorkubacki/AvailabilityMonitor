namespace AvailabilityMonitor.Models
{
    public partial class BusinessLogic
    {
        public IEnumerable<PriceChange> GetAllPriceChanges()
        {
            return _context.PriceChange.ToList();
        }
        public PriceChange GetPriceChangeById(int id)
        {
            return _context.PriceChange.Find(id);
        }
        public IEnumerable<PriceChange> GetPriceChangesForProduct(int productId)
        {
            return _context.PriceChange.Where(i => i.ProductId == productId);
        }
        public void InsertPriceChange(PriceChange priceChange)
        {
            _context.Add(priceChange);
        }
        public bool IsPriceChangeEmpty()
        {
            return _context.PriceChange.Any();
        }
        public bool AnyPriceChangesForProduct(int productId)
        {
            return _context.PriceChange.Any(i => i.ProductId == productId);
        }
    }
}
