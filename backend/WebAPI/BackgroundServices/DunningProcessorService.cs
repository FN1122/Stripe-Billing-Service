using Core.RepositoryContracts;

namespace StripeBilling.API.BackgroundServices
{
    public class DunningProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DunningProcessorService> _logger;

        public DunningProcessorService(IServiceProvider serviceProvider, ILogger<DunningProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dunningRepo = scope.ServiceProvider.GetRequiredService<IDunningRepository>();
                    var dueSchedules = await dunningRepo.GetDueSchedulesAsync();

                    foreach (var schedule in dueSchedules)
                    {
                        try
                        {
                            var steps = await dunningRepo.GetStepsAsync(schedule.TenantId);
                            if (schedule.CurrentStep >= steps.Count)
                            {
                                schedule.Status = "cancelled";
                                schedule.UpdatedAt = DateTime.UtcNow;
                                await dunningRepo.UpdateAsync(schedule);
                                continue;
                            }

                            var currentStep = steps[schedule.CurrentStep];
                            _logger.LogInformation("Processing dunning step {Step} ({Action}) for schedule {ScheduleId}", schedule.CurrentStep, currentStep.Action, schedule.Id);

                            schedule.TotalRetryAttempts++;
                            schedule.LastRetryAt = DateTime.UtcNow;
                            schedule.CurrentStep++;

                            if (schedule.CurrentStep < steps.Count)
                            {
                                var nextStep = steps[schedule.CurrentStep];
                                schedule.NextRetryAt = schedule.OriginalFailureDate.AddDays(nextStep.DaysAfterFailure);
                            }
                            else
                            {
                                schedule.NextRetryAt = null;
                                schedule.Status = "cancelled";
                            }

                            schedule.UpdatedAt = DateTime.UtcNow;
                            await dunningRepo.UpdateAsync(schedule);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing dunning schedule {ScheduleId}", schedule.Id);
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in dunning processor");
                }

                try { await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
