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
        if (dto == null) return BadRequest("Invalid data");

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
        catch (Exception e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RequisiteResponseDto>>> GetAll()
    {
        try
        {
            var requisites = await service.GetAllRequisites();
            return Ok(requisites);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("{id:guid}", Name = "GetById")]
    public async Task<ActionResult<RequisiteResponseDto>> GetById(Guid id)
    {
        try
        {
            var requisite = await service.GetRequisiteById(id);
            if (requisite == null)
            {
                return NotFound($"Requisite with id {id} not found");
            }
            return Ok(requisite);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] RequisiteUpdateDto? dto)
    {
        if (dto == null) return BadRequest("Invalid data");

        try
        {
            var result = await service.UpdateRequisite(id, dto);
            if (!result)
            {
                return NotFound($"Requisite with id {id} not found");
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var result = await service.DeleteRequisite(id);
            if (!result)
            {
                return NotFound($"Requisite with id {id} not found");
                
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}