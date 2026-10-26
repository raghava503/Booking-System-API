// BookingTests.cs
using System.Net;
using System.Net.Http.Json;
using BookingApi.Data;
using BookingApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Tests;

public class BookingTests(TestFactory factory) : IClassFixture<TestFactory>
{
    [Fact]
    public async Task TwentyParallelRequests_ForOneSlot_ExactlyOneSucceeds()
    {
        var client = factory.CreateClient();
        var slots = await client.GetFromJsonAsync<List<SlotDto>>("/api/slots");
        var slotId = slots![0].Id;

        var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(i =>
            client.PostAsJsonAsync("/api/bookings", new { slotId, customerName = $"User{i}" })));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(19, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task Database_RejectsSecondBookingForSameSlot()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slot = await db.Slots.OrderBy(s => s.Id).Skip(1).FirstAsync();

        db.Bookings.Add(new Booking { SlotId = slot.Id, CustomerName = "A" });
        await db.SaveChangesAsync();

        db.Bookings.Add(new Booking { SlotId = slot.Id, CustomerName = "B" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task UnknownSlot_Returns404()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/bookings", new { slotId = 9999, customerName = "Raghava" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShortName_Returns400()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/bookings", new { slotId = 3, customerName = "A" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}