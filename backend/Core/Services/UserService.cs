using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Core.Services
{
    public class UserService : BaseService, IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IEncryptionService _encryption;

        public UserService(ITenantContextProvider tcp, IUserRepository userRepo, IEncryptionService encryption) : base(tcp)
        {
            _userRepo = userRepo;
            _encryption = encryption;
        }

        public async Task<GatewayResponseWrapper<UserResponseDto>> CreateAsync(CreateUserDto request)
        {
            var response = new GatewayResponseWrapper<UserResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var existing = await _userRepo.GetByEmailAsync(tenantId, request.Email);
            if (existing != null) { response.SetError("User with this email already exists."); return response; }

            var user = new User
            {
                TenantId = tenantId, Email = request.Email, FirstName = request.FirstName,
                LastName = request.LastName, Role = request.Role ?? "member", IsActive = true,
                Permissions = request.Permissions != null ? JsonConvert.SerializeObject(request.Permissions) : null,
                Metadata = request.Metadata != null ? JsonConvert.SerializeObject(request.Metadata) : null
            };

            await _userRepo.CreateAsync(user);
            response.SetSuccess(MapUser(user));
            return response;
        }

        public async Task<GatewayResponseWrapper<UserResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<UserResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var user = await _userRepo.Query(tenantId).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) { response.SetError("User not found."); return response; }
            response.SetSuccess(MapUser(user));
            return response;
        }

        public async Task<GatewayResponseWrapper<UserResponseDto>> GetByEmailAsync(string email)
        {
            var response = new GatewayResponseWrapper<UserResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var user = await _userRepo.GetByEmailAsync(tenantId, email);
            if (user == null) { response.SetError("User not found."); return response; }
            response.SetSuccess(MapUser(user));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<UserResponseDto>> ListAsync(UserFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<UserResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _userRepo.Query(tenantId);

            if (!string.IsNullOrEmpty(filter.Search))
                query = query.Where(u => u.Email.Contains(filter.Search) || u.FirstName.Contains(filter.Search) || u.LastName.Contains(filter.Search));
            if (!string.IsNullOrEmpty(filter.Role)) query = query.Where(u => u.Role == filter.Role);
            if (filter.IsActive.HasValue) query = query.Where(u => u.IsActive == filter.IsActive.Value);

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(u => u.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            response.SetSuccessWithPagination(items.Select(MapUser).ToList(), total, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<UserResponseDto>> UpdateAsync(Guid id, UpdateUserDto request)
        {
            var response = new GatewayResponseWrapper<UserResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var user = await _userRepo.Query(tenantId).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) { response.SetError("User not found."); return response; }

            if (request.Email != null && request.Email != user.Email)
            {
                var existing = await _userRepo.GetByEmailAsync(tenantId, request.Email);
                if (existing != null) { response.SetError("Email already in use."); return response; }
                user.Email = request.Email;
            }

            if (request.FirstName != null) user.FirstName = request.FirstName;
            if (request.LastName != null) user.LastName = request.LastName;
            if (request.Role != null) user.Role = request.Role;
            if (request.Permissions != null) user.Permissions = JsonConvert.SerializeObject(request.Permissions);
            if (request.Metadata != null) user.Metadata = JsonConvert.SerializeObject(request.Metadata);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);
            response.SetSuccess(MapUser(user));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> UpdateRoleAsync(Guid id, string role)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var user = await _userRepo.Query(tenantId).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) { response.SetError("User not found."); return response; }
            var validRoles = new[] { "admin", "member", "viewer" };
            if (!validRoles.Contains(role)) { response.SetError("Invalid role. Valid roles are: admin, member, viewer."); return response; }
            user.Role = role; user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);
            response.SetSuccess(true, $"User role updated to {role}.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DeactivateAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var user = await _userRepo.Query(tenantId).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) { response.SetError("User not found."); return response; }
            user.IsActive = false; user.DeactivatedAt = DateTime.UtcNow; user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);
            response.SetSuccess(true, "User deactivated.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ActivateAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var user = await _userRepo.Query(tenantId).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) { response.SetError("User not found."); return response; }
            user.IsActive = true; user.DeactivatedAt = null; user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);
            response.SetSuccess(true, "User activated.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> UpdatePermissionsAsync(Guid id, List<string> permissions)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var user = await _userRepo.Query(tenantId).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) { response.SetError("User not found."); return response; }
            user.Permissions = JsonConvert.SerializeObject(permissions); user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);
            response.SetSuccess(true, "Permissions updated.");
            return response;
        }

        private static UserResponseDto MapUser(User u) => new()
        {
            Id = u.Id, Email = u.Email, FirstName = u.FirstName, LastName = u.LastName,
            FullName = $"{u.FirstName} {u.LastName}".Trim(), Role = u.Role, IsActive = u.IsActive,
            LastLoginAt = u.LastLoginAt, CreatedAt = u.CreatedAt,
            Permissions = !string.IsNullOrEmpty(u.Permissions) ? JsonConvert.DeserializeObject<List<string>>(u.Permissions) : new()
        };
    }
}
