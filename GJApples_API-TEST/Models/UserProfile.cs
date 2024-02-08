using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GJApples_API_TEST.Models;

public class UserProfile
{
    public int Id { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Address { get; set; }
    public bool ForcePasswordChange { get; set; }
    public string IdentityUserId { get; set; }
    public IdentityUser IdentityUser { get; set; }
    public List<Order>? CompletedOrders { get; set; }
    public List<TreeHarvestReport>? TreeHarvestReports { get; set; }
    public List<Order>? Orders { get; set; }
}