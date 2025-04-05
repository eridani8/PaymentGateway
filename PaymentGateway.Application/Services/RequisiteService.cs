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
    public async Task<RequisiteResponseDto?> CreateRequisite(RequisiteCreateDto dto)
    {
        var validationResult = await validator.CreateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var requisite = new RequisiteEntityBuilder()
            .WithFullName(dto.FullName)
            .WithType(dto.RequisiteType)
            .WithPaymentData(dto.PaymentData)
            .WithBankNumber(dto.BankNumber)
            .WithIsActive(dto.IsActive)
            .WithMaxAmount(SettingsExtensions.GetValueOrDefault(dto.MaxAmount, defaults.Value.MaxAmount))
            .WithCooldownMinutes(SettingsExtensions.GetValueOrDefault(dto.CooldownMinutes, defaults.Value.CooldownMinutes))
            .WithPriority(SettingsExtensions.GetValueOrDefault(dto.Priority, defaults.Value.Priority))
            .Build();

        await unit.RequisiteRepository.Add(requisite);
        await unit.Commit();
        
        logger.LogInformation("Создание реквизита {requisiteId}", requisite.Id);

        return mapper.Map<RequisiteResponseDto>(requisite);
    }

    public async Task<IEnumerable<RequisiteResponseDto>> GetAllRequisites()
    {
        var entities = await unit.RequisiteRepository.GetAll().AsNoTracking().ToListAsync();
        return mapper.Map<IEnumerable<RequisiteResponseDto>>(entities);
    }

    public async Task<RequisiteResponseDto?> GetRequisiteById(Guid id)
    {
        var entity = await unit.RequisiteRepository.GetById(id);
        return entity is not null ? mapper.Map<RequisiteResponseDto>(entity) : null;
    }

    public async Task<bool> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        var validationResult = await validator.UpdateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
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