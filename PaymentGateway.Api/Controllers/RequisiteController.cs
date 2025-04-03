using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Validators.Requisite;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RequisiteController(RequisiteService service, RequisiteValidator validator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RequisiteResponseDto>> CreateRequisite([FromBody] RequisiteCreateDto? dto)
    {
        if (dto == null) return BadRequest("Invalid data");

        try
        {
            var createdRequisite = await service.CreateRequisite(dto);
            return CreatedAtAction(nameof(service.CreateRequisite), new { id = createdRequisite.Id }, createdRequisite);
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
    public async Task<ActionResult<IEnumerable<RequisiteResponseDto>>> GetAllRequisites()
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
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequisiteResponseDto>> GetRequisiteById(Guid id)
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
    public async Task<ActionResult> UpdateRequisite(Guid id, [FromBody] RequisiteUpdateDto? dto)
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
    public async Task<ActionResult> DeleteRequisite(Guid id)
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