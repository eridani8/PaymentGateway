using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
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
    public async Task<Result<RequisiteDto>> CreateRequisite(RequisiteCreateDto dto, Guid userId)
    {
        var validation = await createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<RequisiteDto>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage)));
        }
        
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure<RequisiteDto>(Error.UserNotFound);
        }

        var userRequisitesCount = await unit.RequisiteRepository.GetUserRequisitesCount(userId);
        if (userRequisitesCount >= user.MaxRequisitesCount)
        {
            return Result.Failure<RequisiteDto>(Error.RequisiteLimitExceeded(user.MaxRequisitesCount));
        }
        
        var containsRequisite = await unit.RequisiteRepository.HasSimilarRequisite(dto.PaymentData);
        if (containsRequisite is not null)
        {
            return Result.Failure<RequisiteDto>(Error.DuplicateRequisite);
        }

        var requisite = mapper.Map<RequisiteEntity>(dto, opts => 
        {
            opts.Items["UserId"] = userId;
        });
        
        requisite.User = user;
        
        await unit.RequisiteRepository.Add(requisite);
        await unit.Commit();

        user.RequisitesCount++;
        await userManager.UpdateAsync(user);

        var requisiteDto = mapper.Map<RequisiteDto>(requisite);
        
        await notificationService.NotifyRequisiteUpdated(requisiteDto);
        await notificationService.NotifyUserUpdated(mapper.Map<UserDto>(user));

        return Result.Success(requisiteDto);
    }

    public async Task<Result<IEnumerable<RequisiteDto>>> GetAllRequisites()
    {
        var entities = await unit.RequisiteRepository.GetAllRequisites();
        var dtos = mapper.Map<IEnumerable<RequisiteDto>>(entities);
        return Result.Success(dtos);
    }

    public async Task<Result<IEnumerable<RequisiteDto>>> GetUserRequisites(Guid userId)
    {
        var entities = await unit.RequisiteRepository.GetUserRequisites(userId);
        var dtos = mapper.Map<IEnumerable<RequisiteDto>>(entities);
        return Result.Success(dtos);
    }

    public async Task<Result<RequisiteDto>> GetRequisiteById(Guid id)
    {
        var requisite = await unit.RequisiteRepository.GetRequisiteById(id);
        if (requisite is null) 
            return Result.Failure<RequisiteDto>(Error.RequisiteNotFound);
            
        var dto = mapper.Map<RequisiteDto>(requisite);
        return Result.Success(dto);
    }

    public async Task<Result<RequisiteDto>> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        var validation = await updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<RequisiteDto>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage)));
        }

        var requisite = await unit.RequisiteRepository.GetRequisiteById(id);
        if (requisite is null) return Result.Failure<RequisiteDto>(Error.RequisiteNotFound);
        
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

        return Result.Success(requisiteDto);
    }

    public async Task<Result<RequisiteDto>> DeleteRequisite(Guid id)
    {
        var requisite = await unit.RequisiteRepository.GetRequisiteById(id);
        if (requisite is null) return Result.Failure<RequisiteDto>(Error.RequisiteNotFound);

        try
        {
            requisite.User.RequisitesCount--;
            await userManager.UpdateAsync(requisite.User);

            var userId = requisite.UserId;
            var requisiteDto = mapper.Map<RequisiteDto>(requisite);

            unit.RequisiteRepository.Delete(requisite);
            await unit.Commit();

            await notificationService.NotifyRequisiteDeleted(id, userId);
            await notificationService.NotifyUserUpdated(mapper.Map<UserDto>(requisite.User));

            return Result.Success(requisiteDto);
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
        {
            logger.LogWarning("Невозможно удалить реквизит {RequisiteId}, так как он используется в других таблицах", id);
            return Result.Failure<RequisiteDto>(Error.OperationFailed("Невозможно удалить реквизит, так как он используется в платежах"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении реквизита {RequisiteId}", id);
            return Result.Failure<RequisiteDto>(Error.OperationFailed(ex.Message));
        }
    }
}