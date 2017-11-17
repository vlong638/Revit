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
            if (entity.ExtraValue1.HasValue)
                NameValues.Add(nameof(entity.ExtraValue1), SQLiteHelper.ToSQLiteString(entity.ExtraValue1.Value));
            if (entity.ExtraValue2.HasValue)
                NameValues.Add(nameof(entity.ExtraValue2), SQLiteHelper.ToSQLiteString(entity.ExtraValue2.Value));
            if (entity.ExtraValue3.HasValue)
                NameValues.Add(nameof(entity.ExtraValue3), SQLiteHelper.ToSQLiteString(entity.ExtraValue3.Value));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Insert(new TDetail().TableName, NameValues);
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbUpdate(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            Dictionary<string, string> sets = new Dictionary<string, string>();
            sets.Add(nameof(entity.ReportName), SQLiteHelper.ToSQLiteString(entity.ReportName));
            sets.Add(nameof(entity.IssueTimeRange), SQLiteHelper.ToSQLiteString(entity.IssueTimeRange));
            sets.Add(nameof(entity.Contractor), SQLiteHelper.ToSQLiteString(entity.Contractor));
            sets.Add(nameof(entity.Supervisor), SQLiteHelper.ToSQLiteString(entity.Supervisor));
            sets.Add(nameof(entity.Monitor), SQLiteHelper.ToSQLiteString(entity.Monitor));
            sets.Add(nameof(entity.InstrumentName), SQLiteHelper.ToSQLiteString(entity.InstrumentName));
            sets.Add(nameof(entity.InstrumentCode), SQLiteHelper.ToSQLiteString(entity.InstrumentCode));
            sets.Add(nameof(entity.CloseCTSettings), SQLiteHelper.ToSQLiteString(entity.CloseCTSettings));
            sets.Add(nameof(entity.OverCTSettings), SQLiteHelper.ToSQLiteString(entity.OverCTSettings));
            if (entity.ExtraValue1.HasValue)
                sets.Add(nameof(entity.ExtraValue1), SQLiteHelper.ToSQLiteString(entity.ExtraValue1.Value));
            if (entity.ExtraValue2.HasValue)
                sets.Add(nameof(entity.ExtraValue2), SQLiteHelper.ToSQLiteString(entity.ExtraValue2.Value));
            if (entity.ExtraValue3.HasValue)
                sets.Add(nameof(entity.ExtraValue3), SQLiteHelper.ToSQLiteString(entity.ExtraValue3.Value));
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Update(new TDetail().TableName, sets, wheres);
            return command.ExecuteNonQuery() == 1;
        }
        public static bool DbDelete(this TDetail entity, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString(entity.IssueDateTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(entity.IssueType)));
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Delete(new TDetail().TableName, wheres);
            return command.ExecuteNonQuery() == 1;
        }
        /// <summary>
        /// 首日用与时间区间的判断,并不进行节点数据的获取
        /// </summary>
        public static void GetNodeDetailsByTimeRange(this List<TDetail> entities, SQLiteConnection connection, EIssueType issueType, DateTime start, DateTime end)
        {
            var command = connection.CreateCommand();
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(TDetail.IssueDateTime), SQLiteOperater.LT, SQLiteHelper.ToSQLiteString(end)));
            var weightedStart = start.AddDays(-1).Date;
            wheres.Add(new KeyOperatorValue(nameof(TDetail.IssueDateTime), SQLiteOperater.GT, SQLiteHelper.ToSQLiteString(weightedStart)));//往前多取一天
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(null, new TDetail().TableName, wheres);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entities.Add(new TDetail(reader));
            }
            foreach (var entity in entities)
            {
                if (entity.IssueDateTime.Date == weightedStart)
                    continue;
            }
        }
        public static List<DateTimeValue> GetDateTimeValue(this TDetail entity, EIssueType issueType, DateTime startTime, int daySpan, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            List<string> selects = new List<string>();
            selects.Add(nameof(entity.IssueDateTime));
            selects.Add(nameof(entity.ExtraValue3));
            List<KeyOperatorValue> wheres = new List<KeyOperatorValue>();
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueType), SQLiteOperater.Eq, SQLiteHelper.ToSQLiteString<EIssueType>(issueType)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.GTorEq, SQLiteHelper.ToSQLiteString(startTime)));
            wheres.Add(new KeyOperatorValue(nameof(entity.IssueDateTime), SQLiteOperater.LT, SQLiteHelper.ToSQLiteString(startTime.AddDays(daySpan + 10))));//+10作预留区间允许其中有个10天的空档
            command.CommandText = SQLiteHelper.GetSQLiteQuery_Select(selects, entity.TableName, wheres);
            var reader = command.ExecuteReader();
            List<DateTimeValue> results = new List<DateTimeValue>();
            switch (issueType)
            {
                case EIssueType.钢支撑轴力监测:
                    while (reader.Read())
                    {
                        DateTimeValue data = new DateTimeValue();
                        var time = DateTime.Parse(reader[nameof(entity.IssueDateTime)].ToString());
                        data.DateTime = time;
                        double value;
                        if (double.TryParse(reader[nameof(entity.ExtraValue3)].ToString(), out value))
                            data.Value = value;
                        results.Add(data);
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
