using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Builders;

public class RequisiteEntityBuilder
{
    private RequisiteType _type;
    private string? _paymentData;
    private string? _fullName;
    private decimal _maxAmount;
    private int _cooldownMinutes;
    private int _priority;
    private bool _isActive;
    
    public RequisiteEntityBuilder WithType(RequisiteType type)
    {
        _type = type;
        return this;
    }

    public RequisiteEntityBuilder WithPaymentData(string paymentData)
    {
        _paymentData = paymentData;
        return this;
    }

    public RequisiteEntityBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
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
    
    public RequisiteEntity Build()
    {
        if (string.IsNullOrEmpty(_paymentData))
        {
            throw new InvalidOperationException("PaymentData является обязательным полем");
        }

        if (string.IsNullOrEmpty(_fullName))
        {
            throw new InvalidOperationException("FullName является обязательным полем");
        }

        return new RequisiteEntity(
            _type,
            _paymentData,
            _fullName,
            _isActive,
            _maxAmount,
            _cooldownMinutes,
            _priority);
    }
}