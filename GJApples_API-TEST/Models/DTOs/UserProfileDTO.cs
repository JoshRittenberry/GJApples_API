using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GJApples_API_TEST.Controllers;
using Microsoft.AspNetCore.Identity;

namespace GJApples_API_TEST.Models.DTOs;

public class UserProfileDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    public bool ForcePasswordChange { get; set; }
    [MaxLength(50)]
    public string UserName { get; set; }
    public string? IdentityUserId { get; set; }
    public IdentityUser? IdentityUser { get; set; }
    public List<string>? Roles { get; set; }
}