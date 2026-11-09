using Contracts.DTOs.Notification;
using Contracts.enums;
using Contracts.Interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class NotificationRepository
    : RepositoryBase<Notification, NotificationDto, NotificationCreateDto, NotificationUpdateDto>, INotificationRepository
{
    private readonly ILoggerManager _logger;

    public NotificationRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
        _logger = logger;
    }

    public override async Task<SingleObjectResponseModel> Create(NotificationCreateDto entityCreate)
    {
        try
        {
            var entity = entityCreate.Adapt<Notification>();

            if (entity.BranchId <= 0)
            {
                if (entityCreate.BranchId.HasValue && entityCreate.BranchId.Value > 0)
                {
                    entity.BranchId = entityCreate.BranchId.Value;
                }
                else
                {
                    // Resolve from recipient user's branch
                    var userBranchId = await RepositoryContext.Users
                        .AsNoTracking()
                        .Where(u => u.MilitaryNumber == entity.UserId)
                        .Select(u => u.BranchId)
                        .FirstOrDefaultAsync();

                    if (userBranchId.HasValue && userBranchId.Value > 0)
                    {
                        entity.BranchId = userBranchId.Value;
                    }
                    else if (RepositoryContext.CurrentBranchId.HasValue && RepositoryContext.CurrentBranchId.Value > 0)
                    {
                        entity.BranchId = RepositoryContext.CurrentBranchId.Value;
                    }
                    else
                    {
                        entity.BranchId = 1; // Default main branch to prevent FK conflict in SQL Server
                    }
                }
            }

            await RepositoryContext.Notifications.AddAsync(entity);
            await RepositoryContext.SaveChangesAsync();

            return new SingleObjectResponseModel<NotificationDto>
            {
                ErrorCode = ErrorCatalog.noError,
                SingleObject = entity.Adapt<NotificationDto>(),
                IsDone = true,
                ReturnMessage = "Object Added Successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.logErrorWithException(ex, $"{nameof(Notification)} ===> Create ");

            var errorMsg = ex.InnerException != null ? $"{ex.Message} --> {ex.InnerException.Message}" : ex.Message;
            return new SingleObjectResponseModel
            {
                ErrorCode = ErrorCatalog.DataBaseFauiler,
                IsDone = false,
                ReturnMessage = errorMsg,
            };
        }
    }

    public async Task<List<NotificationDto>> GetNotificationsForUserAsync(long userId, bool unreadOnly = false)
    {
        try
        {
            var query = RepositoryContext.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsDeleted);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.InsertDate)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    Type = n.Type,
                    ReferenceId = n.ReferenceId,
                    ReferenceType = n.ReferenceType,
                    InsertDate = n.InsertDate
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.logErrorWithException(ex, $"{nameof(Notification)} ===> GetNotificationsForUserAsync ");
            return new List<NotificationDto>();
        }
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        try
        {
            return await RepositoryContext.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.logErrorWithException(ex, $"{nameof(Notification)} ===> GetUnreadCountAsync ");
            return 0;
        }
    }

    public async Task MarkAsReadAsync(int notificationId, long userId)
    {
        try
        {
            var notification = await RepositoryContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification != null)
            {
                notification.IsRead = true;
                notification.LastUpdate = DateTime.UtcNow;
                await RepositoryContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.logErrorWithException(ex, $"{nameof(Notification)} ===> MarkAsReadAsync ");
        }
    }

    public async Task MarkAllAsReadAsync(long userId)
    {
        try
        {
            var unread = await RepositoryContext.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.LastUpdate = DateTime.UtcNow;
            }

            await RepositoryContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.logErrorWithException(ex, $"{nameof(Notification)} ===> MarkAllAsReadAsync ");
        }
    }
}
