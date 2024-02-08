using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GJApples_API_TEST.Models.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    [ForeignKey("Customer")]
    public int CustomerUserProfileId { get; set; }
    public CustomerDTO? Customer { get; set; }
    [ForeignKey("Employee")]
    public int? EmployeeUserProfileId { get; set; }
    public OrderPickerDTO? Employee { get; set; }
    public DateTime? DateOrdered { get; set; }
    public DateTime? DateCompleted { get; set; }
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
    public List<OrderItemDTO>? OrderItems { get; set; }
}