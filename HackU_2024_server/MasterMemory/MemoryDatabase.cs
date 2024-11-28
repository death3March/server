// <auto-generated />
#pragma warning disable CS0105
using Cysharp.Threading.Tasks;
using HackU_2024_server.DataBase;
using MasterMemory.Validation;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System;
using HackU_2024_server.Tables;

namespace HackU_2024_server
{
   public sealed class MemoryDatabase : MemoryDatabaseBase
   {
        public ClientTable ClientTable { get; private set; }

        public MemoryDatabase(
            ClientTable ClientTable
        )
        {
            this.ClientTable = ClientTable;
        }

        public MemoryDatabase(byte[] databaseBinary, bool internString = true, MessagePack.IFormatterResolver formatterResolver = null, int maxDegreeOfParallelism = 1)
            : base(databaseBinary, internString, formatterResolver, maxDegreeOfParallelism)
        {
        }

        protected override void Init(Dictionary<string, (int offset, int count)> header, System.ReadOnlyMemory<byte> databaseBinary, MessagePack.MessagePackSerializerOptions options, int maxDegreeOfParallelism)
        {
            if(maxDegreeOfParallelism == 1)
            {
                InitSequential(header, databaseBinary, options, maxDegreeOfParallelism);
            }
            else
            {
                InitParallel(header, databaseBinary, options, maxDegreeOfParallelism);
            }
        }

        void InitSequential(Dictionary<string, (int offset, int count)> header, System.ReadOnlyMemory<byte> databaseBinary, MessagePack.MessagePackSerializerOptions options, int maxDegreeOfParallelism)
        {
            this.ClientTable = ExtractTableData<Client, ClientTable>(header, databaseBinary, options, xs => new ClientTable(xs));
        }

        void InitParallel(Dictionary<string, (int offset, int count)> header, System.ReadOnlyMemory<byte> databaseBinary, MessagePack.MessagePackSerializerOptions options, int maxDegreeOfParallelism)
        {
            var extracts = new Action[]
            {
                () => this.ClientTable = ExtractTableData<Client, ClientTable>(header, databaseBinary, options, xs => new ClientTable(xs)),
            };
            
            System.Threading.Tasks.Parallel.Invoke(new System.Threading.Tasks.ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            }, extracts);
        }

        public ImmutableBuilder ToImmutableBuilder()
        {
            return new ImmutableBuilder(this);
        }

        public DatabaseBuilder ToDatabaseBuilder()
        {
            var builder = new DatabaseBuilder();
            builder.Append(this.ClientTable.GetRawDataUnsafe());
            return builder;
        }

        public DatabaseBuilder ToDatabaseBuilder(MessagePack.IFormatterResolver resolver)
        {
            var builder = new DatabaseBuilder(resolver);
            builder.Append(this.ClientTable.GetRawDataUnsafe());
            return builder;
        }

#if !DISABLE_MASTERMEMORY_VALIDATOR

        public ValidateResult Validate()
        {
            var result = new ValidateResult();
            var database = new ValidationDatabase(new object[]
            {
                ClientTable,
            });

            ((ITableUniqueValidate)ClientTable).ValidateUnique(result);
            ValidateTable(ClientTable.All, database, "GlobalUserId", ClientTable.PrimaryKeySelector, result);

            return result;
        }

#endif

        static MasterMemory.Meta.MetaDatabase metaTable;

        public static object GetTable(MemoryDatabase db, string tableName)
        {
            switch (tableName)
            {
                case "Client":
                    return db.ClientTable;
                
                default:
                    return null;
            }
        }

#if !DISABLE_MASTERMEMORY_METADATABASE

        public static MasterMemory.Meta.MetaDatabase GetMetaDatabase()
        {
            if (metaTable != null) return metaTable;

            var dict = new Dictionary<string, MasterMemory.Meta.MetaTable>();
            dict.Add("Client", HackU_2024_server.Tables.ClientTable.CreateMetaTable());

            metaTable = new MasterMemory.Meta.MetaDatabase(dict);
            return metaTable;
        }

#endif
    }
}