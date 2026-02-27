using FAS.Domain.Entities;
using FAS.Domain.Interfaces;
using FAS.Domain.Pagination;
using FAS.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FAS.Infra.Data.Repositories
{
    public class TalhaoRepository : ITalhaoRepository
    {
        private readonly ApplicationDbContext _context;

        public TalhaoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Talhao> Incluir(Talhao talhao)
        {
            await _context.Talhao.AddAsync(talhao);
            return talhao;
        }

        public void Alterar(Talhao talhao)
        {
            _context.Talhao.Update(talhao);
        }

        public async Task<Talhao> Excluir(int id)
        {
            var talhao = await Selecionar(id);

            if (talhao != null)
            {
                _context.Talhao.Remove(talhao);
                return talhao;
            }

            return null;
        }

        public async Task<Talhao> Selecionar(int id)
        {
            return await _context.Talhao
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<PagedList<Talhao>> ListarPorPropriedade(int propriedadeId, int pageNumber, int pageSize)
        {
            var query = _context.Talhao
                .AsNoTracking()
                .Where(t => t.PropriedadeId == propriedadeId)
                .OrderBy(t => t.Id);

            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Talhao>(items, pageNumber, pageSize, count);
        }
    }
}

