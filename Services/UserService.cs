using Microsoft.EntityFrameworkCore;
using ai_dev_asst_api.Data;
using ai_dev_asst_api.DTOs;
using ai_dev_asst_api.Models;

namespace ai_dev_asst_api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public UserService(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, int? createdByUserId = null)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId && !r.IsDeleted);
        if (role == null)
            throw new ArgumentException($"Role with ID {request.RoleId} does not exist.");

        var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email && !u.IsDeleted);
        if (emailExists)
            throw new ArgumentException($"A user with email '{request.Email}' already exists.");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId,
            Theme = request.Theme,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendEmailAsync(
                    toEmail: user.Email,
                    subject: "Welcome to AI Dev Repository",
                    templateName: "WelcomeUser",
                    placeholders: new Dictionary<string, string>
                    {
                        { "FirstName", user.FirstName },
                        { "LastName", user.LastName },
                        { "Email", user.Email },
                        { "RoleName", role.RoleName }
                    }
                );
            }
            catch { }
        });

        return new UserResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = role.RoleName,
            Theme = user.Theme
        };
    }

    public async Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        if (user == null)
            throw new ArgumentException($"User with ID {userId} does not exist.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId && !r.IsDeleted);
        if (role == null)
            throw new ArgumentException($"Role with ID {request.RoleId} does not exist.");

        var emailConflict = await _db.Users.AnyAsync(u => u.Email == request.Email && u.UserId != userId && !u.IsDeleted);
        if (emailConflict)
            throw new ArgumentException($"A user with email '{request.Email}' already exists.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Phone = request.Phone;
        user.Email = request.Email;
        user.RoleId = request.RoleId;
        user.Theme = request.Theme;
        user.LastUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new UserResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = role.RoleName,
            Theme = user.Theme
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        if (user == null)
            throw new ArgumentException($"User with ID {userId} does not exist.");

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            throw new ArgumentException("Old password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.LastUpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user == null)
            throw new ArgumentException($"User with ID {userId} does not exist.");

        if (user.FirstName == "SuperAdmin")
            throw new ArgumentException("The SuperAdmin user cannot be deleted.");

        user.IsDeleted = true;
        user.LastUpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        return await _db.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.Role)
            .Select(u => new UserResponse
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Phone = u.Phone,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.Role.RoleName,
                Theme = u.Theme
            })
            .ToListAsync();
    }
}
