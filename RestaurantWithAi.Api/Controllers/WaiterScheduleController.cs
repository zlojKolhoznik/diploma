using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/waiters/{waiterId}/schedule")]
public class WaiterScheduleController(IWaiterScheduleService scheduleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WaiterScheduleResponse>>> GetSchedules(string waiterId)
    {
        try
        {
            var schedules = await scheduleService.GetSchedulesForWaiterAsync(waiterId);
            return Ok(schedules);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{date:datetime}")]
    public async Task<ActionResult<WaiterScheduleResponse>> GetSchedule(string waiterId, DateOnly date)
    {
        try
        {
            var schedule = await scheduleService.GetScheduleAsync(waiterId, date);
            if (schedule == null)
                return NotFound($"No schedule found for waiter {waiterId} on {date}");
            return Ok(schedule);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateSchedule(string waiterId, CreateWaiterScheduleRequest request)
    {
        try
        {
            var scheduleId = await scheduleService.CreateScheduleAsync(request, User.FindFirst("sub")?.Value ?? string.Empty, true);
            return CreatedAtAction(nameof(GetSchedule), new { waiterId = request.WaiterId, date = request.Date }, new { id = scheduleId });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{scheduleId:guid}")]
    public async Task<ActionResult> UpdateSchedule(string waiterId, Guid scheduleId, UpdateWaiterScheduleRequest request)
    {
        try
        {
            await scheduleService.UpdateScheduleAsync(scheduleId, request, User.FindFirst("sub")?.Value ?? string.Empty, true);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{scheduleId:guid}")]
    public async Task<ActionResult> DeleteSchedule(string waiterId, Guid scheduleId)
    {
        try
        {
            await scheduleService.DeleteScheduleAsync(scheduleId, User.FindFirst("sub")?.Value ?? string.Empty, true);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

