using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using GJApples_API_TEST.Models;
using GJApples_API_TEST.Models.DTOs;
using GJApples_API_TEST.Data;

namespace GJApples_API_TEST.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private GJApplesDbContext _dbContext;
    private UserManager<IdentityUser> _userManager;

    // private RoleManager<IdentityRole> _roleManager;  // maybe this will work???

    public AuthController(GJApplesDbContext context, UserManager<IdentityUser> userManager)
    {
        _dbContext = context;
        _userManager = userManager;
    }

    [HttpPost("login")]
    public IActionResult Login([FromHeader(Name = "Authorization")] string authHeader)
    {
        try
        {
            string encodedCreds = authHeader.Substring(6).Trim();
            string creds = Encoding
            .GetEncoding("iso-8859-1")
            .GetString(Convert.FromBase64String(encodedCreds));

            // Get email and password
            int separator = creds.IndexOf(':');
            string email = creds.Substring(0, separator);
            string password = creds.Substring(separator + 1);

            var user = _dbContext.Users.Where(u => u.Email == email).FirstOrDefault();
            var userRoles = _dbContext.UserRoles.Where(ur => ur.UserId == user.Id).ToList();
            var hasher = new PasswordHasher<IdentityUser>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (user != null && result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)

                };

                foreach (var userRole in userRoles)
                {
                    var role = _dbContext.Roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)).Wait();

                return Ok();
            }

            return new UnauthorizedResult();
        }
        catch (Exception ex)
        {
            return StatusCode(500);
        }
    }

    [HttpGet]
    [Route("logout")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult Logout()
    {
        try
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Me")]
    [Authorize]
    public IActionResult Me()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = _dbContext.UserProfiles.SingleOrDefault(up => up.IdentityUserId == identityUserId);
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        if (profile != null)
        {
            var userDto = new UserProfileDTO
            {
                Id = profile.Id,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Address = profile.Address,
                IdentityUserId = identityUserId,
                UserName = User.FindFirstValue(ClaimTypes.Name),
                Email = User.FindFirstValue(ClaimTypes.Email),
                ForcePasswordChange = profile.ForcePasswordChange,
                Roles = roles
            };

            return Ok(userDto);
        }
        return NotFound();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegistrationDTO registration)
    {
        var user = new IdentityUser
        {
            UserName = registration.UserName,
            Email = registration.Email
        };

        var password = Encoding
            .GetEncoding("iso-8859-1")
            .GetString(Convert.FromBase64String(registration.Password));

        var result = await _userManager.CreateAsync(user, password);

        var newUserRole = _dbContext.Roles.SingleOrDefault(r => r.Name == "Customer");

        if (result.Succeeded)
        {
            await _dbContext.UserProfiles.AddAsync(new UserProfile
            {
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Address = registration.Address,
                IdentityUserId = user.Id,
                ForcePasswordChange = false
            });


            await _dbContext.UserRoles.AddAsync(new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = newUserRole.Id
            });

            await _dbContext.SaveChangesAsync();

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

            var userRoles = _dbContext.UserRoles.Where(ur => ur.UserId == user.Id).ToList();

            foreach (var userRole in userRoles)
            {
                var role = _dbContext.Roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity)).Wait();

            return Ok();
        }
        return StatusCode(500);
    }

    [HttpPost("createemployee")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEmployee(RegistrationDTO registration, [FromQuery] string? roleName)
    {
        var user = new IdentityUser
        {
            UserName = registration.UserName,
            Email = registration.Email
        };

        var password = Encoding
            .GetEncoding("iso-8859-1")
            .GetString(Convert.FromBase64String(registration.Password));

        var result = await _userManager.CreateAsync(user, password);

        var newUserRole = _dbContext.Roles.SingleOrDefault(r => r.Name == roleName);

        if (newUserRole == null)
        {
            newUserRole = _dbContext.Roles.SingleOrDefault(r => r.Name == "Harvester");
        }

        if (result.Succeeded)
        {
            await _dbContext.UserProfiles.AddAsync(new UserProfile
            {
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Address = registration.Address,
                IdentityUserId = user.Id,
                ForcePasswordChange = true
            });


            await _dbContext.UserRoles.AddAsync(new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = newUserRole.Id
            });

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
        return StatusCode(500);
    }

    [HttpPost("createcustomer")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCustomer(RegistrationDTO registration, [FromQuery] string? roleName)
    {
        var user = new IdentityUser
        {
            UserName = registration.UserName,
            Email = registration.Email
        };

        var password = Encoding
            .GetEncoding("iso-8859-1")
            .GetString(Convert.FromBase64String(registration.Password));

        var result = await _userManager.CreateAsync(user, password);

        var newUserRole = _dbContext.Roles.SingleOrDefault(r => r.Name == "Customer");

        if (result.Succeeded)
        {
            await _dbContext.UserProfiles.AddAsync(new UserProfile
            {
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Address = registration.Address,
                IdentityUserId = user.Id,
                ForcePasswordChange = true
            });


            await _dbContext.UserRoles.AddAsync(new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = newUserRole.Id
            });

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
        return StatusCode(500);
    }
}