namespace BookingApi.Models;
public class Booking
{
    public int Id { get; set; }
    public int SlotId { get; set; }
    public Slot Slot { get; set; } = null!;
    public string CustomerName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
