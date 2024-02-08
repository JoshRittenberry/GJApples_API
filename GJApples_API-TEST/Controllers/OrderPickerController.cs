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
public class OrderPickersController : ControllerBase
{
    private GJApplesDbContext _dbContext;

    public OrderPickersController(GJApplesDbContext context)
    {
        _dbContext = context;
    }

    // Get all OrderPicker Profiles
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Get()
    {
        return Ok(_dbContext
            .UserProfiles
            .Include(u => u.IdentityUser)
            .Include(u => u.CompletedOrders)
                .ThenInclude(co => co.Customer)
                    .ThenInclude(e => e.IdentityUser)
            .Include(u => u.CompletedOrders)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.AppleVariety)
            .Where(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.IdentityUserId &&
                       _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "OrderPicker")))
            .Select(orderPicker => new OrderPickerDTO
            {
                Id = orderPicker.Id,
                FirstName = orderPicker.FirstName,
                LastName = orderPicker.LastName,
                Address = orderPicker.Address,
                Email = orderPicker.IdentityUser.Email,
                IdentityUserId = orderPicker.IdentityUserId,
                CompletedOrders = orderPicker.CompletedOrders.Select(co => new OrderDTO
                {
                    Id = co.Id,
                    CustomerUserProfileId = co.CustomerUserProfileId,
                    Customer = new CustomerDTO
                    {
                        Id = co.Employee.Id,
                        FirstName = co.Employee.FirstName,
                        LastName = co.Employee.LastName,
                        Address = co.Employee.Address,
                        Email = co.Employee.IdentityUser.Email,
                        Orders = null
                    },
                    EmployeeUserProfileId = co.EmployeeUserProfileId,
                    Employee = null,
                    DateOrdered = co.DateOrdered,
                    DateCompleted = co.DateCompleted,
                    Canceled = co.Canceled,
                    OrderItems = co.OrderItems.Select(oi => new OrderItemDTO
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

    // Get OrderPicker Profile by Id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,OrderPicker")]
    public IActionResult GetCustomerById(int id)
    {
        var orderPicker = _dbContext
            .UserProfiles
            .Include(u => u.IdentityUser)
            .Include(u => u.CompletedOrders)
                .ThenInclude(co => co.Customer)
                    .ThenInclude(e => e.IdentityUser)
            .Include(u => u.CompletedOrders)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.AppleVariety)
            .Where(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.IdentityUserId &&
                       _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "OrderPicker")))
            .SingleOrDefault(op => op.Id == id);

        if (orderPicker == null)
        {
            return NotFound();
        }

        return Ok(new OrderPickerDTO
        {
            Id = orderPicker.Id,
            FirstName = orderPicker.FirstName,
            LastName = orderPicker.LastName,
            Address = orderPicker.Address,
            Email = orderPicker.IdentityUser.Email,
            CompletedOrders = orderPicker.CompletedOrders.Select(co => new OrderDTO
            {
                Id = co.Id,
                CustomerUserProfileId = co.CustomerUserProfileId,
                Customer = new CustomerDTO
                {
                    Id = co.Employee.Id,
                    FirstName = co.Employee.FirstName,
                    LastName = co.Employee.LastName,
                    Address = co.Employee.Address,
                    Email = co.Employee.IdentityUser.Email,
                    Orders = null
                },
                EmployeeUserProfileId = co.EmployeeUserProfileId,
                Employee = null,
                DateOrdered = co.DateOrdered,
                DateCompleted = co.DateCompleted,
                Canceled = co.Canceled,
                OrderItems = co.OrderItems.Select(oi => new OrderItemDTO
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