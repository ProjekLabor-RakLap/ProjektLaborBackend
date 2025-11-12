namespace ProjectLaborBackend.Email.Models
{
    public class StockEmailModel
    {
        public string Name { get; set; } = "";
        public List<StockInfo> WarningStocks { get; set; } = new();
        public List<StockInfo> NotificationStocks { get; set; } = new();
    }

    public class StockInfo
    {
        public string ProductName { get; set; } = "";
        public int Stock { get; set; }
        public int Capacity { get; set; }
        public string WarehouseName { get; set; } = "";
    }
}
