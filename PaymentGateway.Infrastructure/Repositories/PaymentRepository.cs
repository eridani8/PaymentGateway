using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class PaymentRepository(AppDbContext context) : RepositoryBase<PaymentEntity>(context), IPaymentRepository
{
    private readonly AppDbContext _context = context;
}