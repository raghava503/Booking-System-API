# 🎟️ BookingApi: One Slot, One Winner
![CI](https://github.com/raghava503/Booking-System-API/actions/workflows/ci.yml/badge.svg)
> 20 people click "Book" on the same slot at the same instant.
> **Exactly one gets it. The other 19 get a polite `409 Conflict`.**
> No double bookings. No overbooked haircuts. No drama.

A small ASP.NET Core Web API for booking time slots, built to answer one question properly: **what happens when two people grab the same slot at the same time?**

![.NET](https://img.shields.io/badge/.NET-10-512BD4) ![SQLite](https://img.shields.io/badge/SQLite-EF%20Core-003B57) ![Tests](https://img.shields.io/badge/tests-xUnit-green)

---

## 🧠 The interesting part: beating the race condition

Checking "is this slot free?" before saving is **not enough**. Two requests can both pass the check in the same millisecond. So the API defends in two layers:

| Layer | What it does | Why it exists |
|-------|--------------|---------------|
| 1. Friendly check | `AnyAsync` looks for an existing booking | Fast, clear `409` for the normal case |
| 2. Database backstop | Unique index on `Bookings.SlotId` | Guarantees correctness even when requests race |

When the index rejects the second insert (SQLite error 19), the API catches it and still answers `409 Conflict`, so the caller sees a clean error instead of a crash.

## 🔌 Endpoints

| Method | Route | What it does | Success |
|--------|-------|--------------|---------|
| `GET` | `/api/slots?onlyAvailable=true` | List upcoming slots | `200` |
| `POST` | `/api/bookings` | Book a slot | `201` |
| `GET` | `/api/bookings/{id}` | Look up a booking | `200` |
| `DELETE` | `/api/bookings/{id}` | Cancel a booking | `204` |

Other responses: `400` (past slot or invalid name), `404` (unknown slot or booking), `409` (slot taken).

**Example**

```http
POST /api/bookings
{ "slotId": 3, "customerName": "Rock" }
```

## 🚀 Run it

```bash
git clone https://github.com/raghava503/Booking-System-API.git
cd <repo-name>/BookingApi
dotnet run
```

Open `https://localhost:<port>/swagger` and click **Try it out**. The database is created and seeded with slots automatically on startup.

## 🧪 Prove it works

```bash
dotnet test
```

| Test | What it proves |
|------|----------------|
| `TwentyParallelRequests_ForOneSlot_ExactlyOneSucceeds` | 20 simultaneous requests give 1 × `201` and 19 × `409` |
| `Database_RejectsSecondBookingForSameSlot` | The unique index works at the database level |
| `UnknownSlot_Returns404` | Booking a missing slot is handled |
| `ShortName_Returns400` | Bad input is rejected |

Integration tests run against a real SQLite database through `WebApplicationFactory`, with no mocks.

## 🛠️ Built with

- ASP.NET Core Web API (controllers, DTO records)
- Entity Framework Core + SQLite + migrations
- Swagger UI (Swashbuckle)
- xUnit + `Microsoft.AspNetCore.Mvc.Testing`

## 📚 What I learned

- Why a check-then-insert is a race condition, and why the database must have the final say.
- EF Core tracks a one-to-one relationship and can silently *replace* a row instead of failing, so my database test needed separate contexts.
- Choosing precise HTTP status codes (`201`, `204`, `400`, `404`, `409`).

## 🔮 Next ideas

- JWT login so customers see only their own bookings
- Deploy to Azure App Service
- GitHub Actions CI to run the tests on every push

---
## 👤 Author

**Raghava**: aspiring full stack .NET developer
[GitHub](https://github.com/raghava503) · [LinkedIn](https://linkedin.com/in/raghava-gudiwada-0b2bb4317)