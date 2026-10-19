using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Repositories;

namespace Server.Controllers;

[ApiController, Route("api/bookings")]
public sealed class BookingsController(IBookingRepository bookings) : ControllerBase
{
    [HttpGet, RequireSession("Student")]
    public async Task<IActionResult> List(CancellationToken ct) => Ok((await bookings.ForStudentAsync(this.User().AccountId, ct)).Select(x => new { x.Id, studentEmail = this.User().Email, mentorId = x.MentorId, mentorName = x.Mentor.Account.Name, minutes = x.MentorPlan.DurationMinutes, date = x.StartsAt.ToString("yyyy-MM-dd"), time = x.StartsAt.ToString("HH:mm"), x.MentorPlan.Price, x.Status }));

    [HttpPost, RequireSession("Student")]
    public async Task<IActionResult> Create(BookingRequest request, CancellationToken ct)
    {
        try { var booking = await bookings.CreateAsync(this.User().AccountId, request, ct); return Created($"/api/bookings/{booking.Id}", new { booking.Id, studentEmail = this.User().Email, booking.MentorId, request.Minutes, request.Date, request.Time, booking.Status }); }
        catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
    }
}
