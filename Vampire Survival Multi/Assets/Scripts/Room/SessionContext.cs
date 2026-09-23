public enum RoomExitReason
{
    None,
    UserExit,
    Kicked,
    Timeout,
}

public static class SessionContext
{
    public static RoomExitReason LastExitResson { get; private set; }

    public static void SetExitReason(RoomExitReason reason) => LastExitResson = reason;
    public static void ClearReason() => LastExitResson = RoomExitReason.None;
}