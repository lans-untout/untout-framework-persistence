using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Untout.Framework.Persistence.Interfaces;

namespace Untout.Framework.Persistence.PostgreSql
{
    internal class TransactionScopeFactory : ITransactionScopeFactory
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TransactionScopeFactory(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IDbTransactionScope> CreateAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken)
        {
            if (await _connectionFactory.CreateConnectionAsync(cancellationToken) is DbConnection connection)
            {
                var transaction = await connection.BeginTransactionAsync(isolationLevel, cancellationToken);
                return new NpgsqlTransactionScope(connection, transaction);
            }

            throw new InvalidOperationException("The connection factory did not return a valid DbConnection.");
        }
    }
}
