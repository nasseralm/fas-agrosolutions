using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAS.Domain.Entities;
using FAS.Domain.EventSourcing;
using FAS.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FAS.Infra.Data.EventSourcing
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly ApplicationDbContext _context;

        public EventStoreRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StoredEvent>> GetEventsAsync(int aggregateId)
        {
            return await _context.StoredEvent
                .Where(e => e.AggregateId == aggregateId)
                .OrderBy(e => e.Timestamp)
                .ToListAsync();
        }

        public async Task StoreEventAsync(StoredEvent storedEvent)
        {
            await _context.StoredEvent.AddAsync(storedEvent);
            await _context.SaveChangesAsync();
        }
    }
}
