namespace PaymentGateway.Application.Results;

public enum ErrorCode
{
    None,
    NullValue,
    Validation,
    
    NotFound,
    AlreadyExists,
    OperationFailed,
    
    UserNotFound,
    UserAlreadyExists,
    UserCreationFailed,
    UserUpdateFailed,
    UserDeleteFailed,
    
    AccessDenied,
    DeleteRootUserForbidden,
    SelfDeleteForbidden,
    ModifyRootUserForbidden,
    
    PaymentNotFound,
    DuplicatePayment,
    PaymentAlreadyConfirmed,
    RequisiteNotAttached,
    InsufficientPermissions,
    InsufficientPermissionsForPayment,
    
    RequisiteNotFound,
    DuplicateRequisite,
    RequisiteLimitExceeded,
    
    TransactionError,
    WrongPaymentAmount
} 