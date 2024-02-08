using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GJApples_API_TEST.Models.DTOs;

public class IdentityUserRoleDTO
{
    public string RoleId { get; set; }
    public string UserId { get; set; }
}