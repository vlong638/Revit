using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.SubsidenceMonitor.Utilities;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace MyRevit.SubsidenceMonitor.Operators
{
    public static partial class EntityOperator
    {
        #region Methods
        public static bool DbInsertOrReplace(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> NameValues = new Dictionary<string, string>();
            NameValues.Add(nameof(entity.ReportName), SQLiteHelper.ToSQLiteString(entity.ReportName));
            NameValues.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            NameValues.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            NameValues.Add(nameof(entity.IssueTimeRange), SQLiteHelper.ToSQLiteString(entity.IssueTimeRange));
            NameValues.Add(nameof(entity.Contractor), SQLiteHelper.ToSQLiteString(entity.Contractor));
            NameValues.Add(nameof(entity.Supervisor), SQLiteHelper.ToSQLiteString(entity.Supervisor));
            NameValues.Add(nameof(entity.Monitor), SQLiteHelper.ToSQLiteString(entity.Monitor));
            NameValues.Add(nameof(entity.InstrumentName), SQLiteHelper.ToSQLiteString(entity.InstrumentName));
            NameValues.Add(nameof(entity.InstrumentCode), SQLiteHelper.ToSQLiteString(entity.InstrumentCode));
            NameValues.Add(nameof(entity.CloseCTSettings), SQLiteHelper.ToSQLiteString(entity.CloseCTSettings));
            NameValues.Add(nameof(entity.OverCTSettings), SQLiteHelper.ToSQLiteString(entity.OverCTSettings));
            command.CommandText = $"insert or replace into {entity.TableName}({string.Join(",", NameValues.Keys)}) values({string.Join(",", NameValues.Values)})";
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbDelete(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> NameValues = new Dictionary<string, string>();
            NameValues.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            NameValues.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            command.CommandText = $"delete from {entity.TableName} where {string.Join(" and ", NameValues.Select(c => c.Key + "=" + c.Value))}";
            return command.ExecuteNonQuery() == 1;
        }
        public static void FetchNodes(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            var node = new TNode();
            command.CommandText = $"select * from {node.TableName} where {nameof(node.IssueType)}=={SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)} and {nameof(node.IssueDateTime)} =={SQLiteHelper.ToSQLiteString(entity.IssueDateTime)}";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entity.Nodes.Add(new TNode(reader));
            }
        }
        #endregion
    }
}
