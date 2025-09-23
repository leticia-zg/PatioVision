using PatioVision.Core.Models;
using PatioVision.Data.Context;
using PatioVision.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PatioVision.Service.Services
{
    public class PatioService
    {
        private readonly AppDbContext _context;

        public PatioService(AppDbContext context)
        {
            _context = context;
        }

        // --- Paginado ---
        public async Task<PagedResult<Patio>> ObterPaginadoAsync(
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
                query = query.Where(p => p.Nome.Contains(search) || p.Localizacao.Contains(search));
            }

            switch (sort?.ToLowerInvariant())
            {
                case "nome":
                    query = query.OrderBy(p => p.Nome);
                    break;
                case "-nome":
                    query = query.OrderByDescending(p => p.Nome);
                    break;
                case "dtcadastro":
                    query = query.OrderBy(p => p.DtCadastro);
                    break;
                case "-dtcadastro":
                default:
                    query = query.OrderByDescending(p => p.DtCadastro);
                    break;
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Patio>(items, total, pageNumber, pageSize);
        }

        // métodos já existentes (síncronos), com ajuste em auditoria
        public IEnumerable<Patio> ObterTodos()
        {
            return _context.Patios
                .Include(p => p.Dispositivo)
                .Include(p => p.Motos)
                .ToList();
        }

        public Patio? ObterPorId(Guid id)
        {
            return _context.Patios
                .Include(p => p.Dispositivo)
                .Include(p => p.Motos)
                .FirstOrDefault(p => p.PatioId == id);
        }

        public Patio Criar(Patio patio)
        {
            patio.DtCadastro = DateTime.UtcNow;
            patio.DtAtualizacao = DateTime.UtcNow;

            _context.Patios.Add(patio);
            _context.SaveChanges();
            return patio;
        }

        public bool Atualizar(Guid id, Patio patioAtualizado)
        {
            var patio = ObterPorId(id);
            if (patio == null) return false;

            patio.Nome = patioAtualizado.Nome;
            patio.Categoria = patioAtualizado.Categoria;
            patio.Latitude = patioAtualizado.Latitude;
            patio.Longitude = patioAtualizado.Longitude;
            patio.DispositivoIotId = patioAtualizado.DispositivoIotId;
            patio.DtAtualizacao = DateTime.UtcNow;

            _context.Patios.Update(patio);
            _context.SaveChanges();
            return true;
        }

        public bool Remover(Guid id)
        {
            var patio = ObterPorId(id);
            if (patio == null) return false;

            _context.Patios.Remove(patio);
            _context.SaveChanges();
            return true;
        }
    }
}
