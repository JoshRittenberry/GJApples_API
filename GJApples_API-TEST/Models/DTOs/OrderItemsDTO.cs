using System.ComponentModel.DataAnnotations;

namespace GJApples_API_TEST.Models.DTOs;

public class OrderItemDTO
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int AppleVarietyId { get; set; }
    public AppleVarietyDTO? AppleVariety { get; set; }
    public decimal Pounds { get; set; }
    public decimal? TotalItemCost
    {
        get
        {
            if (AppleVariety == null)
            {
                return null;
            }
            
            return AppleVariety.CostPerPound * Pounds;
        }
    }
}