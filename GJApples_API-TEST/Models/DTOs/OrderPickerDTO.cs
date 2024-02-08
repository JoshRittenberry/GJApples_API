using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GJApples_API_TEST.Models.DTOs;

public class OrderPickerDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    public string? IdentityUserId { get; set; }
    public List<OrderDTO>? CompletedOrders { get; set; }
}