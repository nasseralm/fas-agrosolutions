using FAS.Domain.Entities;
using FAS.Domain.Pagination;

namespace FAS.Domain.Interfaces
{
    public interface ITalhaoRepository
    {
        Task<Talhao> Incluir(Talhao talhao);
        void Alterar(Talhao talhao);
        Task<Talhao> Excluir(int id);
        Task<Talhao> Selecionar(int id);
        Task<PagedList<Talhao>> ListarPorPropriedade(int propriedadeId, int pageNumber, int pageSize);
    }
}

