﻿using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.Payment;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class PaymentController(
    IPaymentService service)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var payment = await service.CreatePayment(dto);
            if (payment is null) return BadRequest();
            return Ok(payment);
        }
        catch (DuplicatePaymentException)
        {
            return Conflict();
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> ManualConfirmPayment([FromBody] PaymentManualConfirmDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var userId = User.GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var payment = await service.ManualConfirmPayment(dto, userId);
            if (payment is null) return BadRequest();
            return Ok(payment);
        }
        catch (PaymentNotFound)
        {
            return NotFound();
        }
        catch (ManualConfirmException e)
        {
            return BadRequest(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin,Support")]
    public async Task<ActionResult<PaymentDto>> CancelPayment([FromBody] PaymentCancelDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var userId = User.GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var payment = await service.CancelPayment(dto, userId);
            if (payment is null) return BadRequest();
            return Ok(payment);
        }
        catch (PaymentNotFound)
        {
            return NotFound();
        }
        catch (ManualConfirmException e)
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
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var payments = await service.GetAllPayments();
        return Ok(payments);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments()
    {
        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var payments = await service.GetUserPayments(userId);
        return Ok(payments);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id)
    {
        var payment = await service.GetPaymentById(id);
        if (payment is null) return NotFound();
        return Ok(payment);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> Delete(Guid id)
    {
        var payment = await service.DeletePayment(id);
        if (payment is null) return NotFound();
        return Ok(payment);
    }
}