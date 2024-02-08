using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GJApples_API_TEST.Models;

public class Order
{
    public int Id { get; set; }
    [Required]
    [ForeignKey("Customer")]
    public int CustomerUserProfileId { get; set; }
    public UserProfile? Customer { get; set; }
    [ForeignKey("Employee")]
    public int? EmployeeUserProfileId { get; set; }
    public UserProfile? Employee { get; set; }
    public DateTime? DateOrdered { get; set; }
    public DateTime? DateCompleted { get; set; }
    [Required]
    public bool Canceled { get; set; }
    public decimal? TotalCost
    {
        get
        {
            if (OrderItems == null)
            {
                return null;
            }

            return OrderItems.Sum(oi => oi.TotalItemCost);
        }
    }
    public List<OrderItem>? OrderItems { get; set; }
}