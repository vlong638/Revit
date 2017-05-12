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
        /// <summary>
        /// 首日用与时间区间的判断,并不进行节点数据的获取
        /// </summary>
        public static void GetDetailsByTimeRange(this List<TDetail> entities, SQLiteConnection connection, EIssueType issueType, DateTime start, DateTime end)
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
                entity.FetchNodes(connection);
                //这里可以做一次优化,所有一次取出,本地匹配处理
            }
        }
        #endregion
    }
}
