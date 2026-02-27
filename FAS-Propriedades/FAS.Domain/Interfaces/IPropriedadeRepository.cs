using FAS.Domain.Entities;
using FAS.Domain.Pagination;

namespace FAS.Domain.Interfaces
{
    public interface IPropriedadeRepository
    {
        Task<Propriedade> Incluir(Propriedade propriedade);
        void Alterar(Propriedade propriedade);
        Task<Propriedade> Excluir(int id);
        Task<Propriedade> Selecionar(int id);
        Task<PagedList<Propriedade>> ListarPorProducer(int producerId, int pageNumber, int pageSize);
    }
}

