﻿using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.Requisite;
using System.Security.Claims;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
[Authorize]
public class RequisiteController(IRequisiteService service) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
    }

    [HttpPost]
    public async Task<ActionResult<RequisiteDto>> Create([FromBody] RequisiteCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var userId = GetCurrentUserId();
            var requisite = await service.CreateRequisite(dto, userId);
            return Ok(requisite);
        }
        catch (DuplicateRequisiteException e)
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
        var userId = GetCurrentUserId();
        var requisites = await service.GetUserRequisites(userId);
        return Ok(requisites);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequisiteDto>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var requisite = await service.GetRequisiteById(id, userId);
        if (requisite is null)
        {
            return NotFound();
        }
        
        return Ok(requisite);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] RequisiteUpdateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var result = await service.UpdateRequisite(id, dto);
            if (!result)
            {
                return BadRequest();
            }
            
            return Ok();
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await service.DeleteRequisite(id);
        if (!result)
        {
            return NotFound();
        }
        
        return Ok();
    }
}