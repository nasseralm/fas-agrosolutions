using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Infra.Data.Context;
using FCG.Infra.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FCG.Infra.Data.Repositories
{
    public class CompraRepository : ICompraRepository
    {
        private readonly ApplicationDbContext _context;

        public CompraRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Compra> Incluir(Compra compra)
        {
            await _context.Compra.AddAsync(compra);
            return compra;
        }
        
        public async Task<Compra> Selecionar(int compraId)
        {
            return await _context.Compra.FirstOrDefaultAsync(c => c.Id == compraId);
        }

        public void Alterar(Compra compra)
        {
            _context.Compra.Update(compra);
        }
    }
}