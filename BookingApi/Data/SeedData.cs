
using BookingApi.Models;
namespace BookingApi.Data;
public static class SeedData
{
    public static void Run(AppDbContext db)
    {
        if (db.Slots.Any()) return;
        var start = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
        db.Slots.AddRange(Enumerable.Range(0, 8).Select(i =>
            new Slot { StartsAt = start.AddHours(i), DurationMinutes = 60 }));
        db.SaveChanges();
    }
}
