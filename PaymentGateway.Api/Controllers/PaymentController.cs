using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
[Authorize]
public class PaymentController(IPaymentService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var payment = await service.CreatePayment(dto);
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
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var requisites = await service.GetAllPayments();
        
        return Ok(requisites);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id)
    {
        var requisite = await service.GetPaymentById(id);
        if (requisite is null)
        {
            return NotFound();
        }
        
        return Ok(requisite);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await service.DeletePayment(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}