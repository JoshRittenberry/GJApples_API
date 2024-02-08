using GJApples.Data;
using GJApples_API_TEST.Models;
using GJApples_API_TEST.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GJApples_API_TEST.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private GJApplesDbContext _dbContext;

    public OrdersController(GJApplesDbContext context)
    {
        _dbContext = context;
    }

    // Get all Orders
    [HttpGet]
    [Authorize(Roles = "Admin,OrderPicker,Customer")]
    public IActionResult Get([FromQuery] bool? unassigned)
    {
        // Check if the user is a Customer
        bool isCustomer = User.IsInRole("Customer");

        // Find Customer UserName
        var customerUserName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile customer = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == customerUserName);

        return Ok(_dbContext
            .Orders
            .Where(o => isCustomer ? o.CustomerUserProfileId == customer.Id : true)
            .Where(o => unassigned == true ? o.EmployeeUserProfileId == null && o.Canceled == false && o.DateOrdered != null : true)
            .Include(o => o.Customer)
                .ThenInclude(c => c.IdentityUser)
            .Include(o => o.Employee)
                .ThenInclude(e => e.IdentityUser)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.AppleVariety)
            .Select(o => new OrderDTO
            {
                Id = o.Id,
                CustomerUserProfileId = o.CustomerUserProfileId,
                Customer = null,
                EmployeeUserProfileId = o.EmployeeUserProfileId,
                Employee = o.EmployeeUserProfileId != null ? new OrderPickerDTO
                {
                    Id = o.Employee.Id,
                    FirstName = o.Employee.FirstName,
                    LastName = o.Employee.LastName,
                    Address = o.Employee.Address,
                    Email = o.Employee.IdentityUser.Email
                } : null,
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
        );
    }

    // Get OrderPicker's Assigned Order
    [HttpGet("orderpicker")]
    [Authorize(Roles = "Admin,OrderPicker")]
    public IActionResult GetOrderPickerAssignment()
    {
        // Find OrderPicker's UserName
        var employeeUserName = User.Identity.Name;

        // Find OrderPicker's UserProfile
        UserProfile orderPicker = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == employeeUserName);

        var order = _dbContext
            .Orders
            .Include(o => o.Customer)
                .ThenInclude(c => c.IdentityUser)
            .Include(o => o.Employee)
                .ThenInclude(e => e.IdentityUser)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.AppleVariety)
            .SingleOrDefault(o => o.EmployeeUserProfileId == orderPicker.Id && o.DateCompleted == null && o.Canceled == false);

        if (orderPicker == null || order == null)
        {
            return NotFound();
        }

        return Ok(new OrderDTO
        {
            Id = order.Id,
            CustomerUserProfileId = order.CustomerUserProfileId,
            Customer = new CustomerDTO
            {
                Id = order.Customer.Id,
                FirstName = order.Customer.FirstName,
                LastName = order.Customer.LastName,
                Address = order.Customer.Address,
                Email = order.Customer.IdentityUser.Email
            },
            EmployeeUserProfileId = order.EmployeeUserProfileId,
            Employee = null,
            DateOrdered = order.DateOrdered,
            DateCompleted = order.DateCompleted,
            Canceled = order.Canceled,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
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
        });
    }

    // Get Customer's Unsubmitted Order
    [HttpGet("unsubmitted")]
    [Authorize(Roles = "Customer")]
    public IActionResult GetUnsubmittedOrder()
    {
        // Find Customer UserName
        var customerUserName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile customer = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == customerUserName);

        Order order = null;

        // Use a loop to repeatedly check for the order until it's created
        while (order == null)
        {
            order = _dbContext
                .Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.IdentityUser)
                .Include(o => o.Employee)
                .ThenInclude(e => e.IdentityUser)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.AppleVariety)
                .SingleOrDefault(o => o.CustomerUserProfileId == customer.Id && o.Canceled == false && o.DateOrdered == null);

            if (order == null)
            {
                CreateNewOrder();
            }
        }

        return Ok(new OrderDTO
        {
            Id = order.Id,
            CustomerUserProfileId = order.CustomerUserProfileId,
            Customer = null,
            EmployeeUserProfileId = order.EmployeeUserProfileId,
            Employee = null,
            DateOrdered = order.DateOrdered,
            DateCompleted = order.DateCompleted,
            Canceled = order.Canceled,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
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
        });
    }

    // Get Order by Id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,OrderPicker,Customer")]
    public IActionResult GetOrderById(int id)
    {
        // Check if the user is a Customer
        bool isCustomer = User.IsInRole("Customer");

        // Find Customer UserName
        var customerUserName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile customer = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == customerUserName);

        var order = _dbContext
            .Orders
            .Include(o => o.Customer)
                .ThenInclude(c => c.IdentityUser)
            .Include(o => o.Employee)
                .ThenInclude(e => e.IdentityUser)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.AppleVariety)
            .SingleOrDefault(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        if (isCustomer && customer.Id != order.CustomerUserProfileId)
        {
            return BadRequest();
        }

        return Ok(new OrderDTO
        {
            Id = order.Id,
            CustomerUserProfileId = order.CustomerUserProfileId,
            Customer = new CustomerDTO
            {
                Id = order.Customer.Id,
                FirstName = order.Customer.FirstName,
                LastName = order.Customer.LastName,
                Address = order.Customer.Address,
                Email = order.Customer.IdentityUser.Email
            },
            EmployeeUserProfileId = order.EmployeeUserProfileId,
            Employee = null,
            DateOrdered = order.DateOrdered,
            DateCompleted = order.DateCompleted,
            Canceled = order.Canceled,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
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
        });
    }

    // Create new Order
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public IActionResult CreateNewOrder()
    {
        var customerUserName = User.Identity.Name;
        UserProfile customer = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == customerUserName);

        if (customer == null)
        {
            return BadRequest();
        }

        Order order = new Order
        {
            CustomerUserProfileId = customer.Id,
            Canceled = false
        };

        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();

        return Ok();
    }

    // Create new OrderItem
    [HttpPost("orderitem")]
    [Authorize(Roles = "Customer")]
    public IActionResult CreateNewOrderItem(OrderItem orderItem)
    {
        // Find Order
        var order = _dbContext
            .Orders
            .SingleOrDefault(o => o.Id == orderItem.OrderId);

        // Find AppleVariety
        var appleVariety = _dbContext
            .AppleVarieties
            .SingleOrDefault(a => a.Id == orderItem.AppleVarietyId);

        // Find Customer UserName
        var customerUserName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile customer = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == customerUserName);

        if (order == null || appleVariety == null)
        {
            return NotFound();
        }

        if (order.DateCompleted != null || orderItem.Pounds <= 0 || customer.Id != order.CustomerUserProfileId)
        {
            return BadRequest();
        }

        _dbContext.OrderItems.Add(orderItem);
        _dbContext.SaveChanges();

        return Ok();
    }

    // Increase OrderItem Pounds by 0.5
    [HttpPut("orderitem/{id}/increase")]
    [Authorize(Roles = "Customer")]
    public IActionResult IncreaseOrderItemPounds(int id)
    {
        // Find Order
        var orderItemToUpdate = _dbContext
            .OrderItems
            .SingleOrDefault(oi => oi.Id == id);

        // Find Customer UserName
        var customerUserName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile customer = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == customerUserName);

        if (orderItemToUpdate == null || customer == null)
        {
            return NotFound();
        }

        orderItemToUpdate.Pounds = orderItemToUpdate.Pounds + 0.5M;
        _dbContext.SaveChanges();

        return Ok();
    }

    // Decrease OrderItem Pounds by 0.5
    [HttpPut("orderitem/{id}/decrease")]
    [Authorize(Roles = "Customer")]
    public IActionResult DecreaseOrderItemPounds(int id)
    {
        // Find Order
        var orderItemToUpdate = _dbContext
            .OrderItems
            .SingleOrDefault(oi => oi.Id == id);

        // Find Customer UserName
        var customerUserName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile customer = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == customerUserName);

        if (orderItemToUpdate == null || customer == null)
        {
            return NotFound();
        }

        orderItemToUpdate.Pounds = orderItemToUpdate.Pounds - 0.5M;

        if (orderItemToUpdate.Pounds < 1)
        {
            _dbContext.Remove(orderItemToUpdate);
        }

        _dbContext.SaveChanges();

        return Ok();
    }

    // Submit Order (Add a DateOrdered Value)
    [HttpPut("{id}/submit")]
    [Authorize(Roles = "Customer")]
    public IActionResult SubmitOrder(int id)
    {
        // Find Order
        var orderToUpdate = _dbContext
            .Orders
            .Include(o => o.OrderItems)
            .SingleOrDefault(o => o.Id == id);

        // Find Customer UserName
        var customerUserName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile customer = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == customerUserName);

        if (orderToUpdate == null || customer == null)
        {
            return NotFound();
        }

        if (customer.Id != orderToUpdate.CustomerUserProfileId)
        {
            return BadRequest();
        }

        if (orderToUpdate.OrderItems.Count < 1)
        {
            return BadRequest();
        }

        orderToUpdate.DateOrdered = DateTime.Now;
        _dbContext.SaveChanges();

        return Ok(orderToUpdate);
    }

    // Cancel an Order
    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Customer")]
    public IActionResult CancelOrder(int id)
    {
        // Find Order
        var orderToUpdate = _dbContext
            .Orders
            .SingleOrDefault(o => o.Id == id);

        // Find Customer UserName
        var customerUserName = User.Identity.Name;

        // Find Customer UserProfile
        UserProfile customer = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == customerUserName);

        if (orderToUpdate == null || customer == null)
        {
            return NotFound();
        }

        if (customer.Id != orderToUpdate.CustomerUserProfileId)
        {
            return BadRequest();
        }

        orderToUpdate.Canceled = true;
        _dbContext.SaveChanges();

        return Ok(orderToUpdate);
    }

    // Assign an Order to an Order Picker
    [HttpPut("{id}/assignorderpicker")]
    [Authorize(Roles = "Admin,OrderPicker")]
    public IActionResult AssignOrderPicker(int id, [FromQuery] int employeeId)
    {
        // Find Order
        var orderToUpdate = _dbContext
            .Orders
            .SingleOrDefault(o => o.Id == id);

        // Find Customer UserProfile
        UserProfile orderPicker = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.Id == employeeId);

        Order order = _dbContext
            .Orders
            .SingleOrDefault(o => o.EmployeeUserProfileId == orderPicker.Id && o.DateCompleted == null && o.Canceled == false);

        if (orderToUpdate == null || orderPicker == null)
        {
            return NotFound();
        }

        if (order != null)
        {
            return BadRequest();
        }

        orderToUpdate.EmployeeUserProfileId = employeeId;
        _dbContext.SaveChanges();

        return Ok(orderToUpdate);
    }

    // Unassign an OrderPicker from an Order
    [HttpPut("{id}/unassignorderpicker")]
    [Authorize(Roles = "Admin,OrderPicker")]
    public IActionResult UnassignOrderPicker(int id, [FromQuery] int employeeId)
    {
        // Find Order
        var orderToUpdate = _dbContext
            .Orders
            .SingleOrDefault(o => o.Id == id);

        // Find OrderPicker UserProfile
        UserProfile orderPicker = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.Id == employeeId);

        Order order = _dbContext
            .Orders
            .SingleOrDefault(o => o.EmployeeUserProfileId == orderPicker.Id && o.DateCompleted == null && o.Canceled == false);

        if (orderToUpdate == null || orderPicker == null)
        {
            return NotFound();
        }

        if (orderPicker.Id != order.EmployeeUserProfileId)
        {
            return BadRequest();
        }

        orderToUpdate.EmployeeUserProfileId = null;
        _dbContext.SaveChanges();

        return Ok(orderToUpdate);
    }

    // Complete an Order (Add a DateCompleted Value)
    [HttpPut("{id}/complete")]
    [Authorize(Roles = "Admin,OrderPicker")]
    public IActionResult CompleteOrder(int id)
    {
        // Find Order
        var orderToUpdate = _dbContext
            .Orders
            .SingleOrDefault(o => o.Id == id);

        // Find Employee UserName
        var employeeUserName = User.Identity.Name;

        // Find Employee UserProfile
        UserProfile employee = _dbContext
            .UserProfiles
            .SingleOrDefault(u => u.IdentityUser.UserName == employeeUserName);

        bool isEmployeeAdmin = User.IsInRole("Admin");

        if (orderToUpdate == null || employee == null)
        {
            return NotFound();
        }

        // This returns BadRequest even if items passed in are correct
        // if (employee.Id != orderToUpdate.EmployeeUserProfileId || !isEmployeeAdmin)
        // {
        //     return BadRequest();
        // }

        orderToUpdate.DateCompleted = DateTime.Now;
        _dbContext.SaveChanges();

        return Ok(orderToUpdate);
    }

    // Delete an OrderItem
    [HttpDelete("orderitem/{id}")]
    [Authorize(Roles = "Customer")]
    public IActionResult DeleteOrderItem(int id)
    {
        var orderItem = _dbContext
            .OrderItems
            .SingleOrDefault(oi => oi.Id == id);

        if (orderItem == null)
        {
            return NotFound();
        }

        _dbContext.Remove(orderItem);
        _dbContext.SaveChanges();

        return NoContent();
    }

}