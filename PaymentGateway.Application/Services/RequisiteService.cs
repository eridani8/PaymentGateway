using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using PaymentGateway.Application.Hubs;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;

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
        if (user is null)
        {
            return Result.Failure<RequisiteDto>(UserErrors.UserNotFound);
        }

        var userRequisitesCount = await unit.RequisiteRepository.GetUserRequisitesCount(userId);
        if (userRequisitesCount >= user.MaxRequisitesCount)
        {
            return Result.Failure<RequisiteDto>(RequisiteErrors.RequisiteLimitExceeded(user.MaxRequisitesCount));
        }

        var containsRequisite = await unit.RequisiteRepository.HasSimilarRequisite(dto.PaymentData);
        if (containsRequisite is not null)
        {
            return Result.Failure<RequisiteDto>(RequisiteErrors.DuplicateRequisite);
        }

        var deviceDto = DeviceHub.Devices.Values
            .FirstOrDefault(d =>
                d.UserId == userId &&
                d.BindingAt == DateTime.MinValue &&
                d.Requisite is null);

        if (deviceDto is null)
        {
            return Result.Failure<RequisiteDto>(DeviceErrors.BindingError);
        }

        var device = mapper.Map<DeviceEntity>(deviceDto);
        
        var requisite = mapper.Map<RequisiteEntity>(dto, opts => { opts.Items["UserId"] = userId; });
        
        requisite.User = user;
        requisite.DeviceId = device.Id;

        device.RequisiteId = requisite.Id;

        await unit.DeviceRepository.Add(device);
        await unit.RequisiteRepository.Add(requisite);
        await unit.Commit();
        
        var requisiteDto = mapper.Map<RequisiteDto>(requisite);

        deviceDto.Requisite = requisiteDto;

        user.RequisitesCount++;
        await userManager.UpdateAsync(user);
        
        var userDto = mapper.Map<UserDto>(user);

        await notificationService.NotifyRequisiteUpdated(requisiteDto);
        await notificationService.NotifyUserUpdated(userDto);

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
        if (requisite is null) return Result.Failure<RequisiteDto>(RequisiteErrors.RequisiteNotFound);

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
        if (requisite is null) return Result.Failure<RequisiteDto>(RequisiteErrors.RequisiteNotFound);

        var now = DateTime.UtcNow;
        var nowTimeOnly = TimeOnly.FromDateTime(now);
        if (requisite.ProcessStatus(now, nowTimeOnly, out var status))
        {
            logger.LogInformation("Статус реквизита {RequisiteId} изменен с {OldStatus} на {NewStatus}", requisite.Id,
                requisite.Status.ToString(), status.ToString());
            requisite.Status = status;
        }

        requisite = mapper.Map(dto, requisite);

        unit.RequisiteRepository.Update(requisite);
        await unit.Commit();

        var requisiteDto = mapper.Map<RequisiteDto>(requisite);

        await notificationService.NotifyRequisiteUpdated(requisiteDto);

        return Result.Success(requisiteDto);
    }

    public async Task<Result<RequisiteDto>> DeleteRequisite(Guid id)
    {
        var requisite = await unit.RequisiteRepository.GetRequisiteById(id);
        if (requisite is null) return Result.Failure<RequisiteDto>(RequisiteErrors.RequisiteNotFound);

        try
        {
            var userId = requisite.UserId;
            var requisiteDto = mapper.Map<RequisiteDto>(requisite);

            unit.RequisiteRepository.Delete(requisite);
            await unit.Commit();

            requisite.User.RequisitesCount--;
            await userManager.UpdateAsync(requisite.User);

            await notificationService.NotifyRequisiteDeleted(id, userId);
            await notificationService.NotifyUserUpdated(mapper.Map<UserDto>(requisite.User));

            return Result.Success(requisiteDto);
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23503" })
        {
            return Result.Failure<RequisiteDto>(
                Error.OperationFailed("Невозможно удалить реквизит, так как он используется в платежах"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении реквизита {RequisiteId}", id);
            return Result.Failure<RequisiteDto>(Error.OperationFailed(ex.Message));
        }
    }
}