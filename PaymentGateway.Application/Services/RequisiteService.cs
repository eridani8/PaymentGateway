using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class RequisiteService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<RequisiteCreateDto> createValidator,
    IValidator<RequisiteUpdateDto> updateValidator,
    ILogger<RequisiteService> logger) : IRequisiteService
{
    public async Task<RequisiteDto> CreateRequisite(RequisiteCreateDto dto)
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

        var entity = mapper.Map<RequisiteEntity>(dto);
        
        await unit.RequisiteRepository.Add(entity);
        await unit.Commit();

        logger.LogInformation("Создание реквизита {requisiteId}", entity.Id);

        return mapper.Map<RequisiteDto>(entity);
    }

    public async Task<IEnumerable<RequisiteDto>> GetAllRequisites()
    {
        var entities = await unit.RequisiteRepository.QueryableGetAll().AsNoTracking().ToListAsync();
        return mapper.Map<IEnumerable<RequisiteDto>>(entities);
    }

    public async Task<RequisiteDto?> GetRequisiteById(Guid id)
    {
        var entity = await unit.RequisiteRepository.GetById(id);
        return entity is not null ? mapper.Map<RequisiteDto>(entity) : null;
    }

    public async Task<bool> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        var validation = await updateValidator.ValidateAsync(dto);
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