using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using PatioVision.Core.Enums;
using PatioVision.Service.Common;

namespace PatioVision.Service.Services
{
    public class MotoService
    {
        private readonly AppDbContext _context;

        public MotoService(AppDbContext context)
        {
            _context = context;
        }

        // --- Paginado ---
        public async Task<PagedResult<Moto>> ObterPaginadoAsync(
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
                query = query.Where(m => m.Modelo.Contains(search) || (m.Placa != null && m.Placa.Contains(search)));
            }

            switch (sort?.ToLowerInvariant())
            {
                case "modelo":
                    query = query.OrderBy(m => m.Modelo);
                    break;
                case "-modelo":
                    query = query.OrderByDescending(m => m.Modelo);
                    break;
                case "dtcadastro":
                    query = query.OrderBy(m => m.DtCadastro);
                    break;
                case "-dtcadastro":
                default:
                    query = query.OrderByDescending(m => m.DtCadastro);
                    break;
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Moto>(items, total, pageNumber, pageSize);
        }

        // métodos existentes (síncronos), com auditoria
        public IEnumerable<Moto> ObterTodas()
        {
            return _context.Motos
                .Include(m => m.Patio)
                .Include(m => m.Dispositivo)
                .ToList();
        }

        public Moto? ObterPorId(Guid id)
        {
            return _context.Motos
                .Include(m => m.Patio)
                .Include(m => m.Dispositivo)
                .FirstOrDefault(m => m.MotoId == id);
        }

        public Moto Criar(Moto moto)
        {
            moto.DtCadastro = DateTime.UtcNow;
            moto.DtAtualizacao = DateTime.UtcNow;

            _context.Motos.Add(moto);
            _context.SaveChanges();
            return moto;
        }

        public bool Atualizar(Guid id, Moto motoAtualizada)
        {
            var moto = ObterPorId(id);
            if (moto == null) return false;

            moto.Modelo = motoAtualizada.Modelo;
            moto.Placa = motoAtualizada.Placa;
            moto.Status = motoAtualizada.Status;
            moto.PatioId = motoAtualizada.PatioId;
            moto.DispositivoIotId = motoAtualizada.DispositivoIotId;
            moto.DtAtualizacao = DateTime.UtcNow;

            _context.Motos.Update(moto);
            _context.SaveChanges();
            return true;
        }

        public bool Remover(Guid id)
        {
            var moto = ObterPorId(id);
            if (moto == null) return false;

            _context.Motos.Remove(moto);
            _context.SaveChanges();
            return true;
        }

        public IEnumerable<Moto> ObterPorStatus(StatusMoto status)
        {
            return _context.Motos
                .Where(m => m.Status == status)
                .Include(m => m.Patio)
                .Include(m => m.Dispositivo)
                .ToList();
        }
    }
}
