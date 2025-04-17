using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class RequisiteService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<RequisiteCreateDto> createValidator,
    IValidator<RequisiteUpdateDto> updateValidator,
    ILogger<RequisiteService> logger,
    UserManager<UserEntity> userManager,
    INotificationService notificationService) : IRequisiteService
{
    public async Task<RequisiteDto?> CreateRequisite(RequisiteCreateDto dto, Guid userId)
    {
        var validation = await createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }
        
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }
        
        var userRequisitesCount = await unit.RequisiteRepository.QueryableGetAll()
            .CountAsync(r => r.UserId == userId);
            
        if (userRequisitesCount >= user.MaxRequisitesCount)
        {
            throw new RequisiteLimitExceededException($"Достигнут лимит реквизитов. Максимум: {user.MaxRequisitesCount}");
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

        user.RequisitesCount++;
        await userManager.UpdateAsync(user);

        var requisiteDto = mapper.Map<RequisiteDto>(requisite);
        
        await notificationService.NotifyRequisiteUpdated(requisiteDto);
        await notificationService.NotifyUserUpdated(mapper.Map<UserDto>(user));

        logger.LogInformation("Создание реквизита {requisiteId}", requisite.Id);

        return requisiteDto;
    }

    public async Task<IEnumerable<RequisiteDto>> GetAllRequisites()
    {
        var entities = await unit.RequisiteRepository.QueryableGetAll()
            .Include(r => r.Payment)
            .AsNoTracking()
            .ToListAsync();
        return mapper.Map<IEnumerable<RequisiteDto>>(entities);
    }

    public async Task<IEnumerable<RequisiteDto>> GetUserRequisites(Guid userId)
    {
        var entities = await unit.RequisiteRepository.QueryableGetAll()
            .Include(r => r.Payment)
            .Where(r => r.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
        return mapper.Map<IEnumerable<RequisiteDto>>(entities);
    }

    public async Task<RequisiteDto?> GetRequisiteById(Guid id, Guid userId)
    {
        var requisite = await unit.RequisiteRepository.QueryableGetAll()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        return requisite is null ? null : mapper.Map<RequisiteDto>(requisite);
    }

    public async Task<RequisiteDto?> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        var validation = await updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }
        
        var requisite = await unit.RequisiteRepository.GetById(id,
            r => r.User,
            r => r.Payment);
        if (requisite is null) return null;
        
        var now = DateTime.UtcNow;
        var nowTimeOnly = TimeOnly.FromDateTime(now);
        if (requisite.ProcessStatus(now, nowTimeOnly, out var status))
        {
            logger.LogInformation("Статус реквизита {requisiteId} изменен с {oldStatus} на {newStatus}", requisite.Id, requisite.Status.ToString(), status.ToString());
            requisite.Status = status;
        }

        mapper.Map(dto, requisite);
        
        unit.RequisiteRepository.Update(requisite);
        await unit.Commit();
        
        var requisiteDto = mapper.Map<RequisiteDto>(requisite);
        
        await notificationService.NotifyRequisiteUpdated(requisiteDto);
        logger.LogInformation("Обновление реквизита {requisiteId}", requisite.Id);

        return requisiteDto;
    }

    public async Task<RequisiteDto?> DeleteRequisite(Guid id)
    {
        var requisite = await unit.RequisiteRepository.GetById(id, r => r.User);
        if (requisite is null) return null;

        requisite.User.RequisitesCount--;
        await userManager.UpdateAsync(requisite.User);

        var userId = requisite.UserId;
        var requisiteDto = mapper.Map<RequisiteDto>(requisite);

        unit.RequisiteRepository.Delete(requisite);
        await unit.Commit();

        await notificationService.NotifyRequisiteDeleted(id, userId);
        await notificationService.NotifyUserUpdated(mapper.Map<UserDto>(requisite.User));

        logger.LogInformation("Удаление реквизита {requisiteId}", id);

        return requisiteDto;
    }
}