using AutoMapper;
using ProjectLaborBackend.Entities;

namespace ProjectLaborBackend
{
    public class MinimumStockWatch : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private AppDbContext _context;
        private readonly Random _random = new();
        public MinimumStockWatch(AppDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                List<Task> tasks = new List<Task>();

                
                
                await Task.WhenAll(tasks);
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
