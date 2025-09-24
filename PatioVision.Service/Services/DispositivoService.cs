using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using PatioVision.Service.Common;

namespace PatioVision.Service.Services
{
    public class DispositivoService
    {
        private readonly AppDbContext _context;

        public DispositivoService(AppDbContext context) => _context = context;

        // --- Pagination ---
        public async Task<PagedResult<DispositivoIoT>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            string? sort = "-ultimaatualizacao",
            CancellationToken ct = default)
        {
            var query = _context.Dispositivos
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.Tipo.ToString().Contains(search) ||
                    (d.UltimaLocalizacao != null && d.UltimaLocalizacao.Contains(search)));
            }

            switch (sort?.ToLowerInvariant())
            {
                case "tipo": query = query.OrderBy(d => d.Tipo); break;
                case "-tipo": query = query.OrderByDescending(d => d.Tipo); break;
                case "ultimaatualizacao": query = query.OrderBy(d => d.UltimaAtualizacao); break;
                case "-ultimaatualizacao":
                default: query = query.OrderByDescending(d => d.UltimaAtualizacao); break;
            }

            var total = await query.CountAsync(ct);
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(ct);

            return new PagedResult<DispositivoIoT>(items, total, pageNumber, pageSize);
        }

        // --- CRUD ---
        public DispositivoIoT? GetById(Guid id)
            => _context.Dispositivos
                       .AsNoTracking()
                       .FirstOrDefault(d => d.DispositivoIotId == id);

        public DispositivoIoT Create(DispositivoIoT device)
        {
            if (device is null) throw new ArgumentNullException(nameof(device));

            device.DtCadastro = DateTime.UtcNow;
            device.DtAtualizacao = DateTime.UtcNow;
            if (device.UltimaAtualizacao == default)
                device.UltimaAtualizacao = DateTime.UtcNow;

            _context.Dispositivos.Add(device);
            _context.SaveChanges();
            return device;
        }

        public bool Update(Guid id, DispositivoIoT updated)
        {
            if (updated is null || id == Guid.Empty || updated.DispositivoIotId != id)
                throw new ArgumentException("Dados inválidos para atualização.");

            var exists = _context.Dispositivos.AsNoTracking().Any(d => d.DispositivoIotId == id);
            if (!exists) return false;

            updated.DtAtualizacao = DateTime.UtcNow;
            updated.UltimaAtualizacao = DateTime.UtcNow;

            _context.Entry(updated).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        public bool Delete(Guid id)
        {
            var entity = _context.Dispositivos.Find(id);
            if (entity is null) return false;

            _context.Dispositivos.Remove(entity);
            _context.SaveChanges();
            return true;
        }
    }
}
