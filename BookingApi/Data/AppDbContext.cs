
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder b) =>
        b.Entity<Slot>()
         .HasOne(s => s.Booking).WithOne(x => x.Slot)
         .HasForeignKey<Booking>(x => x.SlotId);
}