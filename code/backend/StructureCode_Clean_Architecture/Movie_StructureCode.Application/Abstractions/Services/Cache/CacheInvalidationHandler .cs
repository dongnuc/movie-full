using MediatR;
using Movie_StructureCode.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    internal class CacheInvalidationHandler : INotificationHandler<EntityChangedEvent>
    {
        private readonly ICacheDataService _redis;

        public CacheInvalidationHandler(ICacheDataService cacheDataService)
        {
            _redis = cacheDataService;
        }

        public async Task Handle(EntityChangedEvent notification, CancellationToken cancellationToken)
        {
            await _redis.InCrementTagVersionAsync(notification.Entity);

            if(notification.EntityId.HasValue)
            {
                await _redis.InCrementEntityVersionAsync(notification.Entity, notification.EntityId.Value);

                await _redis.RemoveAsync($"{notification.Entity}:{notification.EntityId}");
            }

        }
    }
}
