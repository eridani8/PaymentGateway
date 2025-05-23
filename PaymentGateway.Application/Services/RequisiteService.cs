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
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;

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
        
        var requisite = mapper.Map<RequisiteEntity>(dto, opts => { opts.Items["UserId"] = userId; });
        
        requisite.User = user;
        
        var deviceDto = DeviceHub.DeviceByIdAndUserId(dto.DeviceId, userId);

        if (deviceDto is null)
        {
            return Result.Failure<RequisiteDto>(DeviceErrors.DeviceShouldBeOnline);
        }

        var device = await unit.DeviceRepository.GetDeviceById(deviceDto.Id);
        var hasDeviceInBase = false;
        if (device is null)
        {
            deviceDto.BindingAt = DateTime.UtcNow;
            deviceDto.RequisiteId = requisite.Id;
            device = mapper.Map<DeviceEntity>(deviceDto);
        }
        else
        {
            device.BindingAt = DateTime.UtcNow;
            device.RequisiteId = requisite.Id;
            hasDeviceInBase = true;
        }
        
        requisite.DeviceId = device.Id;

        if (!hasDeviceInBase)
        {
            await unit.DeviceRepository.Add(device);
        }
        else
        {
            unit.DeviceRepository.Update(device);
        }

        logger.LogInformation("Устройство {DeviceId} привязано к реквизиту {RequisiteId} пользователя {UserId}", device.Id, requisite.Id, requisite.UserId);

        await unit.RequisiteRepository.Add(requisite);
        await unit.Commit();
        
        var requisiteDto = mapper.Map<RequisiteDto>(requisite);

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
        // var sourceDeviceId = requisite.DeviceId;
        
        requisite = mapper.Map(dto, requisite);

        // if (requisite.DeviceId != null && sourceDeviceId != dto.DeviceId)
        // {
        //     var deviceDto = DeviceHub.DeviceByIdAndUserId(dto.DeviceId, requisite.UserId);
        //     
        //     if (deviceDto is null)
        //     {
        //         return Result.Failure<RequisiteDto>(DeviceErrors.DeviceShouldBeOnline);
        //     }
        //     
        //     var device = await unit.DeviceRepository.GetDeviceById(deviceDto.Id);
        //     var hasDeviceInBase = false;
        //     if (device is null)
        //     {
        //         deviceDto.BindingAt = DateTime.UtcNow;
        //         deviceDto.RequisiteId = requisite.Id;
        //         device = mapper.Map<DeviceEntity>(deviceDto);
        //     }
        //     else
        //     {
        //         device.BindingAt = DateTime.UtcNow;
        //         device.RequisiteId = requisite.Id;
        //         hasDeviceInBase = true;
        //     }
        //     
        //     requisite.DeviceId = device.Id;
        //     
        //     if (!hasDeviceInBase)
        //     {
        //         await unit.DeviceRepository.Add(device);
        //     }
        //     else
        //     {
        //         unit.DeviceRepository.Update(device);
        //     }
        //     logger.LogInformation("Устройство {DeviceId} привязано к реквизиту {RequisiteId} пользователя {UserId}", device.Id, requisite.Id, requisite.UserId);
        //
        //     if (sourceDeviceId != null && sourceDeviceId != dto.DeviceId)
        //     {
        //         var oldDevice = await unit.DeviceRepository.GetDeviceById(sourceDeviceId.Value);
        //         if (oldDevice != null)
        //         {
        //             oldDevice.RequisiteId = null;
        //             oldDevice.BindingAt = DateTime.MinValue;
        //             unit.DeviceRepository.Update(oldDevice);
        //             logger.LogInformation("Устройство {DeviceId} отвязано от реквизита {RequisiteId} пользователя {UserId}", oldDevice.Id, requisite.Id, requisite.UserId);
        //         }
        //     }
        // } // TODO device
        
        requisite.Status = RequisiteStatus.Frozen;
        
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
            var device = requisite.Device;
            var user = requisite.User;

            if (device is not null)
            {
                device.BindingAt = DateTime.MinValue;
                device.RequisiteId = null;
                unit.DeviceRepository.Update(device);
                logger.LogInformation("Устройство {DeviceId} отвязано от реквизита {RequisiteId} пользователя {UserId}", device.Id, requisite.Id, userId);
            }
            
            unit.RequisiteRepository.Delete(requisite);
            await unit.Commit();

            user.RequisitesCount--;
            await userManager.UpdateAsync(user);

            if (device is not null)
            {
                var deviceDto = DeviceHub.DeviceByIdAndUserId(device.Id, userId);
                if (deviceDto is not null)
                {
                    deviceDto.BindingAt = DateTime.MinValue;
                    deviceDto.RequisiteId = null;
                }
            }

            await notificationService.NotifyRequisiteDeleted(id, userId);
            await notificationService.NotifyUserUpdated(mapper.Map<UserDto>(user));

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