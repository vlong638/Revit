﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace MyRevit.SubsidenceMonitor.Utilities
{
    public class SQLiteHelper
    {
        static string DbName = "Subsidence.sqlite";

        public static SQLiteConnection Connect()
        {
            return new SQLiteConnection($"DataSource={DbName};Version=3;");
        }

        #region 表初始化
        public static void CheckAndCreateTables()
        {
            var tableName = TableCreates.First().Key;
            if (!IsTableExist(tableName))
            {
                CreateTables();
            }
        }
        static Dictionary<string, string> TableCreates = new Dictionary<string, string>()
        {
            {"TList", $@"
create table TList 
(
   IssueType            numeric(16)                    not null,
   IssueDate            datetime                       not null,
   DataCount            numeric(16)                    not null,
   Constraint pk_List primary key(IssueType,IssueDate)
);"
            },
            { "TDetail",$@"
create table TDetail 
(
   IssueType            numeric(8)                     not null,
   IssueDateTime        datetime                       not null,
   IssueTimeRange       numeric(16)                    not null,
   ReportName           varchar(200)                   not null,
   Contractor           varchar(100)                   not null,
   Supervisor           varchar(100)                   not null,
   Monitor              varchar(100)                   not null,
   InstrumentName       varchar(100)                   not null,
   InstrumentCode       varchar(100)                   not null,
   CloseCTSettings      varchar(500)                   not null,
   OverCTSettings       varchar(500)                   not null,
   constraint PK_TDETAIL primary key (IssueType, IssueDateTime)
);
            "},
            {"TNode",  $@"
create table TNode 
(
   IssueType            numeric(8)                     not null,
   IssueDateTime        datetime                       not null,
   NodeCode             varchar(20)                    not null,
   Data                 varchar(1000)                  not null,
   ElementIds           varchar(2000)                  not null,
   ""Index""              numeric(16)                    not null,
   constraint PK_TNODE primary key (IssueType,IssueDateTime, NodeCode)
);
            "},
        };
        static bool IsTableExist(string table)
        {
            bool exits = false;
            using (var connection = Connect())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"select 1 from sqlite_master where name='{table}'";
                var result = command.ExecuteScalar();//注意返回的数值为long,不可强制转换为int
                exits = result != null && (long)result == 1;
                connection.Close();
            }
            return exits;
        }
        static void CreateTables()
        {
            foreach (var kv in TableCreates)
            {
                CreateTable(kv.Value);
            }
        }
        static void CreateTable(string createSQL)
        {
            using (var connection = Connect())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = createSQL;
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
        #endregion

        #region SQLite参数处理
        public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public static string ToSQLiteString(string input)
        {
            return $"'{input}'";
        }
        public static string ToSQLiteString(DateTime input)
        {
            return $"'{input.ToString(DateTimeFormat)}'";
        }
        public static string ToSQLiteString(int input)
        {
            return input.ToString();
        }
        public static string ToSQLiteString(bool input)
        {
            return Convert.ToInt32(input).ToString();
        }
        public static string ToSQLiteString<T>(Enum input)
        {
            return ((int)Enum.Parse(typeof(T), input.ToString())).ToString();
        }
        #endregion
    }
}
