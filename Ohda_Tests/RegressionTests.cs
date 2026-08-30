using System;
using System.IO;
using System.Text.Json;
using Xunit;
using Service_API.Helpers;
using Repositories.Repositories;
using Entities.Models.Tables;
using Contracts.DTOs.Product;
using Contracts.Responses;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Microsoft.AspNetCore.Http;
using MapsterMapper;

namespace Ohda_Tests
{
    public class RegressionTests
    {
        // 1. JSON Converters Tests
        [Fact]
        public void NullableIntConverter_ShouldDeserializeEmptyStringAsNull()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new NullableIntConverter());

            var json = "{\"value\": \"\"}";
            var result = JsonSerializer.Deserialize<TestModelInt>(json, options);

            Assert.Null(result.Value);
        }

        [Fact]
        public void NullableIntConverter_ShouldDeserializeValidNumberString()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new NullableIntConverter());

            var json = "{\"value\": \"123\"}";
            var result = JsonSerializer.Deserialize<TestModelInt>(json, options);

            Assert.Equal(123, result.Value);
        }

        [Fact]
        public void NullableIntConverter_ShouldDeserializeRawNumber()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new NullableIntConverter());

            var json = "{\"value\": 456}";
            var result = JsonSerializer.Deserialize<TestModelInt>(json, options);

            Assert.Equal(456, result.Value);
        }

        [Fact]
        public void NullableDecimalConverter_ShouldDeserializeEmptyStringAsNull()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new NullableDecimalConverter());

            var json = "{\"value\": \"\"}";
            var result = JsonSerializer.Deserialize<TestModelDecimal>(json, options);

            Assert.Null(result.Value);
        }

        [Fact]
        public void NullableDecimalConverter_ShouldDeserializeValidDecimalString()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new NullableDecimalConverter());

            var json = "{\"value\": \"99.99\"}";
            var result = JsonSerializer.Deserialize<TestModelDecimal>(json, options);

            Assert.Equal(99.99m, result.Value);
        }

        [Fact]
        public void NullableLongConverter_ShouldDeserializeEmptyStringAsNull()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new NullableLongConverter());

            var json = "{\"value\": \"\"}";
            var result = JsonSerializer.Deserialize<TestModelLong>(json, options);

            Assert.Null(result.Value);
        }

        // 2. Repository Key Conversion Reflection Test
        [Fact]
        public void RepositoryBase_ConvertKey_ShouldConvertStringToInt()
        {
            var mockLogger = new Mock<LoggerService.ILoggerManager>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockMapper = new Mock<IMapper>();

            var repository = new TestRepository(mockLogger.Object, null, mockHttpAccessor.Object, mockMapper.Object);

            var methodInfo = typeof(TestRepository).BaseType.GetMethod("ConvertKey", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.NotNull(methodInfo);

            var converted = methodInfo.Invoke(repository, new object[] { "5", typeof(int) });
            Assert.Equal(5, converted);

            var convertedGuid = methodInfo.Invoke(repository, new object[] { "d3b07384-d113-49c6-a5db-6d1b312781b2", typeof(Guid) });
            Assert.Equal(Guid.Parse("d3b07384-d113-49c6-a5db-6d1b312781b2"), convertedGuid);
        }

        // Test classes
        private class TestModelInt
        {
            public int? Value { get; set; }
        }

        private class TestModelDecimal
        {
            public decimal? Value { get; set; }
        }

        private class TestModelLong
        {
            public long? Value { get; set; }
        }

        private class TestRepository : RepositoryBase<Product, ProductDto, ProductCreateDto, ProductUpdateDto>
        {
            public TestRepository(
                LoggerService.ILoggerManager logger, 
                Entities.Models.Databases.RepositoryContext repositoryContext, 
                IHttpContextAccessor httpContextAccessor, 
                IMapper mapper) 
                : base(logger, repositoryContext, httpContextAccessor, mapper)
            {
            }
        }
    }
}
