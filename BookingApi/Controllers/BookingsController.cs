
using BookingApi.Data;
using BookingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Controllers;

[ApiController]
[Route("api")]
public class BookingsController(AppDbContext db) : ControllerBase
{
    [HttpGet("slots")]
    public async Task<ActionResult<List<SlotDto>>> GetSlots([FromQuery] bool onlyAvailable = false)
    {
        var query = db.Slots.AsNoTracking().Where(s => s.StartsAt > DateTime.UtcNow);
        if (onlyAvailable) query = query.Where(s => s.Booking == null);

        return await query.OrderBy(s => s.StartsAt)
            .Select(s => new SlotDto(s.Id, s.StartsAt, s.DurationMinutes, s.Booking != null))
            .ToListAsync();
    }

    [HttpPost("bookings")]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest req)
    {
        var slot = await db.Slots.FindAsync(req.SlotId);
        if (slot is null) return NotFound("Slot not found.");
        if (slot.StartsAt <= DateTime.UtcNow) return BadRequest("That slot is in the past.");

        // Friendly early exit. NOT enough on its own (see below).
        if (await db.Bookings.AnyAsync(b => b.SlotId == req.SlotId))
            return Conflict("That slot is already booked.");

        var booking = new Booking { SlotId = req.SlotId, CustomerName = req.CustomerName.Trim() };
        db.Bookings.Add(booking);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            // Two requests passed the check at the same moment; the unique index stopped the second.
            return Conflict("That slot was just booked by someone else.");
        }

        return CreatedAtAction(nameof(GetBooking), new { id = booking.Id },
            new BookingDto(booking.Id, booking.SlotId, booking.CustomerName, slot.StartsAt));
    }

    [HttpGet("bookings/{id:int}")]
    public async Task<ActionResult<BookingDto>> GetBooking(int id)
    {
        var b = await db.Bookings.AsNoTracking().Include(x => x.Slot).FirstOrDefaultAsync(x => x.Id == id);
        return b is null ? NotFound() : new BookingDto(b.Id, b.SlotId, b.CustomerName, b.Slot.StartsAt);
    }

    [HttpDelete("bookings/{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var b = await db.Bookings.FindAsync(id);
        if (b is null) return NotFound();
        db.Bookings.Remove(b);
        await db.SaveChangesAsync();
        return NoContent();
    }
}