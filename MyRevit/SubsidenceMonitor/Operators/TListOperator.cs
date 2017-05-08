using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.SubsidenceMonitor.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace MyRevit.SubsidenceMonitor.Operators
{
    public static partial class EntityOperator
    {
        #region Methods
        public static bool DbInsertOrReplace(this TList entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> NameValues = new Dictionary<string, string>();
            NameValues.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            NameValues.Add(nameof(entity.IssueDate), SQLiteHelper.ToSQLiteString(entity.IssueDate));
            NameValues.Add(nameof(entity.DataCount), SQLiteHelper.ToSQLiteString(entity.Datas.Count()));
            command.CommandText = $"insert {entity.TableName}({string.Join(",", NameValues.Keys)}) values({string.Join(",", NameValues.Values)})";
            return command.ExecuteNonQuery() == 1;
        }
        public static void GetListsByKeys(this List<TList> lists, SQLiteConnection connection, EIssueType issueType, DateTime date)
        {
            var command = connection.CreateCommand();
            var list = new TList();
            command.CommandText = $"select * from {list.TableName} where {nameof(list.IssueType)}=={SQLiteHelper.ToSQLiteString<EIssueType>(issueType)} and datetime({nameof(list.IssueDate)},'start of month')=={SQLiteHelper.ToSQLiteString(date)}";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lists.Add(new TList(reader));
            }
        }
        public static void FetchDetails(this TList list, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            var detail = new TDetail();
            command.CommandText = $"select * from {detail.TableName} where {nameof(detail.IssueType)}=={SQLiteHelper.ToSQLiteString<EIssueType>(list.IssueType)} and datetime({nameof(detail.IssueDateTime)},'start of day') =={SQLiteHelper.ToSQLiteString(list.IssueDate)}";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Datas.Add(new TDetail(reader) { List = list });
            }
        }
        #endregion
    }
}
