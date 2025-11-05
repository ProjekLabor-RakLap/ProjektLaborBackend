using AutoMapper;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProjectLaborBackend.Email;
using ProjectLaborBackend.Entities;
using ProjectLaborBackend.Services;
using System.Globalization;

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
                var threshold = DateTime.Now.AddHours(-24);
                var stocks = await _context.Stocks.Include(s => s.Product).Include(s => s.Warehouse).ToListAsync();
                var users = await _context.Users.Include(u => u.Warehouses).Where(u => u.Role == Role.Manager).ToListAsync();
                var emailLogs = await _context.EmailLogs.Where(e => e.SentDate >= threshold).ToListAsync();

                foreach (var user in users)
                {
                    // Find all stocks for user's warehouses that are low
                    var userStocks = stocks.Where(stock =>
                        (stock.StockInWarehouse < ((stock.WhenToNotify ?? 0) / 100 * stock.WarehouseCapacity) || stock.StockInWarehouse < ((stock.WhenToWarn ?? 0) / 100 * stock.WarehouseCapacity)) &&
                        user.Warehouses.Any(w => w.Id == stock.WarehouseId)).ToList();

                    if (!userStocks.Any()) continue;

                    // Filter out stocks already notified/warned in last24h
                    var stocksToNotify = new List<Stock>();
                    var stocksToWarn = new List<Stock>();
                    foreach (var stock in userStocks)
                    {
                        var recentEmails = emailLogs.Where(e => e.RecipientEmail == user.Email && e.ProductId == stock.ProductId).ToList();
                        bool hasNotification = recentEmails.Any(e => e.EmailType == EmailType.LowStockNotification);
                        bool hasWarning = recentEmails.Any(e => e.EmailType == EmailType.LowStockWarning);
                        if (stock.StockInWarehouse < (stock.WhenToWarn ?? 0) / 100 * stock.WarehouseCapacity)
                        {
                            if (!hasWarning)
                                stocksToWarn.Add(stock);
                        }
                        else if (!hasNotification && !hasWarning)
                        {
                            stocksToNotify.Add(stock);
                        }
                    }

                    if (stocksToWarn.Any() || stocksToNotify.Any())
                    {
                        var model = new StockEmailModel
                        {
                            Name = $"{user.FirstName} {user.LastName}",
                            WarningStocks = stocksToWarn.Select(s => new StockInfo
                            {
                                ProductName = s.Product.Name,
                                Stock = s.StockInWarehouse,
                                Capacity = s.WarehouseCapacity
                            }).ToList(),
                            NotificationStocks = stocksToNotify.Select(s => new StockInfo
                            {
                                ProductName = s.Product.Name,
                                Stock = s.StockInWarehouse,
                                Capacity = s.WarehouseCapacity
                            }).ToList()
                        };

                        await _emailService.SendEmail(
                            user.Email,
                            stocksToWarn.Any() ? "Kritikus termékszint figyelmeztetés" : "Alacsony termékszint figyelmeztetés",
                            $"{Directory.GetCurrentDirectory()}/Email/Templates/MinimumStock.cshtml",
                            model
                        );

                        foreach (var stock in stocksToWarn)
                        {
                            _context.EmailLogs.Add(new EmailLog
                            {
                                RecipientEmail = user.Email,
                                EmailType = EmailType.LowStockWarning,
                                SentDate = DateTime.Now,
                                ProductId = stock.ProductId
                            });
                        }
                        foreach (var stock in stocksToNotify)
                        {
                            _context.EmailLogs.Add(new EmailLog
                            {
                                RecipientEmail = user.Email,
                                EmailType = EmailType.LowStockNotification,
                                SentDate = DateTime.Now,
                                ProductId = stock.ProductId
                            });
                        }
                    }
                }
                try
                {
                   await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

}
