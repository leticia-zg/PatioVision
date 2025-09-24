using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using PatioVision.Service.Common;

namespace PatioVision.Service.Services
{
    public class MotoService
    {
        private readonly AppDbContext _context;

        public MotoService(AppDbContext context) => _context = context;

        // --- Pagination ---
        public async Task<PagedResult<Moto>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            string? sort = "-dtcadastro",
            CancellationToken ct = default)
        {
            var query = _context.Motos
                .AsNoTracking()
                .Include(m => m.Patio)
                .Include(m => m.Dispositivo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    m.Modelo.Contains(search) ||
                    (m.Placa != null && m.Placa.Contains(search)));
            }

            switch (sort?.ToLowerInvariant())
            {
                case "modelo": query = query.OrderBy(m => m.Modelo); break;
                case "-modelo": query = query.OrderByDescending(m => m.Modelo); break;
                case "dtcadastro": query = query.OrderBy(m => m.DtCadastro); break;
                case "-dtcadastro":
                default: query = query.OrderByDescending(m => m.DtCadastro); break;
            }

            var total = await query.CountAsync(ct);
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(ct);

            return new PagedResult<Moto>(items, total, pageNumber, pageSize);
        }

        // --- CRUD ---
        public Moto? GetById(Guid id)
            => _context.Motos
                .AsNoTracking()
                .Include(m => m.Patio)
                .Include(m => m.Dispositivo)
                .FirstOrDefault(m => m.MotoId == id);

        public Moto Create(Moto moto)
        {
            if (moto is null) throw new ArgumentNullException(nameof(moto));
            if (string.IsNullOrWhiteSpace(moto.Modelo))
                throw new ArgumentException("Modelo é obrigatório.");

            moto.DtCadastro = DateTime.UtcNow;
            moto.DtAtualizacao = DateTime.UtcNow;

            _context.Motos.Add(moto);
            _context.SaveChanges();
            return moto;
        }

        public bool Update(Guid id, Moto updated)
        {
            if (updated is null || id == Guid.Empty || updated.MotoId != id)
                throw new ArgumentException("Dados inválidos para atualização.");

            var exists = _context.Motos.AsNoTracking().Any(m => m.MotoId == id);
            if (!exists) return false;

            if (string.IsNullOrWhiteSpace(updated.Modelo))
                throw new ArgumentException("Modelo é obrigatório.");

            updated.DtAtualizacao = DateTime.UtcNow;

            _context.Entry(updated).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        public bool Delete(Guid id)
        {
            var entity = _context.Motos.Find(id);
            if (entity is null) return false;

            _context.Motos.Remove(entity);
            _context.SaveChanges();
            return true;
        }
    }
}
