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
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost]
    [PermissionAuthorize(PermissionIds.CreateEditRoles)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _roleService.CreateRoleAsync(request, currentUserId);
        return CreatedAtAction(nameof(GetRoles), null, result);
    }

    [HttpPut("{id}")]
    [PermissionAuthorize(PermissionIds.CreateEditRoles)]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        var result = await _roleService.UpdateRoleAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [PermissionAuthorize(PermissionIds.CreateEditRoles)]
    public async Task<IActionResult> DeleteRole(int id)
    {
        await _roleService.DeleteRoleAsync(id);
        return Ok(new { message = "Role deleted successfully." });
    }

    [HttpGet]
    [PermissionAuthorize(PermissionIds.CreateEditRoles)]
    public async Task<IActionResult> GetRoles()
    {
        var result = await _roleService.GetAllRolesAsync();
        return Ok(result);
    }

    [HttpGet("permissions")]
    [PermissionAuthorize(PermissionIds.CreateEditRoles)]
    public async Task<IActionResult> GetPermissions()
    {
        var result = await _roleService.GetAllPermissionsAsync();
        return Ok(result);
    }
}
