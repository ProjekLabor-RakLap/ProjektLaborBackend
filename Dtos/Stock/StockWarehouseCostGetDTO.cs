using ProjectLaborBackend.Dtos.Product;
using ProjectLaborBackend.Dtos.StockChange;

namespace ProjectLaborBackend.Dtos.Stock
{
    public class StockWarehouseCostGetDTO
    {
        public List<ProductStockChangesDTO> ProductStockChanges { get; set; } = new();
    }

    public class ProductStockChangesDTO
    {
        public ProductGetNoPicDTO Product { get; set; }
        public StockGetNoPicDTO Stock { get; set; }
        public List<StockChangeGetDTO> StockChanges { get; set; } = new();
    }
}
