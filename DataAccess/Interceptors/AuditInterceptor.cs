using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;
namespace PEQ.DataAccess.Interceptor
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private void UpdateAuditProperties(DbContext? context)
        {
            if (context == null) return;

            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId();
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is FullAuditedEntity fullAuditedEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            fullAuditedEntity.CreationTime = now;
                            break;
                        case EntityState.Modified:
                            fullAuditedEntity.LastModificationTime = now;
                            break;
                        case EntityState.Deleted:
                            fullAuditedEntity.LastModificationTime = now;
                            fullAuditedEntity.DeletionTime = now;
                            fullAuditedEntity.IsDeleted = true;
                            entry.State = EntityState.Modified;
                            break;
                    }

                }
            }
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateAuditProperties(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

}
