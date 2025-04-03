using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Validators.Requisite;
using PaymentGateway.Core;
using PaymentGateway.Core.Builders;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class RequisiteService(
    IUnitOfWork unit,
    IMapper mapper,
    RequisiteValidator validator,
    IOptions<RequisiteDefaults> defaults) : IRequisiteService
{
    public async Task<RequisiteEntity?> GetFreeRequisite()
    {
        return await unit.RequisiteRepository
            .GetAll()
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .FirstOrDefaultAsync();
    }

    public async Task<RequisiteResponseDto> CreateRequisite(RequisiteCreateDto dto)
    {
        var validationResult = await validator.CreateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(", ",
                validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var entity = new RequisiteEntityBuilder()
            .WithType(dto.Type)
            .WithPaymentData(dto.PaymentData)
            .WithFullName(dto.FullName)
            .WithMaxAmount(SettingsHelper.GetValueOrDefault(dto.MaxAmount, defaults.Value.MaxAmount))
            .WithCooldownMinutes(SettingsHelper.GetValueOrDefault(dto.CooldownMinutes, defaults.Value.CooldownMinutes))
            .WithPriority(SettingsHelper.GetValueOrDefault(dto.Priority, defaults.Value.Priority))
            .Build();

        await unit.RequisiteRepository.Add(entity);
        await unit.Commit();

        return mapper.Map<RequisiteResponseDto>(entity);
    }

    public async Task<IEnumerable<RequisiteResponseDto>> GetAllRequisites()
    {
        var entities = await unit.RequisiteRepository.GetAll().ToListAsync();
        return mapper.Map<IEnumerable<RequisiteResponseDto>>(entities);
    }

    public async Task<RequisiteResponseDto?> GetRequisiteById(Guid id)
    {
        var entity = await unit.RequisiteRepository.GetById(id);
        return entity != null ? mapper.Map<RequisiteResponseDto>(entity) : null;
    }

    public async Task<bool> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        var validationResult = await validator.UpdateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(Environment.NewLine,
                validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var entity = await unit.RequisiteRepository.GetById(id);
        if (entity == null) return false;

        mapper.Map(dto, entity);
        unit.RequisiteRepository.Update(entity);
        await unit.Commit();

        return true;
    }

    public async Task<bool> DeleteRequisite(Guid id)
    {
        var entity = await unit.RequisiteRepository.GetById(id);
        if (entity == null) return false;

        unit.RequisiteRepository.Delete(entity);
        await unit.Commit();

        return true;
    }
}