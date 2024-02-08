using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GJApples_API_TEST.Models;

public class AppleVariety
{
    public int Id { get; set; }
    [Required]
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
    [Required]
    public decimal CostPerPound { get; set; }
    public bool IsActive { get; set; }
    public List<Tree>? Trees { get; set; }
    public List<OrderItem>? OrderItems { get; set; }
}