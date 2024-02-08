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
public class HarvestersController : ControllerBase
{
    private GJApplesDbContext _dbContext;

    public HarvestersController(GJApplesDbContext context)
    {
        _dbContext = context;
    }

    // Get all Harvester Profiles
    [HttpGet]
    [Authorize(Roles = "Admin,Harvester")]
    public IActionResult Get()
    {
        return Ok(_dbContext
            .UserProfiles
            .Include(u => u.IdentityUser)
            .Include(u => u.TreeHarvestReports)
                .ThenInclude(thr => thr.Tree)
                    .ThenInclude(t => t.AppleVariety)
            .Where(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.IdentityUserId &&
                       _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Harvester")))
            .Select(harvester => new HarvesterDTO
            {
                Id = harvester.Id,
                FirstName = harvester.FirstName,
                LastName = harvester.LastName,
                Address = harvester.Address,
                Email = harvester.IdentityUser.Email,
                IdentityUserId = harvester.IdentityUserId,
                TreeHarvestReports = harvester.TreeHarvestReports.Select(thr => new TreeHarvestReportDTO
                {
                    Id = thr.Id,
                    TreeId = thr.TreeId,
                    Tree = new TreeDTO
                    {
                        Id = thr.Tree.Id,
                        AppleVarietyId = thr.Tree.AppleVarietyId,
                        AppleVariety = new AppleVarietyDTO
                        {
                            Id = thr.Tree.AppleVariety.Id,
                            Type = thr.Tree.AppleVariety.Type,
                            ImageUrl = thr.Tree.AppleVariety.ImageUrl,
                            CostPerPound = thr.Tree.AppleVariety.CostPerPound,
                            IsActive = thr.Tree.AppleVariety.IsActive,
                            Trees = null,
                            OrderItems = null
                        },
                        DatePlanted = thr.Tree.DatePlanted,
                        DateRemoved = thr.Tree.DateRemoved,
                        TreeHarvestReports = null
                    },
                    EmployeeUserProfileId = thr.EmployeeUserProfileId,
                    Employee = null,
                    HarvestDate = thr.HarvestDate,
                    PoundsHarvested = thr.PoundsHarvested
                }).ToList()
            }).ToList());
    }

    // Get Harvester Profile by Id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Harvester")]
    public IActionResult GetCustomerById(int id)
    {
        var harvester = _dbContext
            .UserProfiles
            .Include(u => u.IdentityUser)
            .Include(u => u.TreeHarvestReports)
                .ThenInclude(thr => thr.Tree)
                    .ThenInclude(t => t.AppleVariety)
            .Where(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.IdentityUserId &&
                       _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Harvester")))
            .SingleOrDefault(c => c.Id == id);

        if (harvester == null)
        {
            return NotFound();
        }

        return Ok(new HarvesterDTO
        {
            Id = harvester.Id,
            FirstName = harvester.FirstName,
            LastName = harvester.LastName,
            Address = harvester.Address,
            Email = harvester.IdentityUser.Email,
            TreeHarvestReports = harvester.TreeHarvestReports.Select(thr => new TreeHarvestReportDTO
            {
                Id = thr.Id,
                TreeId = thr.TreeId,
                Tree = new TreeDTO
                {
                    Id = thr.Tree.Id,
                    AppleVarietyId = thr.Tree.AppleVarietyId,
                    AppleVariety = new AppleVarietyDTO
                    {
                        Id = thr.Tree.AppleVariety.Id,
                        Type = thr.Tree.AppleVariety.Type,
                        ImageUrl = thr.Tree.AppleVariety.ImageUrl,
                        CostPerPound = thr.Tree.AppleVariety.CostPerPound,
                        IsActive = thr.Tree.AppleVariety.IsActive,
                        Trees = null,
                        OrderItems = null
                    },
                    DatePlanted = thr.Tree.DatePlanted,
                    DateRemoved = thr.Tree.DateRemoved,
                    TreeHarvestReports = null
                },
                EmployeeUserProfileId = thr.EmployeeUserProfileId,
                Employee = null,
                HarvestDate = thr.HarvestDate,
                PoundsHarvested = thr.PoundsHarvested
            }).ToList()
        });
    }

}