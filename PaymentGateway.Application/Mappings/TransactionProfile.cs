using AutoMapper;
using PaymentGateway.Application.DTOs.Transaction;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Mappings;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<TransactionEntity, TransactionResponseDto>();
        CreateMap<TransactionCreateDto, TransactionEntity>();
    }
}