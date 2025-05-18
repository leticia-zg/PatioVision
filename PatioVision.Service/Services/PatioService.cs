using PatioVision.Core.Models;
using PatioVision.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
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
