using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using NHibernate.SqlCommand;

namespace Dqe.Infrastructure.Driver
{
    public class SqlReviewHelper
    {
        public enum FormatForEnum
        {
            None,
            Oracle,
            Db2,
            SqlServer
        }

        public static SqlReviewHelper Current { get; set; }

        private IList<string> _sqlStrings = new List<string>();
        private IList<string> _logEntries = new List<string>();

        internal void ExpandQueryParameters(IDbCommand cmd, SqlString sqlString, FormatForEnum format)
        {
            //LogNonParameterizedCommand(cmd, sqlString, format);
            LogParameterizedCommand(cmd, sqlString);
        }

        public void Dump()
        {
            if (_logEntries.Count > 0)
            {
                File.WriteAllLines(@"c:\temp\dqe_sql.txt", _logEntries.ToArray());
            }
            _sqlStrings = new List<string>();
            _logEntries = new List<string>();
        }

        internal void LogParameterizedCommand(IDbCommand cmd, SqlString sqlString)
        {
            if (_sqlStrings.Contains(cmd.CommandText.ToUpper()))
            {
                return;
            }
            _sqlStrings.Add(cmd.CommandText.ToUpper());
            _logEntries.Add("-- ** SQL **");
            foreach (var p in cmd.Parameters)
            {
                var parm = (SqlParameter)p;
                _logEntries.Add(string.Format("DECLARE {0} {1} {2}", parm.ParameterName.ToUpper(), parm.SqlDbType, parm.Size == 0 ? string.Empty : string.Format("({0})", parm.Size)));
            }
            foreach (var p in cmd.Parameters)
            {
                bool checkBool;
                var parm = (SqlParameter)p;
                if (bool.TryParse(parm.Value.ToString(), out checkBool))
                {
                    _logEntries.Add(string.Format("SET {0} = {1}", parm.ParameterName.ToUpper(), checkBool ? 1 : 0));
                }
                else
                {
                    DateTime checkDate;
                    _logEntries.Add(string.Format("SET {0} = {1}", parm.ParameterName.ToUpper(), parm.Value as string == null && !DateTime.TryParse(parm.Value.ToString(), out checkDate) ? parm.Value : string.Format("'{0}'", parm.Value)));
                }
            }
            _logEntries.Add(cmd.CommandText.ToUpper());
            _logEntries.Add("GO");
            _logEntries.Add(" ");
        }

        //internal void LogNonParameterizedCommand(IDbCommand cmd, SqlString sqlString, FormatForEnum formatFor)
        //{
        //    var s = sqlString.ToString();
        //    if (_sqlStrings.Any(i => i == s)) return;
        //    _sqlStrings.Add(s);
        //    var sql = string.Empty;
        //    var position = 0;
        //    for (var i = 0; i < sqlString.Count; i++)
        //    {
        //        var p = sqlString.ElementAt(i);
        //        if (ReferenceEquals(p as Parameter, null))
        //        {
        //            sql += p.ToString();
        //            continue;
        //        }
        //        var parameter = (Parameter)p;
        //        var parmPosition = (parameter.ParameterPosition.HasValue)
        //                                ? parameter.ParameterPosition.Value
        //                                : position;
        //        var parm = (DbParameter)cmd.Parameters[parmPosition];
        //        switch (parm.DbType)
        //        {
        //            case DbType.Date:
        //                switch (formatFor)
        //                {
        //                    case FormatForEnum.Db2:
        //                        sql += FormatDateForDb2((DateTime)parm.Value);
        //                        break;
        //                    case FormatForEnum.Oracle:
        //                        sql += FormatDateForOracle((DateTime)parm.Value);
        //                        break;
        //                    case FormatForEnum.SqlServer:
        //                        sql += string.Format(" '{0}' ", parm.Value);
        //                        break;
        //                    default:
        //                        sql += string.Format(" '{0}' ", parm.Value);
        //                        break;
        //                }
        //                break;
        //            case DbType.DateTime:
        //            case DbType.DateTime2:
        //            case DbType.Time:
        //                switch (formatFor)
        //                {
        //                    case FormatForEnum.Db2:
        //                        sql += FormatDateForDb2((DateTime)parm.Value);
        //                        break;
        //                    case FormatForEnum.Oracle:
        //                        sql += FormatDateForOracle((DateTime)parm.Value);
        //                        break;
        //                    case FormatForEnum.SqlServer:
        //                        sql += string.Format(" '{0}' ", parm.Value);
        //                        break;
        //                    default:
        //                        sql += string.Format(" '{0}' ", parm.Value);
        //                        break;
        //                }
        //                break;
        //            case DbType.Boolean:
        //            case DbType.Byte:
        //            case DbType.Currency:
        //            case DbType.Decimal:
        //            case DbType.Double:
        //            case DbType.Int16:
        //            case DbType.Int32:
        //            case DbType.Int64:
        //            case DbType.SByte:
        //            case DbType.Single:
        //            case DbType.UInt16:
        //            case DbType.UInt32:
        //            case DbType.UInt64:
        //            case DbType.VarNumeric:
        //                if (parm.Value == null)
        //                {
        //                    sql += " null ";
        //                }
        //                else
        //                {
        //                    sql += string.Format(" {0} ", parm.Value);
        //                }

        //                break;
        //            case DbType.AnsiString:
        //            case DbType.AnsiStringFixedLength:
        //            case DbType.String:
        //            case DbType.StringFixedLength:

        //                sql += string.Format(" '{0}' ", parm.Value.ToString().Replace("'", string.Empty));
        //                break;
        //            default:
        //                throw new InvalidOperationException("DB parameter type has no valid conversion");
        //        }
        //        position += 1;
        //    }
        //    _logEntries.Add("-- ** SQL **");
        //    _logEntries.Add(string.Format("{0};", sql));
        //}

        //private static string FormatDateForOracle(DateTime d)
        //{
        //    return string.Format(" TO_DATE('{0}', 'MM/DD/YYYY') ", (d).ToString("MM/dd/yyyy"));
        //}

        //private static string FormatDateForDb2(DateTime d)
        //{
        //    return string.Format("'{0}'", (d).ToString("yyyy-MM-dd"));
        //}
    }
}