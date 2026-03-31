namespace RestaurantWithAi.Shared.Exceptions;

public class WaiterNotFoundException : Exception
{
    public WaiterNotFoundException(string message) : base(message)
    {
    }
}
