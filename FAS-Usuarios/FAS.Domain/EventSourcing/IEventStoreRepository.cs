using System.Collections.Generic;
using System.Threading.Tasks;
using FAS.Domain.Entities;

namespace FAS.Domain.EventSourcing
{
    public interface IEventStoreRepository
    {
        Task StoreEventAsync(StoredEvent storedEvent);
        Task<IEnumerable<StoredEvent>> GetEventsAsync(int aggregateId);
    }
}
