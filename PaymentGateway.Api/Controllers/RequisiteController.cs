using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Application.Interfaces;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class RequisiteController(IRequisiteService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RequisiteResponseDto>> Create([FromBody] RequisiteCreateDto? dto)
    {
        if (dto is null) return BadRequest("Неверные данные");

        try
        {
            var requisite = await service.CreateRequisite(dto);

            return Ok(requisite);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors);
        }
    }

    [HttpGet]
    public async Task<ActionResult<RequisiteResponseDto>> GetFree()
    {
        var requisite = await service.GetFreeRequisite();

        if (requisite is null)
        {
            return NotFound("Нет свободного реквизита");
        }
        
        return Ok(requisite);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RequisiteResponseDto>>> GetAll()
    {
        var requisites = await service.GetAllRequisites();
        
        return Ok(requisites);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequisiteResponseDto>> GetById(Guid id)
    {
        var requisite = await service.GetRequisiteById(id);
        if (requisite is null)
        {
            return NotFound("Реквизит с таким ID не найден");
        }
        
        return Ok(requisite);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] RequisiteUpdateDto? dto)
    {
        if (dto is null) return BadRequest("Неверные данные");

        try
        {
            var result = await service.UpdateRequisite(id, dto);
            if (!result)
            {
                return NotFound("Реквизит с таким ID не найден");
            }
            
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await service.DeleteRequisite(id);
        if (!result)
        {
            return NotFound("Реквизит с таким ID не найден");
        }
        
        return NoContent();
    }
}