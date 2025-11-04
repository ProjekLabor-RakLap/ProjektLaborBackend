using AutoMapper;
using Microsoft.CodeAnalysis;
using ProjectLaborBackend.Entities;
using ProjectLaborBackend.Services;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace ProjectLaborBackend
{
    public class MinimumStockWatch : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public MinimumStockWatch(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();


                var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                List<Task> tasks = new List<Task>();

                List<Stock> stocks = _context.Stocks.ToList();
                foreach (Stock stock in stocks)
                {
                    if (stock.StockInWarehouse <= (0.1 * stock.WarehouseCapacity))
                    {
                        var users = await _context.Users.Where(u => u.Role == Role.Manager)
                                                            .Where(u => u.Warehouses
                                                                .Any(w => w.Stocks
                                                                    .Any(s => s.ProductId == stock.ProductId)))
                                                            .ToListAsync();
                        foreach (var user in users)
                        {
                            var templateBody = @"
                                                <html>
                                                <body>
                                                  <h2>Szia @Model.Name!</h2>
                                                  <p>A <b>@Model.ProductName</b> termék készlete lecsökkent.</p>
                                                  <p>Aktuális készlet: <b>@Model._Stock</b> db</p>
                                                </body>
                                                </html>";
                            Product? prod = await _context.Products.FirstOrDefaultAsync(i => i.Id == stock.ProductId);
                            var model = new {
                                Name = user.FirstName + " " + user.LastName,
                                ProductName = prod!.Name,
                                _Stock = stock.StockInWarehouse
                            };
                            await _emailService.SendEmailFromString(userEmail: user.Email, subject: "Alacsony termékszint értesítés", templateBody: templateBody, model:model);
                        }

                    }
                }

                try
                {
                }
                catch (Exception ex)
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

}
