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
public class CustomersController : ControllerBase
{
    private GJApplesDbContext _dbContext;

    public CustomersController(GJApplesDbContext context)
    {
        _dbContext = context;
    }

    // Get all Customer Profiles
    [HttpGet]
    [Authorize(Roles = "Admin,OrderPicker")]
    public IActionResult Get()
    {
        return Ok(_dbContext
            .UserProfiles
            .Include(u => u.IdentityUser)
            .Include(u => u.Orders)
                .ThenInclude(o => o.Employee)
                    .ThenInclude(e => e.IdentityUser)
            .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.AppleVariety)
            .Where(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.IdentityUserId &&
                       _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Customer")))
            .Select(customer => new CustomerDTO
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Address = customer.Address,
                Email = customer.IdentityUser.Email,
                Orders = customer.Orders.Select(o => new OrderDTO
                {
                    Id = o.Id,
                    CustomerUserProfileId = o.CustomerUserProfileId,
                    Customer = null,
                    EmployeeUserProfileId = o.EmployeeUserProfileId,
                    Employee = o.Employee == null ? null : new OrderPickerDTO
                    {
                        Id = o.Employee.Id,
                        FirstName = o.Employee.FirstName,
                        LastName = o.Employee.LastName,
                        Address = o.Employee.Address,
                        Email = o.Employee.IdentityUser.Email,
                        CompletedOrders = null
                    },
                    DateOrdered = o.DateOrdered,
                    DateCompleted = o.DateCompleted,
                    Canceled = o.Canceled,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDTO
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        AppleVarietyId = oi.AppleVarietyId,
                        AppleVariety = new AppleVarietyDTO
                        {
                            Id = oi.AppleVariety.Id,
                            Type = oi.AppleVariety.Type,
                            ImageUrl = oi.AppleVariety.ImageUrl,
                            CostPerPound = oi.AppleVariety.CostPerPound,
                            IsActive = oi.AppleVariety.IsActive,
                            Trees = null,
                            OrderItems = null
                        },
                        Pounds = oi.Pounds
                    }).ToList()
                }).ToList()
            }).ToList());
    }

    // Get Customer Profile by Id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,OrderPicker")]
    public IActionResult GetCustomerById(int id)
    {
        var customer = _dbContext
            .UserProfiles
                .Include(u => u.IdentityUser)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Employee)
                        .ThenInclude(e => e.IdentityUser)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.AppleVariety)
            .Where(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.IdentityUserId &&
                       _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Customer")))
            .SingleOrDefault(c => c.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(new CustomerDTO
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Address = customer.Address,
            Email = customer.IdentityUser.Email,
            Orders = customer.Orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                CustomerUserProfileId = o.CustomerUserProfileId,
                Customer = null,
                EmployeeUserProfileId = o.EmployeeUserProfileId,
                Employee = o.Employee == null ? null : new OrderPickerDTO
                {
                    Id = o.Employee.Id,
                    FirstName = o.Employee.FirstName,
                    LastName = o.Employee.LastName,
                    Address = o.Employee.Address,
                    Email = o.Employee.IdentityUser.Email,
                    CompletedOrders = null
                },
                DateOrdered = o.DateOrdered,
                DateCompleted = o.DateCompleted,
                Canceled = o.Canceled,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    AppleVarietyId = oi.AppleVarietyId,
                    AppleVariety = new AppleVarietyDTO
                    {
                        Id = oi.AppleVariety.Id,
                        Type = oi.AppleVariety.Type,
                        ImageUrl = oi.AppleVariety.ImageUrl,
                        CostPerPound = oi.AppleVariety.CostPerPound,
                        IsActive = oi.AppleVariety.IsActive,
                        Trees = null,
                        OrderItems = null
                    },
                    Pounds = oi.Pounds
                }).ToList()
            }).ToList()
        });
    }

}