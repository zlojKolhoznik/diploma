using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Orders;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/reservations/{reservationId:guid}/orders")]
[Authorize(Roles = "Waiter,Admin")]
public class OrdersController(IOrdersService ordersService, ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders(Guid restaurantId, Guid reservationId)
    {
        try
        {
            var orders = await ordersService.GetOrdersAsync(restaurantId, reservationId, GetCurrentUserId(), User.IsInRole("Admin"));
            return Ok(orders);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Failed to get orders for reservation {ReservationId} in restaurant {RestaurantId}.", reservationId, restaurantId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving orders for reservation {ReservationId} in restaurant {RestaurantId}.", reservationId, restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponse>> GetOrderById(Guid restaurantId, Guid reservationId, Guid orderId)
    {
        try
        {
            var order = await ordersService.GetOrderByIdAsync(restaurantId, reservationId, orderId, GetCurrentUserId(), User.IsInRole("Admin"));
            return Ok(order);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Failed to get order {OrderId} for reservation {ReservationId} in restaurant {RestaurantId}.", orderId, reservationId, restaurantId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving order {OrderId} for reservation {ReservationId} in restaurant {RestaurantId}.", orderId, reservationId, restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder(Guid restaurantId, Guid reservationId, [FromBody] CreateOrderRequest request)
    {
        try
        {
            await ordersService.CreateOrderAsync(restaurantId, reservationId, request, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Create order request for reservation {ReservationId} in restaurant {RestaurantId} was invalid.", reservationId, restaurantId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Create order failed for reservation {ReservationId} in restaurant {RestaurantId} because a related entity was not found.", reservationId, restaurantId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Create order failed for reservation {ReservationId} in restaurant {RestaurantId} due to state conflict.", reservationId, restaurantId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while creating order for reservation {ReservationId} in restaurant {RestaurantId}.", reservationId, restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{orderId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderStatus(Guid restaurantId, Guid reservationId, Guid orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            await ordersService.UpdateOrderStatusAsync(restaurantId, reservationId, orderId, request, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Update order status request for order {OrderId} was invalid.", orderId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Update order status failed for order {OrderId} because related entities were not found.", orderId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Update order status failed for order {OrderId} due to invalid state transition.", orderId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating status for order {OrderId}.", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("{orderId:guid}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddOrderItem(Guid restaurantId, Guid reservationId, Guid orderId, [FromBody] AddOrderItemRequest request)
    {
        try
        {
            await ordersService.AddOrderItemAsync(restaurantId, reservationId, orderId, request, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Add order item request for order {OrderId} was invalid.", orderId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Add order item failed for order {OrderId} because related entities were not found.", orderId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Add order item failed for order {OrderId} due to order/reservation state conflict.", orderId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while adding item to order {OrderId}.", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{orderId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderItem(Guid restaurantId, Guid reservationId, Guid orderId, Guid itemId, [FromBody] UpdateOrderItemRequest request)
    {
        try
        {
            await ordersService.UpdateOrderItemAsync(restaurantId, reservationId, orderId, itemId, request, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Update order item request for order {OrderId} and item {ItemId} was invalid.", orderId, itemId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Update order item failed for order {OrderId} and item {ItemId} because related entities were not found.", orderId, itemId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Update order item failed for order {OrderId} due to order state conflict.", orderId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating item {ItemId} for order {OrderId}.", itemId, orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveOrderItem(Guid restaurantId, Guid reservationId, Guid orderId, Guid itemId)
    {
        try
        {
            await ordersService.RemoveOrderItemAsync(restaurantId, reservationId, orderId, itemId, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Remove order item failed for order {OrderId} and item {ItemId} because related entities were not found.", orderId, itemId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Remove order item failed for order {OrderId} due to order state conflict.", orderId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while removing item {ItemId} from order {OrderId}.", itemId, orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("{orderId:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CloseOrder(Guid restaurantId, Guid reservationId, Guid orderId)
    {
        try
        {
            await ordersService.CloseOrderAsync(restaurantId, reservationId, orderId, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Close order failed for order {OrderId} because related entities were not found.", orderId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Close order failed for order {OrderId} due to invalid order state.", orderId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while closing order {OrderId}.", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Current user identifier claim is missing.");

        return userId;
    }
}

