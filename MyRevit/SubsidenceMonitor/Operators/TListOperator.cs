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
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Insert(entity.TableName, NameValues);
            return command.ExecuteNonQuery() == 1;
        }
        public static void GetListsByKeys(this List<TList> lists, SQLiteConnection connection, EIssueType issueType, DateTime issueDateTime)
        {
            var command = connection.CreateCommand();
            var entity = new TList();
            Dictionary<string, string> Wheres = new Dictionary<string, string>();
            Wheres.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(issueType));
            Wheres.Add($"datetime({nameof(entity.IssueDate)},'start of month')", SQLiteHelper.ToSQLiteString(issueDateTime));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(null, entity.TableName, Wheres);
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
            Dictionary<string, string> Wheres = new Dictionary<string, string>();
            Wheres.Add(nameof(detail.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(list.IssueType));
            Wheres.Add($"datetime({nameof(detail.IssueDateTime)},'start of day')", SQLiteHelper.ToSQLiteString(list.IssueDate));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(null, detail.TableName, Wheres);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Datas.Add(new TDetail(reader) { List = list });
            }
        }
        #endregion
    }
}
