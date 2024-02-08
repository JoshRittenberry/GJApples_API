using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GJApples_API_TEST.Models.DTOs;

public class TreeHarvestReportDTO
{
    public int Id { get; set; }
    public int TreeId { get; set; }
    public TreeDTO? Tree { get; set; }
    [ForeignKey("Employee")]
    public int EmployeeUserProfileId { get; set; }
    public HarvesterDTO? Employee { get; set; }
    public DateTime? HarvestDate { get; set; }
    [Range(0, 999)]
    public decimal? PoundsHarvested { get; set; }
}