using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface ITaxRepository
    {
        Task<TaxConfiguration?> GetConfigAsync(Guid tenantId);
        Task<Guid> CreateConfigAsync(TaxConfiguration config);
        Task UpdateConfigAsync(TaxConfiguration config);
        Task<List<TaxExemption>> GetExemptionsAsync(Guid tenantId, Guid customerId);
        Task<Guid> CreateExemptionAsync(TaxExemption exemption);
        Task DeleteExemptionAsync(Guid tenantId, Guid id);
    }
}
