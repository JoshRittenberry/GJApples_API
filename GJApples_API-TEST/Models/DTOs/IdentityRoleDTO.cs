using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GJApples_API_TEST.Models.DTOs;

public class IdentityRoleDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
}