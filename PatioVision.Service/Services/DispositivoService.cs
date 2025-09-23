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
    public class DispositivoService
    {
        private readonly AppDbContext _context;

        public DispositivoService(AppDbContext context)
        {
            _context = context;
        }

        // --- Paginado ---
        public async Task<PagedResult<DispositivoIoT>> ObterPaginadoAsync(
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
                case "tipo":
                    query = query.OrderBy(d => d.Tipo);
                    break;
                case "-tipo":
                    query = query.OrderByDescending(d => d.Tipo);
                    break;
                case "ultimaatualizacao":
                case "ultimataualizacao":
                    query = query.OrderBy(d => d.UltimaAtualizacao);
                    break;
                case "-ultimaatualizacao":
                default:
                    query = query.OrderByDescending(d => d.UltimaAtualizacao);
                    break;
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<DispositivoIoT>(items, total, pageNumber, pageSize);
        }

        // métodos existentes (síncronos), com auditoria
        public IEnumerable<DispositivoIoT> ObterTodos()
        {
            return _context.Dispositivos.ToList();
        }

        public DispositivoIoT? ObterPorId(Guid id)
        {
            return _context.Dispositivos.FirstOrDefault(d => d.DispositivoIotId == id);
        }

        public DispositivoIoT Criar(DispositivoIoT dispositivo)
        {
            dispositivo.DtCadastro = DateTime.UtcNow;
            dispositivo.DtAtualizacao = DateTime.UtcNow;

            _context.Dispositivos.Add(dispositivo);
            _context.SaveChanges();
            return dispositivo;
        }

        public bool Atualizar(Guid id, DispositivoIoT atualizado)
        {
            var dispositivo = ObterPorId(id);
            if (dispositivo == null) return false;

            dispositivo.Tipo = atualizado.Tipo;
            dispositivo.UltimaLocalizacao = atualizado.UltimaLocalizacao;
            dispositivo.UltimaAtualizacao = DateTime.UtcNow;
            dispositivo.DtAtualizacao = DateTime.UtcNow;

            _context.Dispositivos.Update(dispositivo);
            _context.SaveChanges();
            return true;
        }

        public bool Remover(Guid id)
        {
            var dispositivo = ObterPorId(id);
            if (dispositivo == null) return false;

            _context.Dispositivos.Remove(dispositivo);
            _context.SaveChanges();
            return true;
        }
    }
}
