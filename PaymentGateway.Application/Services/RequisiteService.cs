using AutoMapper;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class RequisiteService(IRequisiteRepository repository, IMapper mapper) : IRequisiteService
{
    public Task<RequisiteResponseDto> CreateRequisite(RequisiteCreateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<RequisiteResponseDto>> GetAllRequisites()
    {
        throw new NotImplementedException();
    }

    public Task<RequisiteResponseDto?> GetRequisiteById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteRequisite(Guid id)
    {
        throw new NotImplementedException();
    }
}