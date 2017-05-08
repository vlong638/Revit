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
        public static bool DbInsert(this TNode entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> NameValues = new Dictionary<string, string>();
            NameValues.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            NameValues.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            NameValues.Add(nameof(entity.NodeCode), SQLiteHelper.ToSQLiteString(entity.NodeCode));
            NameValues.Add(nameof(entity.Data), SQLiteHelper.ToSQLiteString(entity.Data));
            NameValues.Add(nameof(entity.ElementIds), SQLiteHelper.ToSQLiteString(string.Join(",", entity.ElementIds_Int)));
            NameValues.Add($"[{nameof(entity.Index)}]", $"'{entity.Index.ToString()}'");
            command.CommandText = $"insert into {entity.TableName}({string.Join(",", NameValues.Keys)}) values({string.Join(",", NameValues.Values)})";
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbInsert(this List<TNode> entities, SQLiteConnection connection)
        {
            foreach (var entity in entities)
            {
                if (!entity.DbInsert(connection))
                    return false;
            }
            return true;
        }
        public static bool DbUpdate(this TNode entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> Sets = new Dictionary<string, string>();
            Sets.Add(nameof(entity.Data), SQLiteHelper.ToSQLiteString(entity.Data));
            Sets.Add(nameof(entity.ElementIds), SQLiteHelper.ToSQLiteString(string.Join(",", entity.ElementIds_Int)));
            Sets.Add($"[{nameof(entity.Index)}]", $"'{entity.Index.ToString()}'");
            Dictionary<string, string> Wheres = new Dictionary<string, string>();
            Wheres.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            Wheres.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            Wheres.Add(nameof(entity.NodeCode), SQLiteHelper.ToSQLiteString(entity.NodeCode));
            command.CommandText = $"update {entity.TableName} set {SQLiteHelper.ToSQLiteSets(Sets)} where {SQLiteHelper.ToSQLiteWheres(Wheres)}";
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbUpdate(this List<TNode> entities, SQLiteConnection connection)
        {
            foreach (var entity in entities)
            {
                if (!entity.DbUpdate(connection))
                    return false;
            }
            return true;
        }
        static int dbDeleteByDetailKey(this TNode entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> NameValues = new Dictionary<string, string>();
            NameValues.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            NameValues.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            command.CommandText = $"delete from {entity.TableName} where {string.Join(" and ", NameValues.Select(c => c.Key + "=" + c.Value))}";
            return command.ExecuteNonQuery();
        }
        public static int DbDeleteByDetailKey(this TNode entity, SQLiteConnection connection)
        {
            return entity.dbDeleteByDetailKey(connection);
        }
        public static bool DbDeleteByDetailKey(this List<TNode> entities, SQLiteConnection connection)
        {
            return entities.First().dbDeleteByDetailKey(connection) == entities.Count();
        }
        static int dbDelete(this TNode entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> NameValues = new Dictionary<string, string>();
            NameValues.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            NameValues.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            NameValues.Add(nameof(entity.NodeCode), SQLiteHelper.ToSQLiteString(entity.NodeCode));
            command.CommandText = $"delete from {entity.TableName} where {string.Join(" and ", NameValues.Select(c => c.Key + "=" + c.Value))}";
            return command.ExecuteNonQuery();
        }
        public static bool DbDelete(this TNode entity, SQLiteConnection connection)
        {
            return entity.dbDelete(connection) == 1;
        }
        public static bool DbDelete(this List<TNode> entities, SQLiteConnection connection)
        {
            foreach (var entity in entities)
            {
                if (!entity.DbDelete(connection))
                    return false;
            }
            return true;
        }
        #endregion
    }
}
