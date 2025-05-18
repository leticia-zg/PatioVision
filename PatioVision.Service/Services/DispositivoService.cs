using PatioVision.Core.Models;
using PatioVision.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PatioVision.Service.Services
{
    public class DispositivoService
    {
        private readonly AppDbContext _context;

        public DispositivoService(AppDbContext context)
        {
            _context = context;
        }

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
