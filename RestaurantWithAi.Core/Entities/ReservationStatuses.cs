namespace RestaurantWithAi.Core.Entities;

public static class ReservationStatuses
{
    public const string Created = "Created";
    public const string InProgress = "InProgress";
    public const string PendingPayment = "PendingPayment";
    public const string Closed = "Closed";

    public static readonly IReadOnlyList<string> Flow = [Created, InProgress, PendingPayment, Closed];

    public static readonly IReadOnlySet<string> OpenStatuses =
        new HashSet<string>(StringComparer.Ordinal) { Created, InProgress, PendingPayment };
}

