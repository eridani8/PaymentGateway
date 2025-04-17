namespace PaymentGateway.Shared.Enums;

public enum PaymentStatus
{
    Created = 0,
    Pending = 1,
    Confirmed = 2,
    ManualConfirm = 3,
    Expired = 4,
    Canceled = 5
}