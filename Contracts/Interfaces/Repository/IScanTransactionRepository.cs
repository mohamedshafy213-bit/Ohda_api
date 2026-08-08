using Contracts.DTOs.ScanTransaction;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IScanTransactionRepository : IRepositoryBase<ScanTransaction, ScanTransactionDto, ScanTransactionCreateDto, ScanTransactionUpdateDto>
{
}
