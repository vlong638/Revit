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
        #region TDetail
        public static void FetchNodes(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(null, new TNode().TableName, wheres);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entity.Nodes.Add(new TNode(reader));
            }
        }
        #endregion

        #region Methods
        public static bool DbInsert(this TNode entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> NameValues = new Dictionary<string, string>();
            NameValues.Add(nameof(entity.IssueType), SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType));
            NameValues.Add(nameof(entity.IssueDateTime), SQLiteHelper.ToSQLiteString(entity.IssueDateTime));
            NameValues.Add(nameof(entity.NodeCode), SQLiteHelper.ToSQLiteString(entity.NodeCode));
            NameValues.Add(nameof(entity.Data), SQLiteHelper.ToSQLiteString(entity.Data));
            NameValues.Add(nameof(entity.ElementIds), SQLiteHelper.ToSQLiteString(entity.GetElementIds()));
            NameValues.Add(SQLiteHelper.ToSQLiteReservedField(nameof(entity.Index)), SQLiteHelper.ToSQLiteString(entity.Index));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Insert(entity.TableName, NameValues);
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
            Dictionary<string, string> sets = new Dictionary<string, string>();
            sets.Add(nameof(entity.Data), SQLiteHelper.ToSQLiteString(entity.Data));
            sets.Add(nameof(entity.ElementIds), SQLiteHelper.ToSQLiteString(entity.GetElementIds()));
            sets.Add(SQLiteHelper.ToSQLiteReservedField(nameof(entity.Index)), SQLiteHelper.ToSQLiteString(entity.Index));
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            wheres.Add(new KeyOperatorValue(nameof(entity.NodeCode), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.NodeCode)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Update(entity.TableName, sets, wheres);
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
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Delete(entity.TableName, wheres);
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
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            wheres.Add(new KeyOperatorValue(nameof(entity.NodeCode), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.NodeCode)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Delete(entity.TableName, wheres);
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
        public static List<string> GetNodeCodesByIssueType(this TNode entity,EIssueType issueType, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            List<string> selects = new List<string>();
            selects.Add(nameof(entity.NodeCode));
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(issueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(selects, entity.TableName, wheres, "distinct");
            var reader = command.ExecuteReader();
            List<string> results = new List<string>();
            while (reader.Read())
                results.Add(reader[nameof(entity.NodeCode)].ToString());
            return results;
        }
        public static List<DateTimeValue> GetDateTimeValues(this TNode entity, EIssueType issueType, string nodeCode, string fieldName, DateTime startTime, int daySpan, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            List<string> selects = new List<string>();
            selects.Add(nameof(entity.Data));
            selects.Add(nameof(entity.IssueDateTime));
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(issueType)));
            wheres.Add(new KeyOperatorValue(nameof(entity.NodeCode), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(nodeCode)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.GTorEq, SQLiteHelper.ToSQLiteString(startTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.LTorEq, SQLiteHelper.ToSQLiteString(startTime.AddDays(daySpan))));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(selects, entity.TableName, wheres);
            var reader = command.ExecuteReader();
            List<DateTimeValue> results = new List<DateTimeValue>();
            switch (issueType)
            {
                case EIssueType.未指定:
                    break;
                case EIssueType.建筑物沉降:
                    switch (fieldName)
                    {
                        case nameof(BuildingSubsidenceDataV1.SumChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new BuildingSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumChanges,out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(BuildingSubsidenceDataV1.CurrentChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new BuildingSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.CurrentChanges, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(BuildingSubsidenceDataV1.SumBuildingEnvelope):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new BuildingSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumBuildingEnvelope, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(BuildingSubsidenceDataV1.SumPeriodBuildingEnvelope):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new BuildingSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumPeriodBuildingEnvelope, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case EIssueType.地表沉降:
                    switch (fieldName)
                    {
                        case nameof(SurfaceSubsidenceDataV1.SumChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new SurfaceSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumChanges, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(SurfaceSubsidenceDataV1.CurrentChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new SurfaceSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.CurrentChanges, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case EIssueType.管线沉降_有压:
                    switch (fieldName)
                    {
                        case nameof(PressedPipeLineSubsidenceDataV1.SumChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new PressedPipeLineSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumChanges, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(PressedPipeLineSubsidenceDataV1.CurrentChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new PressedPipeLineSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.CurrentChanges, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(PressedPipeLineSubsidenceDataV1.SumPeriodBuildingEnvelope):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new PressedPipeLineSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumPeriodBuildingEnvelope, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(PressedPipeLineSubsidenceDataV1.SumBuildingEnvelope):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new PressedPipeLineSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumBuildingEnvelope, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case EIssueType.管线沉降_无压:
                    switch (fieldName)
                    {
                        case nameof(UnpressedPipeLineSubsidenceDataV1.SumChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new UnpressedPipeLineSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumChanges, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(UnpressedPipeLineSubsidenceDataV1.CurrentChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new UnpressedPipeLineSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.CurrentChanges, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(UnpressedPipeLineSubsidenceDataV1.SumPeriodBuildingEnvelope):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new UnpressedPipeLineSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumPeriodBuildingEnvelope, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(UnpressedPipeLineSubsidenceDataV1.SumBuildingEnvelope):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new UnpressedPipeLineSubsidenceDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumBuildingEnvelope, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case EIssueType.钢支撑轴力监测:
                    switch (fieldName)
                    {
                        case nameof(STBAPDataV1.SumChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new STBAPDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.SumChanges, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        case nameof(STBAPDataV1.CurrentChanges):
                            while (reader.Read())
                            {
                                var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                                var data = new STBAPDataV1(nodeCode, reader[nameof(entity.Data)].ToString());
                                double value;
                                if (double.TryParse(data.CurrentChanges, out value))
                                    results.Add(new DateTimeValue(time, value));
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return results;
        }
        #endregion
    }
}
