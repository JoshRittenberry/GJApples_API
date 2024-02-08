using System.ComponentModel.DataAnnotations;

namespace GJApples_API_TEST.Models;

public class Tree
{
    public int Id { get; set; }
    [Required]
    public int AppleVarietyId { get; set; }
    public AppleVariety? AppleVariety { get; set; }
    [Required]
    public DateTime DatePlanted { get; set; }
    public DateTime? DateRemoved { get; set; }
    public List<TreeHarvestReport>? TreeHarvestReports { get; set; }
}