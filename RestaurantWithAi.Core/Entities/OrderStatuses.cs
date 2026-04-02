namespace RestaurantWithAi.Core.Entities;

public static class OrderStatuses
{
    public const string Created = "Created";
    public const string InProgress = "InProgress";
    public const string Ready = "Ready";
    public const string Served = "Served";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] Flow = [Created, InProgress, Ready, Served, Closed];
    public static readonly IReadOnlySet<string> OpenStatuses =
        new HashSet<string>([Created, InProgress, Ready, Served], StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> EditableStatuses =
        new HashSet<string>([Created, InProgress, Ready], StringComparer.Ordinal);

    public static bool CanTransition(string currentStatus, string targetStatus)
    {
        if (string.Equals(currentStatus, targetStatus, StringComparison.Ordinal))
            return false;

        if (string.Equals(targetStatus, Cancelled, StringComparison.Ordinal))
            return OpenStatuses.Contains(currentStatus);

        var currentIndex = Array.IndexOf(Flow, currentStatus);
        var targetIndex = Array.IndexOf(Flow, targetStatus);
        return currentIndex >= 0 && targetIndex == currentIndex + 1;
    }
}
