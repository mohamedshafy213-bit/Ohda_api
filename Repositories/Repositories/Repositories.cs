
using Contracts.BaseDtos;
using Contracts.enums;
using Contracts.interfaces.Repository;
using Contracts.Pagging;
using Contracts.Responses;
using Entities.Models;
using Entities.Models.ClassHelper;
using Entities.Models.Databases;
using LoggerService;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;


namespace Repositories.Repositories
{
    public abstract class RepositoryBase<T, TDto, TCreateDto, TUpdateDto> : IRepositoryBase<T, TDto, TCreateDto, TUpdateDto>
        where T : Entities.Models.BaseTables.BaseTable
        where TDto : BaseDto
        where TCreateDto : BaseCreateDto
        where TUpdateDto : BaseUpdateDto

    {
        protected RepositoryContext RepositoryContext { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;

        public RepositoryBase(ILoggerManager logger, RepositoryContext repositoryContext, IHttpContextAccessor httpContextAccessor
            , IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            RepositoryContext = repositoryContext;
            _mapper = mapper;
            _logger = logger;
        }

        public virtual async Task<SingleObjectResponseModel> FindAll()
        {
            try
            {
                var listOfObjects = await RepositoryContext.Set<T>().AsNoTracking().ProjectToType<TDto>().ToListAsync();
                return new ListOfObjectsResponseModel<TDto>()
                {
                    ErrorCode = ErrorCatalog.noError,
                    IsDone = true,
                    ReturnMessage = "Objects Loaded Successufly",
                    Objects = listOfObjects
                };
            }
            catch (Exception ex)
            {
                _logger.logErrorWithException(ex, $"{typeof(T).Name} ===> FindAll ");
                return new SingleObjectResponseModel()
                {
                    ErrorCode = ErrorCatalog.DataBaseFauiler,
                    IsDone = false,
                    ReturnMessage = ex.Message,
                };
            }
        }



        public virtual async Task<SingleObjectResponseModel> Create(TCreateDto entityCreate)
        {
            try
            {
                T entity = entityCreate.Adapt<T>();

                var entityType = RepositoryContext.Model.FindEntityType(typeof(T));
                var primaryKey = entityType?.FindPrimaryKey();

                if (primaryKey != null)
                {
                    var keyProperty = primaryKey.Properties.First();
                    if (keyProperty.ValueGenerated == Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never)
                    {
                        var keyName = keyProperty.Name;
                        var dbSet = RepositoryContext.Set<T>();

                        var maxValue = await dbSet
                            .Select(e => EF.Property<int?>(e, keyName))
                            .MaxAsync();

                        int newId = (maxValue ?? 0) + 1;

                        var propertyInfo = typeof(T).GetProperty(keyName);
                        propertyInfo?.SetValue(entity, Convert.ChangeType(newId, propertyInfo.PropertyType));
                    }
                }

                await RepositoryContext.Set<T>().AddAsync(entity);
                await RepositoryContext.SaveChangesAsync();

                return new SingleObjectResponseModel<TDto>()
                {
                    ErrorCode = ErrorCatalog.noError,
                    SingleObject = entity.Adapt<TDto>(),
                    IsDone = true,
                    ReturnMessage = "Object Added Successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.logErrorWithException(ex, $"{typeof(T).Name} ===> Create ");

                return new SingleObjectResponseModel()
                {
                    ErrorCode = ErrorCatalog.DataBaseFauiler,
                    IsDone = false,
                    ReturnMessage = ex.Message,
                };
            }
        }

        public virtual async Task<SingleObjectResponseModel> Update(object key, TUpdateDto entityUpdate)
        {
            try
            {
                var entityType = RepositoryContext.Model.FindEntityType(typeof(T));
                var primaryKey = entityType?.FindPrimaryKey();

                if (primaryKey == null)
                    throw new Exception("Primary key not found");

                var keyName = primaryKey.Properties.First().Name;

                var dbSet = RepositoryContext.Set<T>();

                var entity = await dbSet
                    .FirstOrDefaultAsync(e => EF.Property<object>(e, keyName).Equals(key));

                if (entity == null)
                {
                    return new SingleObjectResponseModel
                    {
                        ErrorCode = ErrorCatalog.ObjectNotFound,
                        IsDone = false,
                        ReturnMessage = "Object not found"
                    };
                }

                entityUpdate.Adapt(entity);

                RepositoryContext.Set<T>().Update(entity);
                await RepositoryContext.SaveChangesAsync();

                return new SingleObjectResponseModel
                {
                    ErrorCode = ErrorCatalog.noError,
                    IsDone = true,
                    ReturnMessage = "Object Updated Successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.logErrorWithException(ex, $"{typeof(T).Name} ===> Update ");

                return new SingleObjectResponseModel
                {
                    ErrorCode = ErrorCatalog.DataBaseFauiler,
                    IsDone = false,
                    ReturnMessage = ex.Message
                };
            }
        }

        public virtual async Task<SingleObjectResponseModel> Delete(object key)
        {
            try
            {
                var entityType = RepositoryContext.Model.FindEntityType(typeof(T));
                var primaryKey = entityType?.FindPrimaryKey();

                if (primaryKey == null)
                    throw new Exception("Primary key not found");

                var keyName = primaryKey.Properties.First().Name;

                var dbSet = RepositoryContext.Set<T>();

                var entity = await dbSet
                    .FirstOrDefaultAsync(e => EF.Property<object>(e, keyName).Equals(key));

                if (entity == null)
                {
                    return new SingleObjectResponseModel
                    {
                        ErrorCode = ErrorCatalog.ObjectNotFound,
                        IsDone = false,
                        ReturnMessage = "Object not found"
                    };
                }

                dbSet.Remove(entity);
                await RepositoryContext.SaveChangesAsync();

                return new SingleObjectResponseModel
                {
                    ErrorCode = ErrorCatalog.noError,
                    IsDone = true,
                    ReturnMessage = "Object Deleted Successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.logErrorWithException(ex, $"{typeof(T).Name} ===> Delete ");

                return new SingleObjectResponseModel
                {
                    ErrorCode = ErrorCatalog.DataBaseFauiler,
                    IsDone = false,
                    ReturnMessage = ex.Message
                };
            }
        }

    }
}
