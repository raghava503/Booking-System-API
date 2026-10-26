
namespace BookingApi.Models;
public class Slot
{
    public int Id { get; set; }
    public DateTime StartsAt { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public Booking? Booking { get; set; }
}

