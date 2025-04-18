﻿using PaymentGateway.Shared.DTOs.Requisite;

namespace PaymentGateway.Application.Interfaces;

public interface IRequisiteService
{
    Task<RequisiteDto?> CreateRequisite(RequisiteCreateDto dto, Guid userId);
    Task<IEnumerable<RequisiteDto>> GetAllRequisites();
    Task<IEnumerable<RequisiteDto>> GetUserRequisites(Guid userId);
    Task<RequisiteDto?> GetRequisiteById(Guid id, Guid userId);
    Task<RequisiteDto?> UpdateRequisite(Guid id, RequisiteUpdateDto dto);
    Task<RequisiteDto?> DeleteRequisite(Guid id);
}