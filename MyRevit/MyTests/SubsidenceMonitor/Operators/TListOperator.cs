using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.Utilities;
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
            command.CommandText = SQLiteHelper.GetSQLiteQuery_InsertOrReplace(entity.TableName, NameValues);
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbUpdate(this TList entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> sets = new Dictionary<string, string>();
            sets.Add(nameof(entity.DataCount), SQLiteHelper.ToSQLiteString(entity.DataCount));
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDate), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDate)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Update(new TList().TableName, sets, wheres);
            return command.ExecuteNonQuery() == 1;
        }
        public static void GetListsByKeys(this List<TList> lists, SQLiteConnection connection, EIssueType issueType, DateTime issueDateTime)
        {
            var command = connection.CreateCommand();
            var entity = new TList();
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue($"datetime({nameof(entity.IssueDate)},'start of month')", SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(issueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(issueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(null, entity.TableName, wheres);
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
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue($"datetime({nameof(detail.IssueDateTime)},'start of day')", SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(list.IssueDate)));
            wheres.Add(new KeyOperatorValue(nameof(detail.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(list.IssueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(null, detail.TableName, wheres);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Datas.Add(new TDetail(reader) { List = list });
            }
        }
        #endregion
    }
}
