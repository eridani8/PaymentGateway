using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class RequisiteRepositoryBase(AppDbContext context) : RepositoryBase<Requisite>(context), IRequisiteRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<Requisite>> GetActiveRequisites()
    {
        return await _context.Requisites
            .Where(r => r.IsActive)
            .ToListAsync();
    }
}