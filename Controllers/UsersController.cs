using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ai_dev_asst_api.DTOs;
using ai_dev_asst_api.Filters;
using ai_dev_asst_api.Models;
using ai_dev_asst_api.Services;

namespace ai_dev_asst_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [PermissionAuthorize(PermissionIds.AccessUser)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _userService.CreateUserAsync(request, currentUserId);
        return CreatedAtAction(nameof(GetUsers), null, result);
    }

    [HttpGet]
    [PermissionAuthorize(PermissionIds.AccessUser)]
    public async Task<IActionResult> GetUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return Ok(result);
    }

    [HttpPut("{id}")]
    [PermissionAuthorize(PermissionIds.AccessUser)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [PermissionAuthorize(PermissionIds.AccessUser)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(new { message = "User deleted successfully." });
    }

    [HttpPut("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        // Users can only change their own password
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != id.ToString())
            return Forbid();

        await _userService.ChangePasswordAsync(id, request);
        return Ok(new { message = "Password changed successfully." });
    }
}
