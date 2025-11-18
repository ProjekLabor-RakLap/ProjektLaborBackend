using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLaborBackend.Dtos.Product;
using ProjectLaborBackend.Dtos.Stock;
using ProjectLaborBackend.Dtos.StockChange;
using ProjectLaborBackend.Entities;

namespace ProjectLaborBackend.Services
{
    public interface IStockService
    {
        Task<List<StockGetDTO>> GetAllStocksAsync();
        Task<List<StockGetWithProductDTO>> GetStocksByWarehouseAsync(int warehouseId);
        Task<StockGetDTO?> GetStockByIdAsync(int id);
        Task<StockGetDTO> CreateStockAsync(StockCreateDTO stock);
        Task<StockGetDTO> UpdateStockAsync(int id, StockUpdateDto dto);
        Task DeleteStockAsync(int id);
        void InsertOrUpdate(List<List<string>> data);
        Task<StockGetDTO?> GetStockByProductAsync(int productId);
        Task UpdateStockAfterStockChange(int stockId, int warehouseId, int quantity);
        Task<StockWarehouseCostGetDTO> WarehouseCost(int warehouseId);
        Task<StorageCostGetDTO> StorageCost(int warehouseId);
    }

    public class StockService : IStockService
    {
        private readonly AppDbContext _context;
        private IMapper _mapper;

        public StockService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StockGetDTO> CreateStockAsync(StockCreateDTO stock)
        {
            if (stock.Currency.Length > 50)
                throw new ArgumentOutOfRangeException("Currency cannot exceed 50 characters!");

            if (stock.StockInWarehouse < 0 || stock.StockInStore < 0)
                throw new ArgumentOutOfRangeException("Stock cannot be negative!");

            if (stock.WarehouseCapacity <= 0 || stock.StoreCapacity <= 0)
                throw new ArgumentOutOfRangeException("Capacity cannot be equal or less than 0!");

            if (stock.StockInStore > stock.StoreCapacity && stock.StockInWarehouse > stock.WarehouseCapacity)
                throw new Exception("Stock in store and warehouse cannot exceed their capacities!");

            var warehouse = await _context.Warehouses.FindAsync(stock.WarehouseId);
            if (warehouse == null)
                throw new KeyNotFoundException("Warehouse with that id does not exist!");

            var product = await _context.Products.FindAsync(stock.ProductId);
            if (product == null)
                throw new KeyNotFoundException("Warehouse with that id does not exist!");


            if (stock.WhenToNotify != null && stock.WhenToNotify > 100)
                throw new ArgumentException("Notify threshold cant be more than 100%");
            if (stock.WhenToWarn != null && stock.WhenToWarn > 100)
                throw new ArgumentException("Warn threshold cant be more than 100%");

            if(stock.WhenToWarn != null && stock.WhenToNotify != null)
            {
                if(stock.WhenToWarn > stock.WhenToNotify)
                {
                    throw new ArgumentException("When to warn percentage cannot be greater than when to notify percentage!");
                }
            }

            await _context.Stocks.AddAsync(_mapper.Map<Stock>(stock));
            await _context.SaveChangesAsync();
            return _mapper.Map<StockGetDTO>(await _context.Stocks.FirstOrDefaultAsync(o => o.ProductId == stock.ProductId && o.WarehouseId == stock.WarehouseId));
        }

        public async Task DeleteStockAsync(int id)
        {
            Stock? stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
                throw new KeyNotFoundException("Stock not found");

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
        }

        public async Task<List<StockGetDTO>> GetAllStocksAsync()
        {
            return _mapper.Map<List<StockGetDTO>>(await _context.Stocks.Include(p => p.Product).Include(w => w.Warehouse).ToListAsync());
        }

        public async Task<StockGetDTO?> GetStockByIdAsync(int id)
        {
            Stock? stock = await _context.Stocks.Include(p => p.Product).Include(w => w.Warehouse).FirstOrDefaultAsync(i => i.Id == id);
            if (stock == null)
                throw new KeyNotFoundException("Stock not found!");

            return _mapper.Map<StockGetDTO>(stock);
        }

        public async Task<List<StockGetWithProductDTO>> GetStocksByWarehouseAsync(int warehouseId)
        {
            List<Stock> stocks = await _context.Stocks
                .Include(p => p.Product)
                .Include(w => w.Warehouse)
                .Where(s => s.WarehouseId == warehouseId)
                .ToListAsync();
            return _mapper.Map<List<StockGetWithProductDTO>>(stocks);
        }

        public async Task<StockGetDTO> UpdateStockAsync(int id, StockUpdateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException("No data to be changed!");

            Stock? stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
                throw new KeyNotFoundException("Stock not found!");
            
            if (dto.Currency != null && dto.Currency.Length > 50) //50???? 9 a leghosszabb a világon...
                throw new ArgumentOutOfRangeException("Currency cannot exceed 50 characters!");
            else if (dto.Currency == null)
                dto.Currency = stock.Currency;


            var newStoreCapacity = dto.StoreCapacity ?? stock.StoreCapacity;
            var newStoreStock = dto.StockInStore ?? stock.StockInStore;

            if (newStoreStock < 0)
                throw new ArgumentOutOfRangeException("Stock in store cannot be negative!");
            if (newStoreCapacity <= 0)
                throw new ArgumentOutOfRangeException("Store capacity cannot be equal or less than 0!");

            if (newStoreStock > newStoreCapacity)
                throw new Exception("Stock in store cannot exceed its capacity!");

            dto.StoreCapacity = newStoreCapacity;
            dto.StockInStore = newStoreStock;
            


            var newWarehouseCapacity = dto.WarehouseCapacity ?? stock.WarehouseCapacity;
            var newWarehouseStock = dto.StockInWarehouse ?? stock.StockInWarehouse;

            if (newWarehouseStock < 0)
                throw new ArgumentOutOfRangeException("Stock in warehouse cannot be negative!");
            if (newWarehouseCapacity <= 0)
                throw new ArgumentOutOfRangeException("Warehouse capacity cannot be equal or less than 0!");
            if (newWarehouseStock > newWarehouseCapacity)
                throw new ArgumentException("Stock in warehouse cannot exceed its capacity!");
            
            dto.WarehouseCapacity = newWarehouseCapacity;
            dto.StockInWarehouse = newWarehouseStock;


            if (dto.WarehouseId != null)
            {
                var warehouse = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == dto.WarehouseId);
                if (warehouse == null)
                {
                    throw new KeyNotFoundException($"Warehouse with {dto.WarehouseId} id does not exist!");
                }
            }
            else
            {
                dto.WarehouseId = stock.WarehouseId;
            }


            if (dto.ProductId != null)
            {
                var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == dto.ProductId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with {dto.ProductId} id does not exist!");
                }
            }
            else
            {
                dto.ProductId = stock.ProductId;
            }


            var newWarn = dto.WhenToWarn ?? stock.WhenToWarn ?? 0;
            var newNotify = dto.WhenToNotify ?? stock.WhenToNotify ?? 100;

            if (newWarn > newNotify)
                throw new ArgumentException("When to warn percentage cannot be greater than when to notify percentage!");
          
            dto.WhenToWarn = newWarn;
            dto.WhenToNotify = newNotify;           


            if(dto.Currency == null) 
                dto.Currency = stock.Currency;
            if(dto.Price == null) 
                dto.Price = stock.Price;

            _mapper.Map(dto, stock);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\n" + ex.InnerException.Message);
            }

            var newstock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.Id == id);
            Console.WriteLine("\n\n\n\n");
            Console.WriteLine(stock.Warehouse.Id);
            Console.WriteLine("\n\n\n\n");
            return _mapper.Map<StockGetDTO>(newstock);
        }
        public void InsertOrUpdate(List<List<string>> data)
        {
            List<Stock> currentStock = _context.Stocks.ToList();
            List<Stock> stocksFromExcel = new List<Stock>();
            List<Stock> stocksToAdd = new List<Stock>();
            List<Stock> stocksToUpdate = new List<Stock>();
            foreach (List<string> item in data)
            {
                stocksFromExcel.Add(new Stock
                {
                    StockInWarehouse = int.Parse(item[0]),
                    StockInStore = int.Parse(item[1]),
                    WarehouseCapacity = int.Parse(item[2]),
                    StoreCapacity = int.Parse(item[3]),
                    ProductId = int.Parse(item[4]),
                    Currency = item[5],
                    Price = double.Parse(item[6]),
                    WarehouseId = int.Parse(item[7])
                });
            }
            if(stocksFromExcel.Count == 0)
                throw new ArgumentNullException("No data to be changed!");
            if (stocksFromExcel.Any(s => s.Currency.Length > 50))
                throw new ArgumentOutOfRangeException("Currency cannot exceed 50 characters!");
            if (stocksFromExcel.Any(s => s.StockInWarehouse < 0 || s.StockInStore < 0))
                throw new ArgumentOutOfRangeException("Stock cannot be negative!");
            if (stocksFromExcel.Any(s => s.WarehouseCapacity <= 0 || s.StoreCapacity <= 0))
                throw new ArgumentOutOfRangeException("Capacity cannot be equal or less than 0!");
            if (stocksFromExcel.Any(s => s.StockInStore > s.StoreCapacity && s.StockInWarehouse > s.WarehouseCapacity))
                throw new Exception("Stock in store and warehouse cannot exceed their capacities!");
            if (stocksFromExcel.Any(s => !_context.Warehouses.Any(w => w.Id == s.WarehouseId)))
                throw new KeyNotFoundException("One or more warehouses with given ids do not exist!");
            if (stocksFromExcel.Any(s => !_context.Products.Any(p => p.Id == s.ProductId)))
                throw new KeyNotFoundException("One or more products with given ids do not exist!");
            if (stocksFromExcel.Any(s => currentStock.Count(cs => cs.ProductId == s.ProductId) > 1))
                throw new Exception("There are duplicate products in the database!");
            if (stocksFromExcel.Any(s => stocksFromExcel.Count(cs => cs.ProductId == s.ProductId) > 1))
                throw new Exception("There are duplicate products in the imported data!");
            if (stocksFromExcel.Any(s => currentStock.Any(cs => cs.ProductId == s.ProductId && (s.StockInStore < 0 || s.StockInWarehouse < 0))))
                throw new Exception("Stock in store or warehouse cannot be negative!");
            if (stocksFromExcel.Any(s => currentStock.Any(cs => cs.ProductId == s.ProductId && s.Price < 0)))
                throw new ArgumentOutOfRangeException("Price cannot be negative!");

            foreach (Stock stock in stocksFromExcel)
            {
                if (!currentStock.Any(p => p.ProductId == stock.ProductId))
                {
                    stocksToAdd.Add(stock);
                }
                else
                {
                    Stock existingStock = currentStock.First(p => p.ProductId == stock.ProductId);
                    existingStock.StockInStore = stock.StockInStore;
                    existingStock.StockInWarehouse = stock.StockInWarehouse;
                    existingStock.StoreCapacity = stock.StoreCapacity;
                    existingStock.WarehouseCapacity = stock.WarehouseCapacity;
                    existingStock.Price = stock.Price;
                    existingStock.Currency = stock.Currency;
                    stocksToUpdate.Add(existingStock);
                }
            }

            if (stocksToAdd.Count > 0)
                _context.Stocks.AddRange(stocksToAdd);
            if (stocksToUpdate.Count > 0)
                _context.Stocks.UpdateRange(stocksToUpdate);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving changes to the database.", ex);
            }
        }

        public async Task UpdateStockAfterStockChange(int productId, int warehouseId, int quantity)
        {
            Stock? stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);
            if (stock == null)
                throw new KeyNotFoundException("Stock not found!");

            if (stock.StockInWarehouse + quantity < 0)
                throw new ArgumentOutOfRangeException("Stock in warehouse cannot be negative!");

            stock.StockInWarehouse += quantity;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\n" + ex.InnerException.Message);
            }
        }
        
        public async Task<StockGetDTO?> GetStockByProductAsync(int productId)
        {
            Stock? stock = await _context.Stocks.Include("Product").Where(x => x.Product.Id == productId).FirstOrDefaultAsync();
            if (stock == null)
                throw new KeyNotFoundException("Stock not found!");

            return _mapper.Map<StockGetDTO>(stock);
        }

        public async Task<StockWarehouseCostGetDTO> WarehouseCost(int warehouseId)
        {
            var products = await _context.Products
                .Include(p => p.Stocks)
                .ThenInclude(s => s.Warehouse)
                .Where(p => p.Stocks.Any(s => s.WarehouseId == warehouseId))
                .ToListAsync();

            var stockWarehouseCost = new StockWarehouseCostGetDTO();

            foreach (var product in products)
            {
                var stock = product.Stocks.FirstOrDefault(s => s.WarehouseId == warehouseId);
                if (stock == null) continue;

                var stockChanges = await _context.StockChanges
                    .Where(sc => sc.ProductId == product.Id)
                    .ToListAsync();

                stockWarehouseCost.ProductStockChanges.Add(new ProductStockChangesDTO
                {
                    Product = _mapper.Map<ProductGetNoPicDTO>(product),
                    Stock = _mapper.Map<StockGetNoPicDTO>(stock),
                    StockChanges = _mapper.Map<List<StockChangeGetDTO>>(stockChanges)
                });
            }

            return stockWarehouseCost;
        }

        public async Task<StorageCostGetDTO> StorageCost(int warehouseId)
        {
            var products = await _context.Products
                .Include(p => p.Stocks)
                .ThenInclude(s => s.Warehouse)
                .Where(p => p.Stocks.Any(s => s.WarehouseId == warehouseId))
                .ToListAsync();

            StorageCostGetDTO storageCostDto = new StorageCostGetDTO
            {
                StorageCosts = new List<ProductStorageCostDTO>()
            };

            DateTime now = DateTime.UtcNow.Date;

            foreach (var product in products)
            {
                var stock = product.Stocks.FirstOrDefault(s => s.WarehouseId == warehouseId);
                if (stock == null) continue;

                var stockChanges = await _context.StockChanges
                    .Where(sc => sc.ProductId == product.Id)
                    .OrderBy(sc => sc.ChangeDate)
                    .ToListAsync();

                var dailyCosts = new List<DailyStorageCostDTO>();

                DateTime startDate = stockChanges.Count > 0
                    ? stockChanges.First().ChangeDate.Date
                    : now.AddDays(-30);

                int currentStock = stock.StockInWarehouse;

                int changeIndex = 0;
                for (DateTime day = startDate; day <= now; day = day.AddDays(1))
                {
                    while (changeIndex < stockChanges.Count && stockChanges[changeIndex].ChangeDate.Date == day)
                    {
                        currentStock += stockChanges[changeIndex].Quantity;
                        if (currentStock < 0) currentStock = 0;
                        changeIndex++;
                    }

                    double dailyCost = currentStock * stock.StorageCost;

                    dailyCosts.Add(new DailyStorageCostDTO
                    {
                        Date = day,
                        Cost = dailyCost
                    });
                }

                storageCostDto.StorageCosts.Add(new ProductStorageCostDTO
                {
                    Product = new ProductGetNoPicDTO
                    {
                        Id = product.Id,
                        Name = product.Name,
                        EAN = product.EAN,
                        Description = product.Description
                    },
                    DailyCosts = dailyCosts
                });
            }

            return storageCostDto;
        }


    }
}
