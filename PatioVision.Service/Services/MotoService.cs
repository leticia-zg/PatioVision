using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using PatioVision.Core.Enums;


namespace PatioVision.Service.Services
{
    public class MotoService
    {
        private readonly AppDbContext _context;

        public MotoService(AppDbContext context)
        {
            _context = context;
        }

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
