using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Builders;

public class RequisiteEntityBuilder
{
    private string? _fullName;
    private string? _phoneNumber;
    private string? _cardNumber;
    private string? _bankAccountNumber;
    private decimal _maxAmount;
    private int _cooldownMinutes;
    private int _priority;
    private bool _isActive;
    
    public RequisiteEntityBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }
    
    public RequisiteEntityBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }
    
    public RequisiteEntityBuilder WithCardNumber(string cardNumber)
    {
        _cardNumber = cardNumber;
        return this;
    }
    
    public RequisiteEntityBuilder WithBankAccountNumber(string bankAccountNumber)
    {
        _bankAccountNumber = bankAccountNumber;
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
        if (string.IsNullOrEmpty(_fullName))
        {
            throw new InvalidOperationException("FullName является обязательным полем");
        }
        
        if (string.IsNullOrEmpty(_phoneNumber))
        {
            throw new InvalidOperationException("PhoneNumber является обязательным полем");
        }
        
        if (string.IsNullOrEmpty(_cardNumber))
        {
            throw new InvalidOperationException("CardNumber является обязательным полем");
        }
        
        if (string.IsNullOrEmpty(_bankAccountNumber))
        {
            throw new InvalidOperationException("BankAccountNumber является обязательным полем");
        }

        return new RequisiteEntity()
        {
            Id = Guid.NewGuid(),
            FullName = _fullName,
            PhoneNumber = _phoneNumber,
            CardNumber = _cardNumber,
            BankAccountNumber = _bankAccountNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = _isActive,
            MaxAmount = _maxAmount,
            CooldownMinutes = _cooldownMinutes,
            Priority = _priority,
        };
    }
}