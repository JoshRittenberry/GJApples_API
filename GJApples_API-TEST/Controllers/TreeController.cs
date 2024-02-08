using GJApples.Data;
using GJApples_API_TEST.Models;
using GJApples_API_TEST.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GJApples_API_TEST.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TreesController : ControllerBase
{
    private GJApplesDbContext _dbContext;

    public TreesController(GJApplesDbContext context)
    {
        _dbContext = context;
    }

    // Get all Trees
    [HttpGet]
    [Authorize(Roles = "Admin,Harvester")]
    public IActionResult Get([FromQuery] bool? needsHarvested)
    {
        bool isAuthorized = User.Identity.IsAuthenticated && User.IsInRole("Admin");

        DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);

        return Ok(_dbContext
            .Trees
            .Include(t => t.AppleVariety)
            .Include(t => t.TreeHarvestReports)
                .ThenInclude(thr => thr.Employee)
                    .ThenInclude(e => e.IdentityUser)
            .Where(t =>
                (needsHarvested == true &&
                t.DateRemoved == null &&
                (t.TreeHarvestReports.Any(thr => thr.HarvestDate <= sevenDaysAgo && thr.Id == t.TreeHarvestReports.Max(th => th.Id)) ||
                t.TreeHarvestReports.Max(th => th.Id) == 0)) ||
                (needsHarvested != true))
            .Select(t => new TreeDTO
            {
                Id = t.Id,
                AppleVarietyId = t.AppleVarietyId,
                AppleVariety = new AppleVarietyDTO
                {
                    Id = t.AppleVariety.Id,
                    Type = t.AppleVariety.Type,
                    ImageUrl = t.AppleVariety.ImageUrl,
                    CostPerPound = t.AppleVariety.CostPerPound,
                    IsActive = t.AppleVariety.IsActive,
                    Trees = null,
                    OrderItems = null
                },
                DatePlanted = t.DatePlanted,
                DateRemoved = t.DateRemoved,
                TreeHarvestReports = t.TreeHarvestReports.Select(thr => new TreeHarvestReportDTO
                {
                    Id = thr.Id,
                    TreeId = thr.TreeId,
                    Tree = null,
                    EmployeeUserProfileId = thr.EmployeeUserProfileId,
                    Employee = new HarvesterDTO
                    {
                        Id = thr.Employee.Id,
                        FirstName = thr.Employee.FirstName,
                        LastName = thr.Employee.LastName,
                        Address = isAuthorized ? thr.Employee.Address : null,
                        Email = isAuthorized ? thr.Employee.IdentityUser.Email : null,
                        TreeHarvestReports = null
                    },
                    HarvestDate = thr.HarvestDate,
                    PoundsHarvested = thr.PoundsHarvested
                }).ToList()
            }).ToList()
        );
    }

    // Get Tree by Id
    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetTreeById(int id)
    {
        var tree = _dbContext
            .Trees
            .Include(t => t.AppleVariety)
            .Include(t => t.TreeHarvestReports)
                .ThenInclude(thr => thr.Employee)
                .ThenInclude(e => e.IdentityUser)
            .SingleOrDefault(t => t.Id == id);

        if (tree == null)
        {
            return NotFound();
        }

        return Ok(new TreeDTO
        {
            Id = tree.Id,
            AppleVarietyId = tree.AppleVarietyId,
            AppleVariety = new AppleVarietyDTO
            {
                Id = tree.AppleVariety.Id,
                Type = tree.AppleVariety.Type,
                ImageUrl = tree.AppleVariety.ImageUrl,
                CostPerPound = tree.AppleVariety.CostPerPound,
                IsActive = tree.AppleVariety.IsActive,
                Trees = null,
                OrderItems = null
            },
            DatePlanted = tree.DatePlanted,
            DateRemoved = tree.DateRemoved,
            TreeHarvestReports = tree.TreeHarvestReports.Select(thr => new TreeHarvestReportDTO
            {
                Id = thr.Id,
                TreeId = thr.TreeId,
                Tree = null,
                EmployeeUserProfileId = thr.EmployeeUserProfileId,
                Employee = new HarvesterDTO
                {
                    Id = thr.Employee.Id,
                    FirstName = thr.Employee.FirstName,
                    LastName = thr.Employee.LastName,
                    Address = thr.Employee.Address,
                    Email = thr.Employee.IdentityUser.Email,
                    TreeHarvestReports = null
                },
                HarvestDate = thr.HarvestDate,
                PoundsHarvested = thr.PoundsHarvested
            }).ToList()
        });
    }

    // Get TreeHarvestReports
    [HttpGet("harvestreports")]
    [Authorize(Roles = "Admin,Harvester")]
    public IActionResult GetTreeHarvestReports([FromQuery] int? treeId)
    {
        var query = _dbContext.TreeHarvestReports
        .Include(thr => thr.Tree)
            .ThenInclude(t => t.AppleVariety)
        .Include(thr => thr.Employee)
            .ThenInclude(e => e.IdentityUser)
        .AsQueryable(); // Create a base query

        if (treeId.HasValue)
        {
            query = query.Where(thr => thr.Tree.Id == treeId.Value);

            if (query == null)
            {
                return BadRequest();
            }
        }

        var treeHarvestReports = query.Select(thr => new TreeHarvestReportDTO
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
            Employee = new HarvesterDTO
            {
                Id = thr.Employee.Id,
                FirstName = thr.Employee.FirstName,
                LastName = thr.Employee.LastName,
                Address = thr.Employee.Address,
                Email = thr.Employee.IdentityUser.Email,
                TreeHarvestReports = null
            },
            HarvestDate = thr.HarvestDate,
            PoundsHarvested = thr.PoundsHarvested
        }).ToList();

        return Ok(treeHarvestReports);
    }

    // Get TreeHarvestReport by Id
    [HttpGet("harvestreports/{id}")]
    [Authorize(Roles = "Admin,Harvester")]
    public IActionResult GetTreeHarvestReportById(int id)
    {
        var treeHarvestReport = _dbContext
            .TreeHarvestReports
                .Include(thr => thr.Tree)
                    .ThenInclude(t => t.AppleVariety)
                .Include(thr => thr.Employee)
                    .ThenInclude(e => e.IdentityUser)
            .SingleOrDefault(thr => thr.Id == id);

        if (treeHarvestReport == null)
        {
            return NotFound();
        }

        return Ok(new TreeHarvestReportDTO
        {
            Id = treeHarvestReport.Id,
            TreeId = treeHarvestReport.TreeId,
            Tree = new TreeDTO
            {
                Id = treeHarvestReport.Tree.Id,
                AppleVarietyId = treeHarvestReport.Tree.AppleVarietyId,
                AppleVariety = new AppleVarietyDTO
                {
                    Id = treeHarvestReport.Tree.AppleVariety.Id,
                    Type = treeHarvestReport.Tree.AppleVariety.Type,
                    ImageUrl = treeHarvestReport.Tree.AppleVariety.ImageUrl,
                    CostPerPound = treeHarvestReport.Tree.AppleVariety.CostPerPound,
                    IsActive = treeHarvestReport.Tree.AppleVariety.IsActive,
                    Trees = null,
                    OrderItems = null
                },
                DatePlanted = treeHarvestReport.Tree.DatePlanted,
                DateRemoved = treeHarvestReport.Tree.DateRemoved,
                TreeHarvestReports = null
            },
            EmployeeUserProfileId = treeHarvestReport.EmployeeUserProfileId,
            Employee = new HarvesterDTO
            {
                Id = treeHarvestReport.Employee.Id,
                FirstName = treeHarvestReport.Employee.FirstName,
                LastName = treeHarvestReport.Employee.LastName,
                Address = treeHarvestReport.Employee.Address,
                Email = treeHarvestReport.Employee.IdentityUser.Email,
                TreeHarvestReports = null
            },
            HarvestDate = treeHarvestReport.HarvestDate,
            PoundsHarvested = treeHarvestReport.PoundsHarvested
        });
    }

    // Get Harvester's Assigned TreeHarvestReport
    [HttpGet("assignment")]
    [Authorize(Roles = "Admin,Harvester")]
    public IActionResult GetHarvesterAssignment()
    {
        // Find Harvester's UserName
        var employeeUserName = User.Identity.Name;

        // Find Harvester's UserProfile
        UserProfile harvester = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == employeeUserName);

        var treeHarvestReport = _dbContext
            .TreeHarvestReports
            .Include(thr => thr.Tree)
                .ThenInclude(t => t.AppleVariety)
            .Where(thr => thr.EmployeeUserProfileId == harvester.Id)
            .OrderByDescending(thr => thr.Id)
            .FirstOrDefault();

        if (harvester == null || treeHarvestReport == null || treeHarvestReport.HarvestDate != null)
        {
            return NotFound();
        }


        return Ok(new TreeHarvestReportDTO
        {
            Id = treeHarvestReport.Id,
            TreeId = treeHarvestReport.TreeId,
            Tree = new TreeDTO
            {
                Id = treeHarvestReport.Tree.Id,
                AppleVarietyId = treeHarvestReport.Tree.AppleVarietyId,
                AppleVariety = new AppleVarietyDTO
                {
                    Id = treeHarvestReport.Tree.AppleVariety.Id,
                    Type = treeHarvestReport.Tree.AppleVariety.Type,
                    ImageUrl = treeHarvestReport.Tree.AppleVariety.ImageUrl,
                    CostPerPound = treeHarvestReport.Tree.AppleVariety.CostPerPound,
                    IsActive = treeHarvestReport.Tree.AppleVariety.IsActive,
                    Trees = null,
                    OrderItems = null
                },
                DatePlanted = treeHarvestReport.Tree.DatePlanted,
                DateRemoved = treeHarvestReport.Tree.DateRemoved,
                TreeHarvestReports = null
            },
            EmployeeUserProfileId = treeHarvestReport.EmployeeUserProfileId,
            Employee = null,
            HarvestDate = treeHarvestReport.HarvestDate,
            PoundsHarvested = treeHarvestReport.PoundsHarvested
        });
    }

    // Create new Tree
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateNewTree(Tree tree)
    {
        if (tree.DatePlanted == null || tree.DatePlanted == DateTime.MinValue || tree.AppleVarietyId == null)
        {
            tree.DatePlanted = DateTime.Now;
        }

        _dbContext.Trees.Add(tree);
        _dbContext.SaveChanges();

        return Created($"/api/trees/{tree.Id}", tree);
    }

    // Create new TreeHarvestReport
    [HttpPost("harvestreports")]
    [Authorize(Roles = "Admin,Harvester")]
    public IActionResult CreateHarvestReport(TreeHarvestReport treeHarvestReport)
    {
        var tree = _dbContext.Trees.SingleOrDefault(t => t.Id == treeHarvestReport.TreeId);
        var employee = _dbContext.Trees.SingleOrDefault(u => u.Id == treeHarvestReport.EmployeeUserProfileId);

        // if (tree == null || employee == null || treeHarvestReport.HarvestDate == null || treeHarvestReport.HarvestDate == DateTime.MinValue || treeHarvestReport.PoundsHarvested == null || treeHarvestReport.PoundsHarvested < 0)
        if (tree == null || employee == null)
        {
            return BadRequest();
        }

        _dbContext.TreeHarvestReports.Add(treeHarvestReport);
        _dbContext.SaveChanges();

        return Created($"/api/treeHarvestReports/{treeHarvestReport.Id}", treeHarvestReport);
    }

    // Edit Tree
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult EditTree(Tree tree, int id)
    {
        var treeToUpdate = _dbContext
            .Trees
            .SingleOrDefault(t => t.Id == id);

        if (treeToUpdate == null)
        {
            return NotFound();
        }

        bool isUpdated = false;

        // Update AppleVarietyId
        if (tree.AppleVarietyId != null && tree.AppleVarietyId != treeToUpdate.AppleVarietyId)
        {
            treeToUpdate.AppleVarietyId = tree.AppleVarietyId;
            isUpdated = true;
        }
        // Update DatePlanted
        if (tree.DatePlanted != null && tree.DatePlanted != treeToUpdate.DatePlanted)
        {
            if (tree.DatePlanted == DateTime.MinValue)
            {
                treeToUpdate.DatePlanted = treeToUpdate.DatePlanted;
            }
            else
            {
                treeToUpdate.DatePlanted = tree.DatePlanted;
                isUpdated = true;
            }
        }
        // Update DateRemoved
        if (tree.DateRemoved != treeToUpdate.DateRemoved)
        {
            if (tree.DatePlanted == DateTime.MinValue || tree.DatePlanted == null)
            {
                treeToUpdate.DateRemoved = null;
            }
            else
            {
                treeToUpdate.DateRemoved = tree.DateRemoved;
                isUpdated = true;
            }
        }
        // Save Changes
        if (isUpdated)
        {
            _dbContext.SaveChanges();
            return Ok(treeToUpdate);
        }
        // Cancel Changes (if no changes were made)
        else
        {
            return NoContent();
        }
    }

    // Edit Harvest Report
    [HttpPut("harvestreports/{id}")]
    [Authorize(Roles = "Admin,Harvester")]
    public IActionResult EditHarvestReport(TreeHarvestReport treeHarvestReport, int id)
    {
        // Find TreeHarvestReport
        var treeHarvestReportToUpdate = _dbContext
            .TreeHarvestReports
            .SingleOrDefault(thr => thr.Id == id);

        // Find Username
        var employeeUserName = User.Identity.Name;

        // Find UserProfile
        UserProfile employee = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == employeeUserName);

        // Check if the User is an Admin
        bool isUserAdmin = User.IsInRole("Admin");

        // If the TreeHarvestReport or User/Employee doesn't exist cancel changes
        if (treeHarvestReportToUpdate == null || employee == null)
        {
            return NotFound();
        }

        // Causing an error
        // If the User/Employee isn't the creator of the report, or an Admin, forbid them from making changes
        // if (employee.Id != treeHarvestReportToUpdate.EmployeeUserProfileId || !isUserAdmin)
        // {
        //     return BadRequest();
        // }

        // If the User/Employee is the creator of the report, or is an Admin, allow changes to be made
        if (employee.Id == treeHarvestReportToUpdate.EmployeeUserProfileId || isUserAdmin)
        {
            bool isUpdated = false;

            // Update EmployeeId only if the user is an Admin
            if (isUserAdmin && treeHarvestReport.EmployeeUserProfileId != null && treeHarvestReport.EmployeeUserProfileId != treeHarvestReportToUpdate.EmployeeUserProfileId)
            {
                treeHarvestReportToUpdate.EmployeeUserProfileId = treeHarvestReport.EmployeeUserProfileId;
                isUpdated = true;
            }
            // Update HarvestDate
            if (treeHarvestReport.HarvestDate != null && treeHarvestReport.HarvestDate != treeHarvestReportToUpdate.HarvestDate)
            {
                if (treeHarvestReport.HarvestDate == DateTime.MinValue)
                {
                    treeHarvestReportToUpdate.HarvestDate = treeHarvestReportToUpdate.HarvestDate;
                }
                else
                {
                    treeHarvestReportToUpdate.HarvestDate = treeHarvestReport.HarvestDate;
                    isUpdated = true;
                }
            }
            // Update PoundsHarvested
            if (treeHarvestReport.PoundsHarvested != null && treeHarvestReport.PoundsHarvested != treeHarvestReportToUpdate.PoundsHarvested)
            {
                treeHarvestReportToUpdate.PoundsHarvested = treeHarvestReport.PoundsHarvested;
                isUpdated = true;
            }
            // Save Changes
            if (isUpdated)
            {
                _dbContext.SaveChanges();
                return Ok(treeHarvestReportToUpdate);
            }
            // Cancel Changes (if no changes were made)
            else
            {
                return NoContent();
            }
        }

        // If no changes were made, something must have went wrong, send a BadRequest
        else
        {
            return BadRequest();
        }
    }

    // Remove Tree / Input DateRemoved
    [HttpPut("{id}/remove")]
    [Authorize(Roles = "Admin")]
    public IActionResult RemoveTree(int id)
    {
        var treeToUpdate = _dbContext
            .Trees
            .SingleOrDefault(t => t.Id == id);

        if (treeToUpdate == null)
        {
            return NotFound();
        }

        treeToUpdate.DateRemoved = DateTime.Today;
        _dbContext.SaveChanges();

        return NoContent();
    }

    // Delete TreeHarvestReport
    [HttpDelete("harvestreports/{id}")]
    [Authorize(Roles = "Admin,Harvester")]
    public IActionResult DeleteTreeHarvestReport(int id)
    {
        // Find Harvester's UserName
        var employeeUserName = User.Identity.Name;

        // Find Harvester's UserProfile
        UserProfile harvester = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == employeeUserName);

        var treeHarvestReport = _dbContext
            .TreeHarvestReports
            .SingleOrDefault(thr => thr.Id == id);

        if (harvester == null || treeHarvestReport == null)
        {
            return NotFound();
        }

        if (treeHarvestReport.EmployeeUserProfileId != harvester.Id)
        {
            return BadRequest();
        }

        _dbContext.Remove(treeHarvestReport);
        _dbContext.SaveChanges();

        return NoContent();

    }

    // Delete Tree
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteTree(int id)
    {
        var treeToUpdate = _dbContext
            .Trees
            .SingleOrDefault(t => t.Id == id);

        if (treeToUpdate == null)
        {
            return NotFound();
        }

        _dbContext.Remove(treeToUpdate);
        _dbContext.SaveChanges();

        return NoContent();
    }
}