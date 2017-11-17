using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.SubsidenceMonitor.Operators
{
    public class Facade
    {
        #region Write
        public static BLLResult CreateDetail(TList list, TDetail detail, List<TNode> nodes)
        {
            var result = new BLLResult();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    list.DataCount = (short)list.Datas.Count;
                    if (!list.DbInsertOrReplace(connection))
                        throw new NotImplementedException("List数据未按预期存储");
                    if (!detail.DbInsert(connection))
                        throw new NotImplementedException("Detail数据未按预期存储");
                    //nodes.DbDeleteByDetailKey(connection);//由于提交时,Nodes数量发生了变更,这里省略对原有Nodes数量删除的监测
                    if (!nodes.DbInsert(connection))
                        throw new NotImplementedException("Nodes数据未按预期存储");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                }
                connection.Close();
            }
            return result;
        }
        public static BLLResult CreateDetail(TList list, TDetail detail, List<TDepthNode> nodes)
        {
            var result = new BLLResult();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    list.DataCount = (short)list.Datas.Count;
                    if (!list.DbInsertOrReplace(connection))
                        throw new NotImplementedException("List数据未按预期存储");
                    if (!detail.DbInsert(connection))
                        throw new NotImplementedException("Detail数据未按预期存储");
                    //nodes.DbDeleteByDetailKey(connection);//由于提交时,Nodes数量发生了变更,这里省略对原有Nodes数量删除的监测
                    if (!nodes.DbInsert(connection))
                        throw new NotImplementedException("Nodes数据未按预期存储");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                }
                connection.Close();
            }
            return result;
        }
        public static BLLResult UpdateDetail(TDetail detail, List<TNode> nodes)
        {
            var result = new BLLResult();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    if (!detail.DbUpdate(connection))
                        throw new NotImplementedException("Detail数据未按预期存储");
                    if (!nodes.DbUpdate(connection))
                        throw new NotImplementedException("Nodes数据未按预期存储");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                }
                connection.Close();
            }
            return result;
        }
        public static BLLResult UpdateDetail(TDetail detail, List<TDepthNode> nodes)
        {
            var result = new BLLResult();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    if (!detail.DbUpdate(connection))
                        throw new NotImplementedException("Detail数据未按预期存储");
                    if (!nodes.DbUpdate(connection))
                        throw new NotImplementedException("Nodes数据未按预期存储");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                }
                connection.Close();
            }
            return result;
        }
        public static BLLResult DeleteDetail(TList list,TDetail detail, List<TNode> nodes)
        {
            var result = new BLLResult();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    if (!detail.DbDelete(connection))
                        throw new NotImplementedException("Detail数据未成功删除");
                    if (!nodes.DbDeleteByDetailKey(connection))
                        throw new NotImplementedException("Nodes数据未成功删除");
                    list.Datas.Remove(detail);
                    list.DataCount = (short)list.Datas.Count();
                    if (!list.DbUpdate(connection))
                        throw new NotImplementedException("List数据未成功更新");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                }
                connection.Close();
            }
            return result;
        }
        public static BLLResult DeleteDetail(TList list, TDetail detail, List<TDepthNode> nodes)
        {
            var result = new BLLResult();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    if (!detail.DbDelete(connection))
                        throw new NotImplementedException("Detail数据未成功删除");
                    if (!nodes.DbDeleteByDetailKey(connection))
                        throw new NotImplementedException("Nodes数据未成功删除");
                    list.Datas.Remove(detail);
                    list.DataCount = (short)list.Datas.Count();
                    if (!list.DbUpdate(connection))
                        throw new NotImplementedException("List数据未成功更新");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                }
                connection.Close();
            }
            return result;
        }
        #endregion

        #region Read
        public static void FetchDetails(TList list)
        {
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                list.FetchDetails(connection);
                connection.Close();
            }
        }
        public static void FetchNodes(TDetail detail)
        {
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                detail.FetchNodes(connection);
                connection.Close();
            }
        }
        public static void FetchDepthNodes(TDetail detail)
        {
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                detail.FetchDepthNodes(connection);
                connection.Close();
            }
        }
        public static List<TList> GetLists(EIssueType issueType, DateTime date)
        {
            List<TList> result = new List<TList>();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                result.GetListsByKeys(connection, issueType, date);
                connection.Close();
            }
            return result;
        }
        public static List<TDetail> GetDetailsByTimeRange(EIssueType issueType, DateTime start, DateTime end)
        {
            List<TDetail> result = new List<TDetail>();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                result.GetNodeDetailsByTimeRange(connection, issueType, start, end);
                //这里可以做一次优化,所有一次取出,本地匹配处理
                var d = result.FirstOrDefault();
                if (d != null && d.IssueType == EIssueType.侧斜监测)
                    foreach (var detail in result)
                        detail.FetchDepthNodes(connection);
                else
                    foreach (var detail in result)
                        detail.FetchNodes(connection);
                connection.Close();
            }
            return result;
        }
        public static List<string> GetNodeCodesByType(EIssueType issueType)
        {
            List<string> result;
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                result = new TNode().GetNodeCodesByIssueType(issueType, connection);
                connection.Close();
            }
            return result;
        }
        public static List<string> GetDepthsByNodeCode(string nodeCode)
        {
            List<string> result;
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                result = new TDepthNode().GetDepthsByNodeCode(nodeCode, connection);
                connection.Close();
            }
            return result;
        }
        public static List<DateTimeValue> GetDateTimeValues(EIssueType issueType,DateTime startTime, int daySpan)
        {
            List<DateTimeValue> result;
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                switch (issueType)
                {
                    case EIssueType.侧斜监测:
                        result = new TDetail().GetDateTimeValue(issueType, startTime, daySpan, connection);
                        break;
                    case EIssueType.建筑物沉降:
                    case EIssueType.地表沉降:
                    case EIssueType.管线沉降_有压:
                    case EIssueType.管线沉降_无压:
                    case EIssueType.钢支撑轴力监测:
                    default:
                        throw new NotImplementedException("该方法暂不支持该类型");
                }
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// 日周期化的数据(TNode数据)
        /// </summary>
        public static List<DateTimeValue> GetDateTimeValues(EIssueType issueType, string nodeCode,string fieldName,DateTime startTime, int daySpan)
        {
            List<DateTimeValue> result;
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                switch (issueType)
                {
                    case EIssueType.建筑物沉降:
                    case EIssueType.地表沉降:
                    case EIssueType.管线沉降_有压:
                    case EIssueType.管线沉降_无压:
                    case EIssueType.钢支撑轴力监测:
                        result = new TNode().GetDateTimeValues(issueType, nodeCode, fieldName, startTime, daySpan, connection);
                        break;
                    case EIssueType.侧斜监测:
                    default:
                        throw new NotImplementedException("该方法暂不支持该类型");
                }
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// 日周期化的数据(TNode数据)
        /// </summary>
        public static List<DateTimeValue> GetDateTimeValues(EIssueType issueType, string nodeCode, string depth, string fieldName, DateTime startTime, int daySpan)
        {
            List<DateTimeValue> result;
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                switch (issueType)
                {
                    case EIssueType.侧斜监测:
                        result = new TDepthNode().GetDateTimeValues(issueType,depth, nodeCode, fieldName, startTime, daySpan, connection);
                        break;
                    default:
                        throw new NotImplementedException("该方法暂不支持该类型");
                }
                connection.Close();
            }
            return result;
        }
        #endregion
    }
}
