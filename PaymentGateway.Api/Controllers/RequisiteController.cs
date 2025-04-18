﻿using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.Requisite;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
[Authorize]
public class RequisiteController(IRequisiteService service, ILogger<RequisiteController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RequisiteDto>> Create([FromBody] RequisiteCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var userId = User.GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var requisite = await service.CreateRequisite(dto, userId);
            if (requisite is null) return BadRequest();
            logger.LogInformation("Создание реквизита {requisiteId} [{UserId}]", requisite.Id, User.GetCurrentUserId());
            return Ok(requisite);
        }
        catch (DuplicateRequisiteException e)
        {
            return BadRequest(e.Message);
        }
        catch (RequisiteLimitExceededException e)
        {
            return BadRequest(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<RequisiteDto>>> GetAll()
    {
        var requisites = await service.GetAllRequisites();
        return Ok(requisites);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RequisiteDto>>> GetUserRequisites()
    {
        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var requisites = await service.GetUserRequisites(userId);
        return Ok(requisites);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequisiteDto>> GetById(Guid id)
    {
        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var requisite = await service.GetRequisiteById(id, userId);
        if (requisite is null) return NotFound();
        return Ok(requisite);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RequisiteDto>> Update(Guid id, [FromBody] RequisiteUpdateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var requisite = await service.UpdateRequisite(id, dto);
            if (requisite is null) return BadRequest();
            logger.LogInformation("Обновление реквизита {requisiteId} [{UserId}]", requisite.Id, User.GetCurrentUserId());
            return Ok(requisite);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequisiteDto>> Delete(Guid id)
    {
        var requisite = await service.DeleteRequisite(id);
        if (requisite is null) return NotFound();
        logger.LogInformation("Удаление реквизита {requisiteId} [{UserId}]", id, User.GetCurrentUserId());
        return Ok(requisite);
    }
}