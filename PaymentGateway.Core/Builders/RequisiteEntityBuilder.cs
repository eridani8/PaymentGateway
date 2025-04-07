using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Builders;

public class RequisiteEntityBuilder
{
    private string? _fullName;
    private RequisiteType _type;
    private string? _paymentData;
    private string? _bankNumber;
    private decimal _maxAmount;
    private int _cooldownMinutes;
    private int _priority;
    private bool _isActive;
    private TimeOnly _workFrom;
    private TimeOnly _workTo;

    public RequisiteEntityBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }
    
    public RequisiteEntityBuilder WithPaymentData(string paymentData)
    {
        _paymentData = paymentData;
        return this;
    }

    public RequisiteEntityBuilder WithBankNumber(string bankNumber)
    {
        _bankNumber = bankNumber;
        return this;
    }
    
    public RequisiteEntityBuilder WithType(RequisiteType type)
    {
        _type = type;
        return this;
    }

    public RequisiteEntityBuilder WithMaxAmount(decimal maxAmount)
    {
        _maxAmount = maxAmount;
        return this;
    }

    public RequisiteEntityBuilder WithCooldownMinutes(int cooldownMinutes)
    {
        _cooldownMinutes = cooldownMinutes;
        return this;
    }

    public RequisiteEntityBuilder WithPriority(int priority)
    {
        _priority = priority;
        return this;
    }

    public RequisiteEntityBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public RequisiteEntityBuilder WithWorkFrom(TimeOnly workFrom)
    {
        _workFrom = workFrom;
        return this;
    }

    public RequisiteEntityBuilder WithWorkTo(TimeOnly workTo)
    {
        _workTo = workTo;
        return this;
    }
    
    public RequisiteEntity Build()
    {
        if (string.IsNullOrEmpty(_fullName))
        {
            throw new ArgumentException("FullName является обязательным полем");
        }
        
        if (string.IsNullOrEmpty(_paymentData))
        {
            throw new ArgumentException("PaymentData является обязательным полем");
        }
        
        if (string.IsNullOrEmpty(_bankNumber))
        {
            throw new ArgumentException("BankNumber является обязательным полем");
        }

        if ((_workFrom != TimeOnly.MinValue && _workTo == TimeOnly.MinValue) || 
            (_workFrom == TimeOnly.MinValue && _workTo != TimeOnly.MinValue))
        {
            throw new ArgumentException("Если задано начало рабочего времени, должно быть задано и окончание, и наоборот");
        }
        
        if (_workFrom != TimeOnly.MinValue && _workTo != TimeOnly.MinValue && _workFrom >= _workTo)
        {
            throw new ArgumentException("Время начала работы должно быть меньше времени окончания");
        }

        return new RequisiteEntity
        {
            Id = Guid.NewGuid(),
            FullName = _fullName,
            PaymentType = _type,
            PaymentData = _paymentData,
            BankNumber = _bankNumber,
            CreatedAt = DateTime.UtcNow,
            Status = _isActive ? RequisiteStatus.Active : RequisiteStatus.Inactive,
            MaxAmount = _maxAmount,
            CooldownMinutes = _cooldownMinutes,
            Priority = _priority,
            WorkFrom = _workFrom,
            WorkTo = _workTo
        };
    }
}