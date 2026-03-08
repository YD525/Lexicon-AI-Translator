using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PhoenixEngine.ADO;

namespace LexTranslator.SQLManager
{
    public class ConvertT
    {
        public static T ConverToObject<T>(object Asobject)
        {
            var Instance = Activator.CreateInstance<T>();

            if (Asobject != null)
            {
                Type type = Asobject.GetType();

                foreach (var Info in typeof(T).GetProperties())
                {
                    object Obj = null;

                    var Value = type.GetProperty(Info.Name)?.GetValue(Asobject);

                    if (Value != null)
                    {
                        if (!Info.PropertyType.IsGenericType)

                            Obj = Convert.ChangeType(Value, Info.PropertyType);

                        else
                        {
                            Type GenericTypeDefinition = Info.PropertyType.GetGenericTypeDefinition();

                            if (GenericTypeDefinition == typeof(Nullable<>))
                            {
                                Obj = Convert.ChangeType(Value, Nullable.GetUnderlyingType(Info.PropertyType));
                            }
                        }

                        Info.SetValue(Instance, Obj, null);
                    }
                }
            }
            return Instance;
        }
    }

    public class SqlCore<T> where T : new()
    {
        public object LockerDB = new object();
        public T SQLHelper;
        public string ConnectStr = "";
        public SqlType ThisType = SqlType.Null;
        public SqlCore(string ConnectStr)
        {
            this.SQLHelper = new T();

            if (SQLHelper is P_SQLite)
            {
                (this.SQLHelper as P_SQLite).OpenSQL(ConnectStr);
                this.ThisType = SqlType.SQLite;
            }

            this.ConnectStr = ConnectStr;
        }
        public int ExecuteNonQuery(string SqlOrder)
        {
            lock (LockerDB)
            {
                int State = -9;
                switch (this.ThisType)
                {
                    case SqlType.SQLite:
                        {
                            State = (SQLHelper as P_SQLite).ExecuteNonQuery(SqlOrder);
                        }
                        break;
                    case SqlType.SqlServer:
                        {
                            
                        }
                        break;
                    case SqlType.MySql:
                        {
                        }
                        break;
                }
                return State;
            }
        }
        public List<Dictionary<string, object>> ExecuteQuery(string SqlOrder)
        {
            lock (LockerDB)
            {
                List<Dictionary<string, object>> Table = null;

                switch (this.ThisType)
                {
                    case SqlType.SQLite:
                        {
                            Table = (SQLHelper as P_SQLite).ExecuteQuery(SqlOrder);
                        }
                        break;
                    case SqlType.SqlServer:
                        {
                           
                        }
                        break;
                    case SqlType.MySql:
                        {
                        }
                        break;
                }

                return Table;
            }
        }
        //public T1 ExecuteScalar<T1>(string SqlOrder)
        //{
        //    object Result = null;

        //    switch (this.ThisType)
        //    {
        //        case SqlType.SQLite:
        //            {
        //                Result = (SQLHelper as SQLiteHelper).ExecuteScalar(SqlOrder);
        //            }
        //            break;
        //        case SqlType.SqlServer:
        //            {
        //                Result = (SQLHelper as SqlServerHelper).ExecuteScalar(SqlOrder);
        //            }
        //            break;
        //        case SqlType.MySql:
        //            {
        //            }
        //            break;
        //    }

        //    try
        //    {
        //        return ConvertT.ConverToObject<T1>(Result);
        //    }
        //    catch { return default(T1); }
        //}

        public static object GetPropertyValue(object info, string field)
        {
            if (info == null) return null;
            Type t = info.GetType();
            IEnumerable<System.Reflection.PropertyInfo> property = from pi in t.GetProperties() where pi.Name.ToLower() == field.ToLower() select pi;
            return property.First().GetValue(info, null);
        }
        public object ExecuteScalar(string SqlOrder)
        {
            lock (LockerDB)
            {
                object Result = null;

                switch (this.ThisType)
                {
                    case SqlType.SQLite:
                        {
                            Result = (SQLHelper as P_SQLite).ExecuteScalar(SqlOrder);
                        }
                        break;
                    case SqlType.SqlServer:
                        {
                           
                        }
                        break;
                    case SqlType.MySql:
                        {
                        }
                        break;
                }

                return Result;
            }
        }
    }
    public enum SqlType
    {
        Null = 0, SQLite = 1, SqlServer = 2, MySql = 3, Access = 4, Oracle = 5,
    }
}
