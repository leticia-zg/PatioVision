using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using PatioVision.Service.Common;

namespace PatioVision.Service.Services
{
    public class PatioService
    {
        private readonly AppDbContext _context;

        public PatioService(AppDbContext context) => _context = context;

        // --- Pagination ---
        public async Task<PagedResult<Patio>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            string? sort = "-dtcadastro",
            CancellationToken ct = default)
        {
            var query = _context.Patios
                .AsNoTracking()
                .Include(p => p.Dispositivo)
                .Include(p => p.Motos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Nome.Contains(search));
            }

            switch (sort?.ToLowerInvariant())
            {
                case "nome": query = query.OrderBy(p => p.Nome); break;
                case "-nome": query = query.OrderByDescending(p => p.Nome); break;
                case "dtcadastro": query = query.OrderBy(p => p.DtCadastro); break;
                case "-dtcadastro":
                default: query = query.OrderByDescending(p => p.DtCadastro); break;
            }

            var total = await query.CountAsync(ct);
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(ct);

            return new PagedResult<Patio>(items, total, pageNumber, pageSize);
        }

        // --- CRUD ---
        public Patio? GetById(Guid id)
            => _context.Patios
                       .AsNoTracking()
                       .Include(p => p.Dispositivo)
                       .Include(p => p.Motos)
                       .FirstOrDefault(p => p.PatioId == id);

        public Patio Create(Patio patio)
        {
            if (patio is null) throw new ArgumentNullException(nameof(patio));
            if (string.IsNullOrWhiteSpace(patio.Nome))
                throw new ArgumentException("Nome é obrigatório.");

            patio.DtCadastro = DateTime.UtcNow;
            patio.DtAtualizacao = DateTime.UtcNow;

            _context.Patios.Add(patio);
            _context.SaveChanges();
            return patio;
        }

        public bool Update(Guid id, Patio updated)
        {
            if (updated is null || id == Guid.Empty || updated.PatioId != id)
                throw new ArgumentException("Dados inválidos para atualização.");

            var exists = _context.Patios.AsNoTracking().Any(p => p.PatioId == id);
            if (!exists) return false;

            if (string.IsNullOrWhiteSpace(updated.Nome))
                throw new ArgumentException("Nome é obrigatório.");

            updated.DtAtualizacao = DateTime.UtcNow;

            _context.Entry(updated).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        public bool Delete(Guid id)
        {
            var entity = _context.Patios.Find(id);
            if (entity is null) return false;

            _context.Patios.Remove(entity);
            _context.SaveChanges();
            return true;
        }
    }
}
