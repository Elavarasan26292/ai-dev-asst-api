using Microsoft.EntityFrameworkCore;
using ai_dev_asst_api.Data;
using ai_dev_asst_api.DTOs;
using ai_dev_asst_api.Models;

namespace ai_dev_asst_api.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _db;

    public RoleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, int? createdByUserId = null)
    {
        var exists = await _db.Roles.AnyAsync(r => r.RoleName == request.RoleName && !r.IsDeleted);
        if (exists)
            throw new ArgumentException($"Role '{request.RoleName}' already exists.");

        var validPermissions = await _db.Permissions
            .Where(p => request.PermissionIds.Contains(p.PermissionId))
            .ToListAsync();

        if (validPermissions.Count != request.PermissionIds.Count)
            throw new ArgumentException("One or more permission IDs are invalid.");

        var role = new Role
        {
            RoleName = request.RoleName,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            RolePermissions = request.PermissionIds.Select(pid => new RolePermission
            {
                PermissionId = pid
            }).ToList()
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return new RoleResponse
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Permissions = validPermissions.Select(p => new PermissionResponse
            {
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName
            }).ToList()
        };
    }

    public async Task<RoleResponse> UpdateRoleAsync(int roleId, UpdateRoleRequest request)
    {
        var role = await _db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.RoleId == roleId && !r.IsDeleted);

        if (role == null)
            throw new ArgumentException($"Role with ID {roleId} does not exist.");

        var nameConflict = await _db.Roles.AnyAsync(r => r.RoleName == request.RoleName && r.RoleId != roleId && !r.IsDeleted);
        if (nameConflict)
            throw new ArgumentException($"Role '{request.RoleName}' already exists.");

        var validPermissions = await _db.Permissions
            .Where(p => request.PermissionIds.Contains(p.PermissionId))
            .ToListAsync();

        if (validPermissions.Count != request.PermissionIds.Count)
            throw new ArgumentException("One or more permission IDs are invalid.");

        role.RoleName = request.RoleName;
        role.LastUpdatedAt = DateTime.UtcNow;

        _db.RolePermissions.RemoveRange(role.RolePermissions);
        role.RolePermissions = request.PermissionIds.Select(pid => new RolePermission
        {
            RoleId = roleId,
            PermissionId = pid
        }).ToList();

        await _db.SaveChangesAsync();

        return new RoleResponse
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Permissions = validPermissions.Select(p => new PermissionResponse
            {
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName
            }).ToList()
        };
    }

    public async Task DeleteRoleAsync(int roleId)
    {
        var role = await _db.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.RoleId == roleId && !r.IsDeleted);

        if (role == null)
            throw new ArgumentException($"Role with ID {roleId} does not exist.");

        if (role.RoleName == "Admin")
            throw new ArgumentException("The Admin role cannot be deleted.");

        var activeUsersCount = role.Users.Count(u => !u.IsDeleted);
        if (activeUsersCount > 0)
            throw new ArgumentException($"Cannot delete role '{role.RoleName}' because it is assigned to {activeUsersCount} user(s).");

        role.IsDeleted = true;
        role.LastUpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<List<RoleResponse>> GetAllRolesAsync()
    {
        return await _db.Roles
            .Where(r => !r.IsDeleted)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Select(r => new RoleResponse
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Permissions = r.RolePermissions.Select(rp => new PermissionResponse
                {
                    PermissionId = rp.Permission.PermissionId,
                    PermissionName = rp.Permission.PermissionName
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<List<PermissionResponse>> GetAllPermissionsAsync()
    {
        return await _db.Permissions
            .Select(p => new PermissionResponse
            {
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName
            })
            .ToListAsync();
    }
}
