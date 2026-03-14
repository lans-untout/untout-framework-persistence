using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Untout.Framework.Persistence.Interfaces;

internal interface ITransactionScopeFactory
{
    Task<IDbTransactionScope> CreateAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken);
}