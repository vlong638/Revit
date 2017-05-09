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
        public static bool DbInsert(this TDetail entity, SQLiteConnection connection)
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
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Insert(entity.TableName, NameValues);
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbUpdate(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> Sets = new Dictionary<string, string>();
            Sets.Add(nameof(entity.ReportName), SQLiteHelper.ToSQLiteString(entity.ReportName));
            Sets.Add(nameof(entity.IssueTimeRange), SQLiteHelper.ToSQLiteString(entity.IssueTimeRange));
            Sets.Add(nameof(entity.Contractor), SQLiteHelper.ToSQLiteString(entity.Contractor));
            Sets.Add(nameof(entity.Supervisor), SQLiteHelper.ToSQLiteString(entity.Supervisor));
            Sets.Add(nameof(entity.Monitor), SQLiteHelper.ToSQLiteString(entity.Monitor));
            Sets.Add(nameof(entity.InstrumentName), SQLiteHelper.ToSQLiteString(entity.InstrumentName));
            Sets.Add(nameof(entity.InstrumentCode), SQLiteHelper.ToSQLiteString(entity.InstrumentCode));
            Sets.Add(nameof(entity.CloseCTSettings), SQLiteHelper.ToSQLiteString(entity.CloseCTSettings));
            Sets.Add(nameof(entity.OverCTSettings), SQLiteHelper.ToSQLiteString(entity.OverCTSettings));
            Dictionary<string, string> Wheres = new Dictionary<string, string>();
            Wheres.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            Wheres.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Update(entity.TableName, Sets, Wheres);
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbDelete(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> NameValues = new Dictionary<string, string>();
            NameValues.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            NameValues.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Delete(entity.TableName,  NameValues);
            return command.ExecuteNonQuery() == 1;
        }
        public static void FetchNodes(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            var node = new TNode();
            Dictionary<string, string> Wheres = new Dictionary<string, string>();
            Wheres.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            Wheres.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(null, node.TableName, Wheres);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entity.Nodes.Add(new TNode(reader));
            }
        }
        #endregion
    }
}
