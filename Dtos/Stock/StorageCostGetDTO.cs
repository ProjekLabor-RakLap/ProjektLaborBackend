using ProjectLaborBackend.Dtos.Product;

namespace ProjectLaborBackend.Dtos.Stock
{
    public class DailyStorageCostDTO
    {
        public DateTime Date { get; set; }
        public double Cost { get; set; }
    }

    public class ProductStorageCostDTO
    {
        public ProductGetNoPicDTO Product { get; set; }
        public List<DailyStorageCostDTO> DailyCosts { get; set; }
    }

    public class StorageCostGetDTO
    {
        public List<ProductStorageCostDTO> StorageCosts { get; set; }
    }
}
