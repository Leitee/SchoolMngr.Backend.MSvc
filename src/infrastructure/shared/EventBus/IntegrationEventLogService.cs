﻿/// <summary>
/// 
/// </summary>
namespace SchoolMngr.Infrastructure.Shared.EventBus
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Codeit.NetStdLibrary.Base.Abstractions.Desentralized;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class IntegrationEventLogService : IIntegrationEventLogService
    {
        private readonly IntegrationEventLogContext _integrationEventLogContext;
        private readonly List<Type> _eventTypes;

        public IntegrationEventLogService(IntegrationEventLogContext eventLogContext)
        {
            _integrationEventLogContext = eventLogContext;

            _eventTypes = Assembly.Load(Assembly.GetEntryAssembly().FullName)
                .GetTypes()
                .Where(t => t.Name.EndsWith(nameof(IntegrationEventPayload)))
                .ToList();
        }

        public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
        {
            var tid = transactionId.ToString();

            var result = await _integrationEventLogContext.IntegrationEventLogs
                .Where(e => e.TransactionId == tid && e.State == EventStateEnum.NotPublished).ToListAsync();

            if (result != null && result.Any())
            {
                return result.OrderBy(o => o.CreationTime)
                    .Select(e => e.DeserializeJsonContent(_eventTypes.Find(t => t.Name == e.EventTypeShortName)));
            }

            return new List<IntegrationEventLogEntry>();
        }

        public Task SaveEventAsync(IntegrationEventPayload @event, IDbContextTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);

            _integrationEventLogContext.Database.UseTransaction(transaction.GetDbTransaction());
            _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

            return _integrationEventLogContext.SaveChangesAsync();
        }

        public Task MarkEventAsPublishedAsync(Guid eventId)
        {
            return UpdateEventStatus(eventId, EventStateEnum.Published);
        }

        public Task MarkEventAsInProgressAsync(Guid eventId)
        {
            return UpdateEventStatus(eventId, EventStateEnum.InProgress);
        }

        public Task MarkEventAsFailedAsync(Guid eventId)
        {
            return UpdateEventStatus(eventId, EventStateEnum.PublishedFailed);
        }

        private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
        {
            var eventLogEntry = _integrationEventLogContext.IntegrationEventLogs.Single(ie => ie.EventId == eventId);
            eventLogEntry.State = status;

            if (status == EventStateEnum.InProgress)
                eventLogEntry.TimesSent++;

            _integrationEventLogContext.IntegrationEventLogs.Update(eventLogEntry);

            return _integrationEventLogContext.SaveChangesAsync();
        }
    }
}
