namespace Core.ServiceContracts;

public interface IBaseService
{
    Task<bool> HealthCheckAsync();
}
