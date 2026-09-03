using Microsoft.EntityFrameworkCore;
using Nailify.Application.Common.Interfaces;
using Nailify.Domain.Entities;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly NailifyDbContext _context;

    public UserRepository(NailifyDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
