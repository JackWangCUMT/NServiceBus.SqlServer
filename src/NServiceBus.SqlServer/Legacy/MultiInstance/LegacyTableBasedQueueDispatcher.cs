namespace NServiceBus.Transport.SQLServer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Transactions;

    class LegacyTableBasedQueueDispatcher : IQueueDispatcher
    {
        public LegacyTableBasedQueueDispatcher(LegacySqlConnectionFactory connectionFactory, LegacyQueueAddressTranslator addressTranslator)
        {
            this.connectionFactory = connectionFactory;
            this.addressTranslator = addressTranslator;
        }

        public virtual async Task DispatchAsNonIsolated(List<UnicastTransportOperation> operations, TransportTransaction transportTransaction)
        {
            //If dispatch is not isolated then either TS has been created by the receive operation or needs to be created here.
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var operation in operations)
                {
                    var address = addressTranslator.Parse(operation.Destination);
                    var queue = new TableBasedQueue(address.QualifiedTableName, address.Address);
                    using (var connection = await connectionFactory.OpenNewConnection(queue.Name).ConfigureAwait(false))
                    {
                        await queue.Send(operation.Message.Headers, operation.Message.Body, connection, null).ConfigureAwait(false);
                    }
                }
                scope.Complete();
            }
        }

        public virtual async Task DispatchAsIsolated(List<UnicastTransportOperation> operations)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var operation in operations)
                {
                    var address = addressTranslator.Parse(operation.Destination);
                    var queue = new TableBasedQueue(address.QualifiedTableName, address.Address);
                    using (var connection = await connectionFactory.OpenNewConnection(queue.Name).ConfigureAwait(false))
                    {
                        await queue.Send(operation.Message.Headers, operation.Message.Body, connection, null).ConfigureAwait(false);
                    }
                }
                scope.Complete();
            }
        }

        TableBasedQueueFactory queueFactory = new TableBasedQueueFactory();
        LegacySqlConnectionFactory connectionFactory;
        LegacyQueueAddressTranslator addressTranslator;
    }
}