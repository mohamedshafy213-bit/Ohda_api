using Contracts.DTOs.UntrackedItem;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IUntrackedItemRepository : IRepositoryBase<UntrackedItem, UntrackedItemDto, UntrackedItemCreateDto, UntrackedItemUpdateDto>
{
}

