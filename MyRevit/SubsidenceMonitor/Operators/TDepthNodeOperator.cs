using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.Utilities;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace MyRevit.SubsidenceMonitor.Operators
{
    public static partial class EntityOperator
    {
        #region TDetail
        public static void FetchDepthNodes(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(null, new TDepthNode().TableName, wheres);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entity.DepthNodes.Add(new TDepthNode(reader));
            }
        }
        #endregion

        #region Methods
        public static bool DbInsert(this TDepthNode entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> NameValues = new Dictionary<string, string>();
            NameValues.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            NameValues.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            NameValues.Add(nameof(entity.NodeCode), SQLiteHelper.ToSQLiteString(entity.NodeCode));
            NameValues.Add(nameof(entity.Depth), SQLiteHelper.ToSQLiteString(entity.Depth));
            NameValues.Add(nameof(entity.Data), SQLiteHelper.ToSQLiteString(entity.Data));
            NameValues.Add(nameof(entity.ElementIds), SQLiteHelper.ToSQLiteString(entity.GetElementIds()));
            NameValues.Add(SQLiteHelper.ToSQLiteReservedField(nameof(entity.Index)), SQLiteHelper.ToSQLiteString(entity.Index));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Insert(entity.TableName, NameValues);
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbInsert(this List<TDepthNode> entities, SQLiteConnection connection)
        {
            foreach (var entity in entities)
            {
                if (!entity.DbInsert(connection))
                    return false;
            }
            return true;
        }
        public static bool DbUpdate(this TDepthNode entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> sets = new Dictionary<string, string>();
            sets.Add(nameof(entity.Data), SQLiteHelper.ToSQLiteString(entity.Data));
            sets.Add(nameof(entity.ElementIds), SQLiteHelper.ToSQLiteString(entity.GetElementIds()));
            sets.Add(SQLiteHelper.ToSQLiteReservedField(nameof(entity.Index)), SQLiteHelper.ToSQLiteString(entity.Index));
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            wheres.Add(new KeyOperatorValue(nameof(entity.NodeCode), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.NodeCode)));
            wheres.Add(new KeyOperatorValue(nameof(entity.Depth), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.Depth)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Update(entity.TableName, sets, wheres);
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbUpdate(this List<TDepthNode> entities, SQLiteConnection connection)
        {
            foreach (var entity in entities)
            {
                if (!entity.DbUpdate(connection))
                    return false;
            }
            return true;
        }
        static int dbDeleteByDetailKey(this TDepthNode entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Delete(entity.TableName, wheres);
            return command.ExecuteNonQuery();
        }
        public static int DbDeleteByDetailKey(this TDepthNode entity, SQLiteConnection connection)
        {
            return entity.dbDeleteByDetailKey(connection);
        }
        public static bool DbDeleteByDetailKey(this List<TDepthNode> entities, SQLiteConnection connection)
        {
            return entities.First().dbDeleteByDetailKey(connection) == entities.Count();
        }
        static int dbDelete(this TDepthNode entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            wheres.Add(new KeyOperatorValue(nameof(entity.NodeCode), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.NodeCode)));
            wheres.Add(new KeyOperatorValue(nameof(entity.Depth), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.Depth)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Delete(entity.TableName, wheres);
            return command.ExecuteNonQuery();
        }
        public static bool DbDelete(this TDepthNode entity, SQLiteConnection connection)
        {
            return entity.dbDelete(connection) == 1;
        }
        public static bool DbDelete(this List<TDepthNode> entities, SQLiteConnection connection)
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
