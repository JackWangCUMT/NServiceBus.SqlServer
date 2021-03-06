#pragma warning disable 618
namespace NServiceBus.Transport.SQLServer
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Transport;

    class QueueCreator : ICreateQueues
    {
        public QueueCreator(SqlConnectionFactory connectionFactory, QueueAddressParser addressParser)
        {
            this.connectionFactory = connectionFactory;
            this.addressParser = addressParser;
        }

        public async Task CreateQueueIfNecessary(QueueBindings queueBindings, string identity)
        {
            using (var connection = await connectionFactory.OpenNewConnection().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var receivingAddress in queueBindings.ReceivingAddresses)
                {
                    await CreateQueue(addressParser.Parse(receivingAddress), connection, transaction).ConfigureAwait(false);
                }

                foreach (var receivingAddress in queueBindings.SendingAddresses)
                {
                    await CreateQueue(addressParser.Parse(receivingAddress), connection, transaction).ConfigureAwait(false);
                }

                transaction.Commit();
            }
        }

        static async Task CreateQueue(QueueAddress address, SqlConnection connection, SqlTransaction transaction)
        {
            string tableName;
            string schemaName;
            using (var sanitizer = new SqlCommandBuilder())
            {
                tableName = sanitizer.QuoteIdentifier(address.TableName);
                schemaName = sanitizer.QuoteIdentifier(address.SchemaName);
            }
            var sql = string.Format(SqlConstants.CreateQueueText, schemaName, tableName);
            using (var command = new SqlCommand(sql, connection, transaction)
            {
                CommandType = CommandType.Text
            })
            {
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        SqlConnectionFactory connectionFactory;
        QueueAddressParser addressParser;
    }
}