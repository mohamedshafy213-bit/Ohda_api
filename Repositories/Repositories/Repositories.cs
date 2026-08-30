
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
using Microsoft.Extensions.Caching.Memory;
using Entities.Models.Tables;
using System.Reflection;


namespace Repositories.Repositories
{
    public abstract class RepositoryBase<T, TDto, TCreateDto, TUpdateDto> : IRepositoryBase<T, TDto, TCreateDto, TUpdateDto>
        where T : class
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class

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

        protected IMemoryCache? MemoryCache => 
            _httpContextAccessor.HttpContext?.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;

        private void InvalidateCache()
        {
            var cache = MemoryCache;
            if (cache != null && (typeof(T) == typeof(Department) || typeof(T) == typeof(Product)))
            {
                var versionKey = $"CacheVersion_{typeof(T).Name}";
                if (cache.TryGetValue(versionKey, out int version))
                {
                    cache.Set(versionKey, version + 1);
                }
                else
                {
                    cache.Set(versionKey, 1);
                }
            }
        }

        private object? ConvertKey(object key, Type targetType)
        {
            if (key == null) return null;
            if (key.GetType() == targetType) return key;

            if (key is string strKey)
            {
                if (targetType == typeof(int))
                {
                    return int.Parse(strKey);
                }
                if (targetType == typeof(long))
                {
                    return long.Parse(strKey);
                }
                if (targetType == typeof(Guid))
                {
                    return Guid.Parse(strKey);
                }
            }

            return Convert.ChangeType(key, targetType);
        }

        public virtual async Task<SingleObjectResponseModel> FindAll(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                var cache = MemoryCache;
                int version = 0;
                if (cache != null && (typeof(T) == typeof(Department) || typeof(T) == typeof(Product)))
                {
                    var versionKey = $"CacheVersion_{typeof(T).Name}";
                    if (!cache.TryGetValue(versionKey, out version))
                    {
                        version = 0;
                        cache.Set(versionKey, version);
                    }
                    var cacheKey = $"FindAll_{typeof(T).Name}_{version}_{pageNumber}_{pageSize}";
                    if (cache.TryGetValue(cacheKey, out ListOfObjectsResponseModel<TDto>? cachedResult) && cachedResult != null)
                    {
                        return cachedResult;
                    }
                }

                var query = RepositoryContext.Set<T>().AsNoTracking();
                var totalCount = await query.CountAsync();

                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    int skip = (pageNumber.Value - 1) * pageSize.Value;
                    query = query.Skip(skip).Take(pageSize.Value);
                }
                else
                {
                    query = query.Take(100); // cap unbounded queries
                }

                var listOfObjects = await query.ProjectToType<TDto>().ToListAsync();
                var response = new ListOfObjectsResponseModel<TDto>()
                {
                    ErrorCode = ErrorCatalog.noError,
                    IsDone = true,
                    ReturnMessage = "Objects Loaded Successufly",
                    Objects = listOfObjects,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                if (cache != null && (typeof(T) == typeof(Department) || typeof(T) == typeof(Product)))
                {
                    var cacheKey = $"FindAll_{typeof(T).Name}_{version}_{pageNumber}_{pageSize}";
                    cache.Set(cacheKey, response, TimeSpan.FromMinutes(10));
                }

                return response;
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

                        // Only auto-assign a key if the entity doesn't already have one set
                        var propertyInfo = typeof(T).GetProperty(keyName);
                        var currentKeyValue = propertyInfo?.GetValue(entity);
                        bool isKeyDefault = currentKeyValue == null
                            || currentKeyValue.Equals(0)
                            || currentKeyValue.Equals(Guid.Empty)
                            || currentKeyValue.Equals(string.Empty);

                        if (isKeyDefault)
                        {
                            var dbSet = RepositoryContext.Set<T>();
                            var maxValue = await dbSet
                                .Select(e => EF.Property<int?>(e, keyName))
                                .MaxAsync();
                            int newId = (maxValue ?? 0) + 1;
                            propertyInfo?.SetValue(entity, Convert.ChangeType(newId, propertyInfo.PropertyType));
                        }
                    }
                }

                await RepositoryContext.Set<T>().AddAsync(entity);
                await RepositoryContext.SaveChangesAsync();
                InvalidateCache();

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

                var keyProperty = primaryKey.Properties.First();
                var keyName = keyProperty.Name;
                var keyClrType = keyProperty.ClrType;
                var resolvedKey = ConvertKey(key, keyClrType);

                var dbSet = RepositoryContext.Set<T>();

                var entity = await dbSet
                    .FirstOrDefaultAsync(e => EF.Property<object>(e, keyName).Equals(resolvedKey));

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
                InvalidateCache();

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

                var keyProperty = primaryKey.Properties.First();
                var keyName = keyProperty.Name;
                var keyClrType = keyProperty.ClrType;
                var resolvedKey = ConvertKey(key, keyClrType);

                var dbSet = RepositoryContext.Set<T>();

                var entity = await dbSet
                    .FirstOrDefaultAsync(e => EF.Property<object>(e, keyName).Equals(resolvedKey));

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
                InvalidateCache();

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

        public virtual async Task CreateDirectAsync(T entity)
        {
            await RepositoryContext.Set<T>().AddAsync(entity);
        }
    }
}
