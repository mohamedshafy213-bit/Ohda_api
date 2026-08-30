using Contracts.Responses;
using System.Linq.Expressions;

namespace Contracts.interfaces.Repository
{
    public interface IRepositoryBase<T, TDto, TCreateDto, TUpdateDto>
    {
        Task<SingleObjectResponseModel> FindAll(int? pageNumber = null, int? pageSize = null);
        Task<SingleObjectResponseModel> Create(TCreateDto entityCreate);
        Task<SingleObjectResponseModel> Update(object key, TUpdateDto entityUpdate);
        Task<SingleObjectResponseModel> Delete(object key);
        Task CreateDirectAsync(T entity);
    }
}
