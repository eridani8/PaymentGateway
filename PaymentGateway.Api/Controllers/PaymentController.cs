using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Application.Interfaces;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class PaymentController(IPaymentService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PaymentResponseDto>> Create([FromBody] PaymentCreateDto? dto)
    {
        if (dto is null) return BadRequest("Неверные данные");

        try
        {
            var payment = await service.CreatePayment(dto);

            if (payment is null)
            {
                return Conflict("Платеж с таким ID уже существует");
            }

            return Ok(payment.Id);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetAll()
    {
        var requisites = await service.GetAllPayments();
        
        return Ok(requisites);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponseDto>> GetById(Guid id)
    {
        var requisite = await service.GetPaymentById(id);
        if (requisite is null)
        {
            return NotFound("Платеж с таким ID не найден");
        }
        
        return Ok(requisite);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await service.DeletePayment(id);
        if (!result)
        {
            return NotFound("Платеж с таким ID не найден");
        }
        
        return NoContent();
    }
}