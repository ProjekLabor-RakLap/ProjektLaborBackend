using ProjectLaborBackend.Dtos.Product;

namespace ProjectLaborBackend.Dtos.Stock
{
    public class StockGetWithProductDTO
    {
        public int Id { get; set; }
        public int StockInWarehouse { get; set; }
        public int StockInStore { get; set; }
        public int WarehouseCapacity { get; set; }
        public int StoreCapacity { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public int? WhenToNotify { get; set; } = 0;
        public int? WhenToWarn { get; set; } = 0;
        public ProductGetNoPicDTO Product { get; set; }
    }
}
