using GJApples_API_TEST.Data;
using GJApples_API_TEST.Models;
using GJApples_API_TEST.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GJApples_API_TEST.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplesController : ControllerBase
{
    private GJApplesDbContext _dbContext;

    public ApplesController(GJApplesDbContext context)
    {
        _dbContext = context;
    }

    // Get all AppleVarieties
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        bool isEmployee = User.IsInRole("Admin") || User.IsInRole("OrderPicker") || User.IsInRole("Harvester");

        return Ok(_dbContext
            .AppleVarieties
                .Include(a => a.Trees)
                    .ThenInclude(t => t.TreeHarvestReports)
                .Include(a => a.OrderItems)
            .Select(a => new AppleVarietyDTO
            {
                Id = a.Id,
                Type = a.Type,
                ImageUrl = a.ImageUrl,
                CostPerPound = a.CostPerPound,
                IsActive = a.IsActive,
                Trees = isEmployee ? a.Trees
                    .Select(t => new TreeDTO
                    {
                        Id = t.Id,
                        AppleVarietyId = t.AppleVarietyId,
                        AppleVariety = null,
                        DatePlanted = t.DatePlanted,
                        DateRemoved = t.DateRemoved,
                        TreeHarvestReports = t.TreeHarvestReports.Select(thr => new TreeHarvestReportDTO
                        {
                            Id = thr.Id,
                            TreeId = thr.TreeId,
                            Tree = null,
                            EmployeeUserProfileId = thr.EmployeeUserProfileId,
                            Employee = null,
                            HarvestDate = thr.HarvestDate,
                            PoundsHarvested = thr.PoundsHarvested
                        }).ToList()
                    }).ToList() : null,
                OrderItems = isEmployee ? a.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    AppleVarietyId = oi.AppleVarietyId,
                    AppleVariety = null,
                    Pounds = oi.Pounds
                }).ToList() : null
            }).ToList()
        );
    }

    // Get AppleVariety by Id
    [HttpGet("{id}")]
    [Authorize]
    public IActionResult Get(int id)
    {
        var appleVariety = _dbContext
            .AppleVarieties
                .Include(a => a.Trees)
                    .ThenInclude(t => t.TreeHarvestReports)
                .Include(a => a.OrderItems)
            .SingleOrDefault(a => a.Id == id);

        if (appleVariety == null)
        {
            return NotFound();
        }

        return Ok(new AppleVarietyDTO
        {
            Id = appleVariety.Id,
            Type = appleVariety.Type,
            ImageUrl = appleVariety.ImageUrl,
            CostPerPound = appleVariety.CostPerPound,
            IsActive = appleVariety.IsActive,
            Trees = appleVariety.Trees
                .Where(t => t.DateRemoved == null)
                .Select(t => new TreeDTO
                {
                    Id = t.Id,
                    AppleVarietyId = t.AppleVarietyId,
                    AppleVariety = null,
                    DatePlanted = t.DatePlanted,
                    DateRemoved = t.DateRemoved,
                    TreeHarvestReports = t.TreeHarvestReports.Select(thr => new TreeHarvestReportDTO
                    {
                        Id = thr.Id,
                        TreeId = thr.TreeId,
                        Tree = null,
                        EmployeeUserProfileId = thr.EmployeeUserProfileId,
                        Employee = null,
                        HarvestDate = thr.HarvestDate,
                        PoundsHarvested = thr.PoundsHarvested
                    }).ToList()
                }).ToList(),
            OrderItems = appleVariety.OrderItems.Select(oi => new OrderItemDTO
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                AppleVarietyId = oi.AppleVarietyId,
                AppleVariety = null,
                Pounds = oi.Pounds
            }).ToList()
        });
    }

    // Create New AppleVariety
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateNewAppleVariety(AppleVariety appleVariety)
    {
        var matchingAppleVariety = _dbContext
            .AppleVarieties
            .SingleOrDefault(a => a.Type.ToLower() == appleVariety.Type.ToLower().Trim());

        if (matchingAppleVariety != null)
        {
            return BadRequest();
        }

        if (appleVariety.Type == null && appleVariety.CostPerPound == 0)
        {
            return BadRequest();
        }

        appleVariety.IsActive = true;
        _dbContext.AppleVarieties.Add(appleVariety);
        _dbContext.SaveChanges();

        return Created($"/api/apple/{appleVariety.Id}", appleVariety);
    }

    // Edit AppleVariety
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult EditAppleVariety(AppleVariety appleVariety, int id)
    {
        var appleVarietyToUpdate = _dbContext
            .AppleVarieties
            .SingleOrDefault(a => a.Id == id);

        if (appleVariety == null)
        {
            return NotFound();
        }

        bool isUpdated = false;

        // Update Type
        if (!string.IsNullOrWhiteSpace(appleVariety.Type) && appleVariety.Type != appleVarietyToUpdate.Type)
        {
            appleVarietyToUpdate.Type = appleVariety.Type.Trim();
            isUpdated = true;
        }
        // Update ImageUrl
        if (!string.IsNullOrWhiteSpace(appleVariety.ImageUrl) && appleVariety.ImageUrl != appleVarietyToUpdate.ImageUrl)
        {
            appleVarietyToUpdate.ImageUrl = appleVariety.ImageUrl.Trim();
            isUpdated = true;
        }
        // Update CostPerPound
        if (appleVariety.CostPerPound != 0 && appleVariety.CostPerPound != appleVarietyToUpdate.CostPerPound)
        {
            appleVarietyToUpdate.CostPerPound = appleVariety.CostPerPound;
            isUpdated = true;
        }
        // Update IsActive
        if (appleVariety.IsActive != appleVarietyToUpdate.IsActive)
        {
            appleVarietyToUpdate.IsActive = appleVariety.IsActive;
            isUpdated = true;
        }
        // Save Changes
        if (isUpdated)
        {
            _dbContext.SaveChanges();
            return Ok(appleVarietyToUpdate);
        }
        // Cancel Changes (if everything matches)
        else
        {
            return NoContent();
        }
    }

    // Deactivate AppleVariety
    [HttpPut("{id}/changestatus")]
    [Authorize(Roles = "Admin")]
    public IActionResult EditAppleVarietyActiveStatus(int id)
    {
        var appleVarietyToUpdate = _dbContext
            .AppleVarieties
            .SingleOrDefault(a => a.Id == id);

        if (appleVarietyToUpdate == null)
        {
            return NotFound();
        }

        appleVarietyToUpdate.IsActive = !appleVarietyToUpdate.IsActive;
        _dbContext.SaveChanges();
        return NoContent();
    }

    // Delete AppleVariety
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteAppleVariety(int id)
    {
        var appleVarietyToDelete = _dbContext
            .AppleVarieties
            .SingleOrDefault(a => a.Id == id);

        if (appleVarietyToDelete == null)
        {
            return NotFound();
        }

        _dbContext.Remove(appleVarietyToDelete);
        _dbContext.SaveChanges();

        return NoContent();
    }
}