using System.Text;
using GJApples_API_TEST.Data;
using GJApples_API_TEST.Models;
using GJApples_API_TEST.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GJApples_API_TEST.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserProfilesController : ControllerBase
{
    private GJApplesDbContext _dbContext;

    public UserProfilesController(GJApplesDbContext context)
    {
        _dbContext = context;
    }

    // Get all UserProfiles
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Get()
    {
        return Ok(_dbContext
            .UserProfiles
            .Include(up => up.IdentityUser)
            .Select(up => new UserProfileDTO
            {
                Id = up.Id,
                FirstName = up.FirstName,
                LastName = up.LastName,
                Address = up.Address,
                IdentityUserId = up.IdentityUserId,
                Email = up.IdentityUser.Email,
                ForcePasswordChange = up.ForcePasswordChange,
                UserName = up.IdentityUser.UserName
            })
            .ToList());
    }

    // Get all Roles
    [HttpGet("roles")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetRoles()
    {
        return Ok(_dbContext
            .Roles
            .Select(r => new IdentityRoleDTO
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToList()
        );
    }

    // Get UserProfile by Id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Get(int id)
    {
        var foundUP = _dbContext
            .UserProfiles
            .Include(up => up.IdentityUser)
            .SingleOrDefault(up => up.Id == id);

        if (foundUP == null)
        {
            return NotFound();
        }

        return Ok(new UserProfileDTO
        {
            Id = foundUP.Id,
            FirstName = foundUP.FirstName,
            LastName = foundUP.LastName,
            Address = foundUP.Address,
            Email = foundUP.IdentityUser.Email,
            UserName = foundUP.IdentityUser.UserName,
            ForcePasswordChange = foundUP.ForcePasswordChange,
            IdentityUserId = foundUP.IdentityUserId,
            IdentityUser = foundUP.IdentityUser
        });
    }

    // Get UserProfiles with Roles
    [HttpGet("withroles")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetWithRoles()
    {
        return Ok(_dbContext.UserProfiles
        .Include(up => up.IdentityUser)
        .Select(up => new UserProfileDTO
        {
            Id = up.Id,
            FirstName = up.FirstName,
            LastName = up.LastName,
            Address = up.Address,
            Email = up.IdentityUser.Email,
            ForcePasswordChange = up.ForcePasswordChange,
            UserName = up.IdentityUser.UserName,
            IdentityUserId = up.IdentityUserId,
            Roles = _dbContext.UserRoles
            .Where(ur => ur.UserId == up.IdentityUserId)
            .Select(ur => _dbContext.Roles.SingleOrDefault(r => r.Id == ur.RoleId).Name)
            .ToList()
        }));
    }

    // Get UserProfile with Roles
    [HttpGet("withroles/{id}")]
    [Authorize]
    public IActionResult GetWithRoles(int id)
    {
        // Check if User is Admin
        bool isAdmin = User.Identity.IsAuthenticated && User.IsInRole("Admin");

        // Find User's UserName
        var userName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile userAccount = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == userName);

        var user = _dbContext.UserProfiles
            .Include(up => up.IdentityUser)
            .SingleOrDefault(up => up.Id == id);

        if (!isAdmin && userAccount.Id != user.Id)
        {
            return BadRequest();
        }

        return Ok(new UserProfileDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            Email = user.IdentityUser.Email,
            ForcePasswordChange = user.ForcePasswordChange,
            UserName = user.IdentityUser.UserName,
            IdentityUserId = user.IdentityUserId,
            Roles = _dbContext.UserRoles
                .Where(ur => ur.UserId == user.IdentityUserId)
                .Select(ur => _dbContext.Roles.SingleOrDefault(r => r.Id == ur.RoleId).Name)
                .ToList()
        });
    }

    // Promote UserProfile
    [HttpPost("promote/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Promote(string id)
    {
        IdentityRole role = _dbContext.Roles.SingleOrDefault(r => r.Name == "Admin");
        // This will create a new row in the many-to-many UserRoles table.
        _dbContext.UserRoles.Add(new IdentityUserRole<string>
        {
            RoleId = role.Id,
            UserId = id
        });
        _dbContext.SaveChanges();
        return NoContent();
    }

    // Demote UserProfile
    [HttpPost("demote/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Demote(string id)
    {
        IdentityRole role = _dbContext.Roles
            .SingleOrDefault(r => r.Name == "Admin");
        IdentityUserRole<string> userRole = _dbContext
            .UserRoles
            .SingleOrDefault(ur =>
                ur.RoleId == role.Id &&
                ur.UserId == id);

        _dbContext.UserRoles.Remove(userRole);
        _dbContext.SaveChanges();
        return NoContent();
    }

    // Edit UserProfile
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateUserProfile(UserProfileDTO update, int id)
    {
        var foundUP = _dbContext
            .UserProfiles
            .Include(up => up.IdentityUser)
            .SingleOrDefault(up => up.Id == id);

        if (foundUP == null)
        {
            return NotFound();
        }

        bool isUpdated = false;

        // Update FirstName
        if (!string.IsNullOrWhiteSpace(update.FirstName) && update.FirstName != foundUP.FirstName)
        {
            foundUP.FirstName = update.FirstName.Trim();
            isUpdated = true;
        }
        // Update LastName
        if (!string.IsNullOrWhiteSpace(update.LastName) && update.LastName != foundUP.LastName)
        {
            foundUP.LastName = update.LastName.Trim();
            isUpdated = true;
        }
        // Update Email
        if (!string.IsNullOrWhiteSpace(update.Email) && update.Email != foundUP.IdentityUser.Email)
        {
            foundUP.IdentityUser.Email = update.Email.Trim();
            isUpdated = true;
        }
        // Update UserName
        if (!string.IsNullOrWhiteSpace(update.UserName) && update.UserName != foundUP.IdentityUser.UserName)
        {
            foundUP.IdentityUser.UserName = update.UserName.Trim();
            isUpdated = true;
        }
        // Update Address
        if (!string.IsNullOrWhiteSpace(update.Address) && update.Address != foundUP.Address)
        {
            foundUP.Address = update.Address.Trim();
            isUpdated = true;
        }

        if (isUpdated)
        {
            _dbContext.SaveChanges();
            return Ok();
        }

        return NoContent();
    }

    // Change UserProfile's Role
    [HttpPut("changerole/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateUserProfileRole(string id, [FromQuery] string roleId)
    {
        IdentityRole role = _dbContext.Roles.SingleOrDefault(r => r.Id == roleId);
        IdentityUser user = _dbContext.Users.SingleOrDefault(u => u.Id == id);

        var userRole = _dbContext.UserRoles.SingleOrDefault(ur => ur.UserId == user.Id);

        if (userRole == null)
        {
            return NotFound();
        }

        // Delete the old entry
        _dbContext.UserRoles.Remove(userRole);
        _dbContext.SaveChanges();

        // Create a new entry
        _dbContext.UserRoles.Add(new IdentityUserRole<string>
        {
            RoleId = role.Id,
            UserId = user.Id
        });

        _dbContext.SaveChanges();

        return NoContent();
    }

    // Change UserProfile's Password
    [HttpPut("changepassword")]
    [Authorize]
    public IActionResult UpdateUserProfilePassword(NewPasswordDTO newPassword)
    {
        // Check if User is Admin
        bool isAdmin = User.Identity.IsAuthenticated && User.IsInRole("Admin");

        // Find User's UserName
        var userName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile userAccount = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUserId == newPassword.IdentityUserId);

        if (!isAdmin && userAccount.IdentityUserId != newPassword.IdentityUserId)
        {
            return BadRequest();
        }

        var password = Encoding
            .GetEncoding("iso-8859-1")
            .GetString(Convert.FromBase64String(newPassword.Password));

        var user = _dbContext.Users.SingleOrDefault(u => u.Id == newPassword.IdentityUserId);

        if (string.IsNullOrWhiteSpace(password) || user == null)
        {
            return BadRequest();
        }

        var passwordHasher = new PasswordHasher<IdentityUser>();
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        _dbContext.SaveChanges();

        var userProfile = _dbContext.UserProfiles.SingleOrDefault(up => up.IdentityUserId == user.Id);

        if (isAdmin) 
        {
            userProfile.ForcePasswordChange = true;
        }
        else 
        {
            userProfile.ForcePasswordChange = false;
        }

        _dbContext.SaveChanges();

        return Ok();
    }
}