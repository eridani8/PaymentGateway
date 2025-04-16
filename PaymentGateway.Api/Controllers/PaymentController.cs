using FluentValidation;
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
    IPaymentService paymentService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var payment = await paymentService.CreatePayment(dto);
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
            var payment = await paymentService.ManualConfirmPayment(dto, userId);
            if (payment is null) return BadRequest();
            return Ok(payment);
        }
        catch (PaymentNotFound)
        {
            return NotFound();
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var requisites = await paymentService.GetAllPayments();
        
        return Ok(requisites);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id)
    {
        var payment = await paymentService.GetPaymentById(id);
        if (payment is null) return NotFound();
        return Ok(payment);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> Delete(Guid id)
    {
        var payment = await paymentService.DeletePayment(id);
        if (payment is null) return NotFound();
        return Ok(payment);
    }
}