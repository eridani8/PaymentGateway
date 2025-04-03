﻿using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class RequisiteRepository(AppDbContext context) : RepositoryBase<RequisiteEntity>(context), IRequisiteRepository
{
    private readonly AppDbContext _context = context;
}