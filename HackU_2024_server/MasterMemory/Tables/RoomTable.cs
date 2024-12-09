// <auto-generated />
#pragma warning disable CS0105
using HackU_2024_server.DataBase;
using MasterMemory.Validation;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;

namespace HackU_2024_server.Tables
{
   public sealed partial class RoomTable : TableBase<Room>, ITableUniqueValidate
   {
        public Func<Room, string> PrimaryKeySelector => primaryIndexSelector;
        readonly Func<Room, string> primaryIndexSelector;


        public RoomTable(Room[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.RoomName;
            OnAfterConstruct();
        }

        partial void OnAfterConstruct();


        public Room FindByRoomName(string key)
        {
            return FindUniqueCore(data, primaryIndexSelector, System.StringComparer.Ordinal, key, false);
        }
        
        public bool TryFindByRoomName(string key, out Room result)
        {
            return TryFindUniqueCore(data, primaryIndexSelector, System.StringComparer.Ordinal, key, out result);
        }

        public Room FindClosestByRoomName(string key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.StringComparer.Ordinal, key, selectLower);
        }

        public RangeView<Room> FindRangeByRoomName(string min, string max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.StringComparer.Ordinal, min, max, ascendant);
        }


        void ITableUniqueValidate.ValidateUnique(ValidateResult resultSet)
        {
#if !DISABLE_MASTERMEMORY_VALIDATOR

            ValidateUniqueCore(data, primaryIndexSelector, "RoomName", resultSet);       

#endif
        }

#if !DISABLE_MASTERMEMORY_METADATABASE

        public static MasterMemory.Meta.MetaTable CreateMetaTable()
        {
            return new MasterMemory.Meta.MetaTable(typeof(Room), typeof(RoomTable), "Room",
                new MasterMemory.Meta.MetaProperty[]
                {
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("RoomName")),
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("UserIDs")),
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("UserOrder")),
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("UserOtoshidama")),
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("UserPosition")),
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("UserIsAnswered")),
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("UserAnswer")),
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("QuizData")),
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("State")),
                    new MasterMemory.Meta.MetaProperty(typeof(Room).GetProperty("SugorokuMap")),
                },
                new MasterMemory.Meta.MetaIndex[]{
                    new MasterMemory.Meta.MetaIndex(new System.Reflection.PropertyInfo[] {
                        typeof(Room).GetProperty("RoomName"),
                    }, true, true, System.StringComparer.Ordinal),
                });
        }

#endif
    }
}