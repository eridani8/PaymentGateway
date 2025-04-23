namespace PaymentGateway.Shared;

public static class SignalREvents
{
    public const string RequisiteUpdated = "RequisiteUpdated";
    public const string RequisiteDeleted = "RequisiteDeleted";
    public const string UserUpdated = "UserUpdated";
    public const string UserDeleted = "UserDeleted";
    public const string PaymentUpdated = "PaymentUpdated";
    public const string PaymentDeleted = "PaymentDeleted";
    public const string TransactionUpdated = "TransactionUpdated";
    public const string ChatMessageReceived = "ChatMessageReceived";
    public const string UserConnected = "UserConnected";
    public const string UserDisconnected = "UserDisconnected";
    public const string SendChatMessage = "SendChatMessage";
    public const string GetAdminsAndSupports = "GetAdminsAndSupports";
    public const string GetChatMessages = "GetChatMessages";
    public const string ChangeRequisiteAssignmentAlgorithm = "ChangeRequisiteAssignmentAlgorithm";
}