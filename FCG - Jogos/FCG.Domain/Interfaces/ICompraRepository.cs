using FCG.Domain.Entities;
using System.Threading.Tasks;

namespace FCG.Domain.Interfaces
{
    public interface ICompraRepository
    {
        Task<Compra> Incluir(Compra compra);
        Task<Compra> Selecionar(int compraId);
        public void Alterar(Compra compra);
    }
}