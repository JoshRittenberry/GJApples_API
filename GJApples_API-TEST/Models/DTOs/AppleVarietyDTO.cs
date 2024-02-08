using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GJApples_API_TEST.Models.DTOs;

public class AppleVarietyDTO
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string ImageUrl { get; set; }
    public decimal? PoundsOnHand
    {
        get
        {
            if (Trees == null || OrderItems == null)
            {
                return null;
            }
            decimal? HarvestedTotal = Trees.Sum(t => t.TreeHarvestReports.Sum(th => th.PoundsHarvested));
            decimal PoundsOrdered = OrderItems.Sum(oi => oi.Pounds);
            return HarvestedTotal - PoundsOrdered;
        }
    }
    public decimal CostPerPound { get; set; }
    public bool IsActive { get; set; }
    public List<TreeDTO>? Trees { get; set; }
    public List<OrderItemDTO>? OrderItems { get; set; }
}