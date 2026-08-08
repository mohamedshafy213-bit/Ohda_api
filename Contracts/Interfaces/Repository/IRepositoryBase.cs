using Contracts.Responses;
using System.Linq.Expressions;

namespace Contracts.interfaces.Repository
{
    public interface IRepositoryBase<T, TDto, TCreateDto, TUpdateDto>
    {
        Task<SingleObjectResponseModel> FindAll();
        Task<SingleObjectResponseModel> Create(TCreateDto entityCreate);
        Task<SingleObjectResponseModel> Update(object key, TUpdateDto entityUpdate);
        Task<SingleObjectResponseModel> Delete(object key);
    }
}
