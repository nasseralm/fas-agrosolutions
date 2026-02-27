using FCG.Domain.Entities;

namespace FCG.Domain.Interfaces
{
    public interface IJogoRepository
    {
        Task<Jogo> Incluir(Jogo usuario);
        void Alterar(Jogo usuario);
        Task<Jogo> Excluir(int id);
        Task<Jogo> Selecionar(int id);
        Task<Jogo> SelecionarPorNome(string nome);
        Task<IEnumerable<Jogo>> SelecionarTodos();
        Task<IEnumerable<Jogo>> SelecionarTodosAsync();
    }
}
