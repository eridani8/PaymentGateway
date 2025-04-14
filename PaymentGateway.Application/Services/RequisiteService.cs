using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Application.Services;

public class RequisiteService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<RequisiteCreateDto> createValidator,
    IValidator<RequisiteUpdateDto> updateValidator,
    ILogger<RequisiteService> logger) : IRequisiteService
{
    public async Task<RequisiteDto?> CreateRequisite(RequisiteCreateDto dto, Guid userId)
    {
        var validation = await createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }
        
        var requisites = await unit.RequisiteRepository.GetAll();
        var containsRequisite = requisites.FirstOrDefault(r => r.PaymentData.Equals(dto.PaymentData));
        if (containsRequisite is not null)
        {
            throw new DuplicateRequisiteException("Реквизит с такими платежными данными уже существует");
        }

        var requisite = mapper.Map<RequisiteEntity>(dto, opts => 
        {
            opts.Items["UserId"] = userId;
        });
        
        await unit.RequisiteRepository.Add(requisite);
        await unit.Commit();

        logger.LogInformation("Создание реквизита {requisiteId}", requisite.Id);

        return mapper.Map<RequisiteDto>(requisite);
    }

    public async Task<IEnumerable<RequisiteDto>> GetAllRequisites()
    {
        var entities = await unit.RequisiteRepository.QueryableGetAll()
            .AsNoTracking()
            .ToListAsync();
            
        return mapper.Map<IEnumerable<RequisiteDto>>(entities);
    }

    public async Task<IEnumerable<RequisiteDto>> GetUserRequisites(Guid userId)
    {
        var entities = await unit.RequisiteRepository.QueryableGetAll()
            .Where(r => r.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
            
        return mapper.Map<IEnumerable<RequisiteDto>>(entities);
    }

    public async Task<RequisiteDto?> GetRequisiteById(Guid id, Guid userId)
    {
        var requisite = await unit.RequisiteRepository.QueryableGetAll()
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            
        if (requisite is null) return null;
        
        return mapper.Map<RequisiteDto>(requisite);
    }

    public async Task<RequisiteDto?> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        var validation = await updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }
        
        var requisite = await unit.RequisiteRepository.GetById(id);
        if (requisite is null) return null;
        if (requisite.Status == RequisiteStatus.Pending) return null;

        mapper.Map(dto, requisite);
        unit.RequisiteRepository.Update(requisite);
        await unit.Commit();
        
        logger.LogInformation("Обновление реквизита {requisiteId}", requisite.Id);

        return mapper.Map<RequisiteDto>(requisite);
    }

    public async Task<RequisiteDto?> DeleteRequisite(Guid id)
    {
        var requisite = await unit.RequisiteRepository.GetById(id);
        if (requisite is null) return null;

        unit.RequisiteRepository.Delete(requisite);
        await unit.Commit();
        
        logger.LogInformation("Удаление реквизита {requisiteId}", requisite.Id);

        return mapper.Map<RequisiteDto>(requisite);
    }
}