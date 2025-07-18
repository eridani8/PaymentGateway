﻿namespace PaymentGateway.Shared.Constants;

public static class SignalREvents
{
    public static class Web
    {
        public const string WalletUpdated = "WalletUpdated";
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
        public const string GetAllUsers = "GetAllUsers";
        public const string GetChatMessages = "GetChatMessages";
        public const string ChangeRequisiteAssignmentAlgorithm = "ChangeRequisiteAssignmentAlgorithm";
        public const string DeviceUpdated = "DeviceUpdated";
        public const string DeviceDeleted = "DeviceDeleted";
        public const string ChangeUsdtExchangeRate = "ChangeUsdtExchangeRate";
    }

    public static class DeviceApp
    {
        public const string RequestDeviceRegistration = "RequestDeviceRegistration";
        public const string RegisterDevice = "RegisterDevice";
        public const string TransactionReceived = "TransactionReceived";
    }
}