using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Validators.Requisite;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class RequisiteController(RequisiteService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RequisiteResponseDto>> Create([FromBody] RequisiteCreateDto? dto)
    {
        if (dto == null) return BadRequest("Неверные данные");

        try
        {
            var createdRequisite = await service.CreateRequisite(dto);
            
            return CreatedAtRoute(
                "GetById",
                new { id = createdRequisite.Id },
                createdRequisite);
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

        if (requisite == null)
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
    
    [HttpGet("{id:guid}", Name = "GetById")]
    public async Task<ActionResult<RequisiteResponseDto>> GetById(Guid id)
    {
        var requisite = await service.GetRequisiteById(id);
        if (requisite == null)
        {
            return NotFound("Реквизит с таким ID не найден");
        }
        
        return Ok(requisite);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] RequisiteUpdateDto? dto)
    {
        if (dto == null) return BadRequest("Неверные данные");

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