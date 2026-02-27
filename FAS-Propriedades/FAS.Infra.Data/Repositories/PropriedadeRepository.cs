using FAS.Domain.Entities;
using FAS.Domain.Interfaces;
using FAS.Domain.Pagination;
using FAS.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FAS.Infra.Data.Repositories
{
    public class PropriedadeRepository : IPropriedadeRepository
    {
        private readonly ApplicationDbContext _context;

        public PropriedadeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Propriedade> Incluir(Propriedade propriedade)
        {
            await _context.Propriedade.AddAsync(propriedade);
            return propriedade;
        }

        public void Alterar(Propriedade propriedade)
        {
            _context.Propriedade.Update(propriedade);
        }

        public async Task<Propriedade> Excluir(int id)
        {
            var propriedade = await Selecionar(id);

            if (propriedade != null)
            {
                _context.Propriedade.Remove(propriedade);
                return propriedade;
            }

            return null;
        }

        public async Task<Propriedade> Selecionar(int id)
        {
            return await _context.Propriedade
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PagedList<Propriedade>> ListarPorProducer(int producerId, int pageNumber, int pageSize)
        {
            var query = _context.Propriedade
                .AsNoTracking()
                .Where(p => p.ProducerId == producerId)
                .OrderBy(p => p.Id);

            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Propriedade>(items, pageNumber, pageSize, count);
        }
    }
}

