using FruitStoreRepositories.Interfaces;
using FruitStoreRepositories.InterFaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FruitStoreRepositories.Implementation
{
    public class CronJobRepository : BackgroundService, ICronJobRepository
    {
        private readonly ICartDetailsRepository _cartDetailsRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<CronJobRepository> _logger;
        private readonly TimeSpan cartChronStartTime = TimeSpan.FromHours(0);
        private readonly TimeSpan cartChronEndTime = TimeSpan.FromHours(6);
        public CronJobRepository(ICartDetailsRepository cartDetailsRepository, ITokenRepository tokenRepository, ILogger<CronJobRepository> logger)
        {
            _cartDetailsRepository = cartDetailsRepository;
            _tokenRepository = tokenRepository;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation($"Excuting chron job schedular starts in method ExecuteAsync \n");

                while (!stoppingToken.IsCancellationRequested)
                {
                    TimeSpan currentTime = DateTime.Now.TimeOfDay;

                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

                    await _tokenRepository.SetTokenSchedularAsync();

                    if (currentTime >= cartChronStartTime && currentTime < cartChronEndTime)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                        await _cartDetailsRepository.SetSchedularAsync();
                    } 
                }
                _logger.LogInformation($"Excuting chron job schedular completed in method ExecuteAsync \n");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excuting chron job schedular failed in method ExecuteAsync : {ex.Message} \n");
                throw;
            }
        }
    }
}


