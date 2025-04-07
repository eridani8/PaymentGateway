using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core;
using PaymentGateway.Core.Builders;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;
using Serilog;

namespace PaymentGateway.Application.Services;

public class RequisiteService(
    IUnitOfWork unit,
    IMapper mapper,
    IRequisiteValidator validator,
    IOptions<RequisiteDefaults> defaults,
    ILogger<RequisiteService> logger) : IRequisiteService
{
    public async Task<RequisiteResponseDto> CreateRequisite(RequisiteCreateDto dto)
    {
        var validation = await validator.CreateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var entity = mapper.Map<RequisiteEntity>(dto);

        await unit.RequisiteRepository.Add(entity);
        await unit.Commit();

        logger.LogInformation("Создание реквизита {requisiteId}", entity.Id);

        return mapper.Map<RequisiteResponseDto>(entity);
    }

    public async Task<IEnumerable<RequisiteResponseDto>> GetAllRequisites()
    {
        var entities = await unit.RequisiteRepository.QueryableGetAll().AsNoTracking().ToListAsync();
        return mapper.Map<IEnumerable<RequisiteResponseDto>>(entities);
    }

    public async Task<RequisiteResponseDto?> GetRequisiteById(Guid id)
    {
        var entity = await unit.RequisiteRepository.GetById(id);
        return entity is not null ? mapper.Map<RequisiteResponseDto>(entity) : null;
    }

    public async Task<bool> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        var validation = await validator.UpdateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }
        
        var entity = await unit.RequisiteRepository.GetById(id);
        if (entity is null) return false;

        mapper.Map(dto, entity);
        unit.RequisiteRepository.Update(entity);
        await unit.Commit();
        
        logger.LogInformation("Обновление реквизита {requisiteId}", entity.Id);

        return true;
    }

    public async Task<bool> DeleteRequisite(Guid id)
    {
        var entity = await unit.RequisiteRepository.GetById(id);
        if (entity is null) return false;

        unit.RequisiteRepository.Delete(entity);
        await unit.Commit();
        
        logger.LogInformation("Удаление реквизита {requisiteId}", entity.Id);

        return true;
    }
}