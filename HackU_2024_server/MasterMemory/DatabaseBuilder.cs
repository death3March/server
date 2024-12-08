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
   public sealed class DatabaseBuilder : DatabaseBuilderBase
   {
        public DatabaseBuilder() : this(null) { }
        public DatabaseBuilder(MessagePack.IFormatterResolver resolver) : base(resolver) { }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<Client> dataSource)
        {
            AppendCore(dataSource, x => x.GlobalUserId, System.StringComparer.Ordinal);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<Room> dataSource)
        {
            AppendCore(dataSource, x => x.RoomName, System.StringComparer.Ordinal);
            return this;
        }

    }
}