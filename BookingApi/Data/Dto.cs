using System.ComponentModel.DataAnnotations;
namespace BookingApi.Models;

public class CreateBookingRequest
{
    public int SlotId { get; set; }
    [Required, StringLength(50, MinimumLength = 2)]
    public string CustomerName { get; set; } = "";
}
public record SlotDto(int Id, DateTime StartsAt, int DurationMinutes, bool IsBooked);
public record BookingDto(int Id, int SlotId, string CustomerName, DateTime StartsAt);