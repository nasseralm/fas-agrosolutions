using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using FCG.Infra.Data.Extensions;

namespace FCG.Infra.Data.Repositories
{
    public class JogoRepository : IJogoRepository
    {
        private readonly ApplicationDbContext _context;

        public JogoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Jogo> Incluir(Jogo jogo)
        {
            using var activity = TelemetryExtensions.StartRepositoryActivity("JogoRepository", "Incluir");
            activity?.SetTag("jogo.nome", jogo.Nome);
            
            try
            {
                await _context.Jogo.AddAsync(jogo);
                activity?.EnrichWithDatabaseContext("Jogo");
                activity?.SetRepositoryResult(true);
                return jogo;
            }
            catch (Exception ex)
            {
                activity?.SetRepositoryResult(false, ex.Message);
                throw;
            }
        }
        public void Alterar(Jogo jogo)
        {
            using var activity = TelemetryExtensions.StartRepositoryActivity("JogoRepository", "Alterar", jogo.Id);
            activity?.SetTag("jogo.nome", jogo.Nome);
            
            try
            {
                _context.Jogo.Update(jogo);
                activity?.EnrichWithDatabaseContext("Jogo");
                activity?.SetRepositoryResult(true);
            }
            catch (Exception ex)
            {
                activity?.SetRepositoryResult(false, ex.Message);
                throw;
            }
        }
        public async Task<Jogo> Excluir(int id)
        {
            using var activity = TelemetryExtensions.StartRepositoryActivity("JogoRepository", "Excluir", id);
            
            try
            {
                var jogo = await _context.Jogo.FindAsync(id);
                
                if (jogo != null)
                {
                    activity?.SetTag("jogo.nome", jogo.Nome);
                    _context.Jogo.Remove(jogo);
                    activity?.SetRepositoryResult(true);
                }
                else
                {
                    activity?.SetTag("result", "not_found");
                    activity?.SetRepositoryResult(true); 
                }
                
                activity?.EnrichWithDatabaseContext("Jogo");
                return jogo;
            }
            catch (Exception ex)
            {
                activity?.SetRepositoryResult(false, ex.Message);
                throw;
            }
        }
        public async Task<Jogo> Selecionar(int id)
        {
            using var activity = TelemetryExtensions.StartRepositoryActivity("JogoRepository", "Selecionar", id);
            
            try
            {
                var jogo = await _context.Jogo.FindAsync(id);
                
                if (jogo != null)
                {
                    activity?.SetTag("jogo.nome", jogo.Nome);
                }
                else
                {
                    activity?.SetTag("result", "not_found");
                }
                
                activity?.EnrichWithDatabaseContext("Jogo");
                activity?.SetRepositoryResult(true);
                return jogo;
            }
            catch (Exception ex)
            {
                activity?.SetRepositoryResult(false, ex.Message);
                throw;
            }
        }
        public async Task<Jogo> SelecionarPorNome(string nome)
        {
            using var activity = TelemetryExtensions.StartRepositoryActivity("JogoRepository", "SelecionarPorNome");
            activity?.SetTag("jogo.nome", nome);
            
            try
            {
                var jogo = await _context.Jogo.FirstOrDefaultAsync(j => j.Nome == nome);
                
                if (jogo != null)
                {
                    activity?.SetTag("jogo.id", jogo.Id);
                }
                else
                {
                    activity?.SetTag("result", "not_found");
                }
                
                activity?.EnrichWithDatabaseContext("Jogo");
                activity?.SetRepositoryResult(true);
                return jogo;
            }
            catch (Exception ex)
            {
                activity?.SetRepositoryResult(false, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Jogo>> SelecionarTodos()
        {
            using var activity = TelemetryExtensions.StartRepositoryActivity("JogoRepository", "SelecionarTodos");
            
            try
            {
                var jogos = await _context.Jogo.ToListAsync();
                
                activity?.EnrichWithDatabaseContext("Jogo", jogos.Count);
                activity?.SetRepositoryResult(true);
                return jogos;
            }
            catch (Exception ex)
            {
                activity?.SetRepositoryResult(false, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Jogo>> SelecionarTodosAsync()
        {
            return await _context.Jogo
                .OrderBy(jogo => jogo.Id)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
