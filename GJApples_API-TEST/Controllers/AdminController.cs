using GJApples.Data;
using GJApples_API_TEST.Models;
using GJApples_API_TEST.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GJApples_API_TEST.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private GJApplesDbContext _dbContext;

    public AdminController(GJApplesDbContext context)
    {
        _dbContext = context;
    }

    // Get all Admin Profiles
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Get()
    {
        return Ok(_dbContext
            .UserProfiles
            .Include(u => u.IdentityUser)
            .Where(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.IdentityUserId &&
                       _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin")))
            .Select(harvester => new AdminDTO
            {
                Id = harvester.Id,
                FirstName = harvester.FirstName,
                LastName = harvester.LastName,
                Address = harvester.Address,
                Email = harvester.IdentityUser.Email,
                IdentityUserId = harvester.IdentityUserId,
            }).ToList());
    }

    // Get Admin Profile by Id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetCustomerById(int id)
    {
        var admin = _dbContext
            .UserProfiles
            .Include(u => u.IdentityUser)
            .Where(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.IdentityUserId &&
                       _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin")))
            .SingleOrDefault(c => c.Id == id);

        if (admin == null)
        {
            return NotFound();
        }

        return Ok(new AdminDTO
        {
            Id = admin.Id,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            Address = admin.Address,
            Email = admin.IdentityUser.Email,
        });
    }

}