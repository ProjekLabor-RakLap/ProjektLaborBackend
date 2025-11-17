using Microsoft.AspNetCore.Mvc;
using ProjectLaborBackend.Controllers;
using ProjectLaborBackend.Entities;
using ProjectLaborBackend.Services;

namespace ProjectLaborBackend.Controllers
{
    [Route("api/excel")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        CsvOperations csvOperations;
        private IProductService productService;
        private IStockChangeService stockChangeService;
        private IStockService stockService;
        private IWarehouseService warehouseService;

        public ExcelController(IProductService productService, IStockChangeService stockChangeService, IWarehouseService warehouseService, IStockService stockService)
        {
            this.productService = productService;
            this.stockChangeService = stockChangeService;
            this.warehouseService = warehouseService;
            this.stockService = stockService;

            csvOperations = new CsvOperations();
        }

        [HttpPost("export")]
        public IActionResult ExportDataToExcel([FromBody] AppDbContext.Tables table)
        {
            try
            {
                byte[] excelBytes = csvOperations.ExportDataToExcel(table);

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{table}_Export.xlsx"
                );
            }
            catch (IOException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Database error: " + e.Message);
            }
        }

        [HttpPost("import")]
        public IActionResult ImportDataFromExcel([FromQuery] AppDbContext.Tables table,IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var data = csvOperations.ImportDataFromExcel(file);

                if (data.Count == 0)
                    return BadRequest("No data found in the Excel file.");

                switch (table)
                {
                    case AppDbContext.Tables.Products:
                        productService.InsertOrUpdate(data);
                        break;

                    case AppDbContext.Tables.StockChanges:
                        stockChangeService.InsertOrUpdate(data);
                        break;

                    case AppDbContext.Tables.Warehouses:
                        warehouseService.InsertOrUpdate(data);
                        break;

                    case AppDbContext.Tables.Stocks:
                        stockService.InsertOrUpdate(data);
                        break;

                    default:
                        return BadRequest("Unsupported table.");
                }

                return Ok("Imported");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("template")]
        public IActionResult DownloadTemplate([FromQuery] AppDbContext.Tables table)
        {
            try
            {
                var fileBytes = csvOperations.GenerateTemplateForTable(table);
                var fileName = $"{table}_Template.xlsx";

                return File(fileBytes,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating template: " + ex.Message);
            }
        }

    }
}
