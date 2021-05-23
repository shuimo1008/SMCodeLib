using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZCSharpLib.Databases
{
    public class DbStructure
    {
        private DbFieldConvert mConvert;

        public DbStructure()
        {
            mConvert = new DbFieldConvert();
        }

        #region 数据解析相关
        public class DbStructContainer
        {
            public string SQLTable { get; set; }
            public string SQLQuery { get; set; }
            public DbFieldBase[] SQLFields { get; set; }
            /// <summary>
            /// 操作类型 1 删除，2 添加, 3 修改, 4 查找, 5 查找全部
            /// </summary>
            public int OperateType { get; set; }

            public void CopyFrom(DbStructContainer container)
            {
                SQLTable = container.SQLTable;
                SQLQuery = container.SQLQuery;
                SQLFields = new DbFieldBase[container.SQLFields.Length];
                for (int i = 0; i < SQLFields.Length; i++)
                {
                    DbFieldBase oBaseField = new DbFieldBase();
                    oBaseField.CopyFrom(container.SQLFields[i]);
                    SQLFields[i] = oBaseField;
                }
            }
        }

        #endregion

        #region 生成SQL语句

        public DbStructContainer Insert<T>(T obj) where T : class, new()
        {
            string query = string.Empty;
            DbStructContainer container = TryGetTableFields(obj);
            if (container == null) return container;
            container.OperateType = 2;
            DbFieldBase[] oFields = container.SQLFields;
            string[] fields = new string[oFields.Length];
            string[] values = new string[oFields.Length];
            for (int i = 0; i < oFields.Length; i++)
            {
                DbFieldBase oBaseField = oFields[i];
                fields[i] = oBaseField.FieldName;
                string sValue = oBaseField.Value == null ? string.Empty : oBaseField.Value.ToString();
                sValue = sValue.Replace("\"", "__SY__"); // 为了防止数据内容因为引号导致无法输入, 故此进行引号转换
                values[i] = "\"" + sValue + "\"";
            }
            string sFields = string.Join(",", fields);
            string sValues = string.Join(",", values);
            container.SQLQuery = string.Format("insert into {0}({1}) values ({2})", container.SQLTable, sFields, sValues);
            return container;
        }

        public DbStructContainer Delete<T>(T obj) where T : class, new()
        {
            string query = string.Empty;
            DbStructContainer container = TryGetTableFields(obj);
            if (container == null) return container;
            container.OperateType = 1;
            container.SQLQuery = string.Format("delete from {0} where Guid=\"{1}\"", container.SQLTable, container.SQLFields[container.SQLFields.Length - 1].Value); // 基类的Guid在最后一位，故而这里使用.Length - 1
            return container;
        }

        public DbStructContainer Modify<T>(T obj) where T : class, new()
        {
            string query = string.Empty;
            DbStructContainer container = TryGetTableFields(obj);
            if (container == null) return container;
            container.OperateType = 3;
            DbFieldBase[] oFields = container.SQLFields;

            string[] keyvalues = new string[oFields.Length];
            for (int i = 0; i < oFields.Length; i++)
            {
                DbFieldBase oBaseField = oFields[i];
                string sValue = oBaseField.Value == null ? string.Empty : oBaseField.Value.ToString();
                sValue = sValue.Replace("\"", "__SY__"); // 为了防止数据内容因为引号导致无法输入, 故此进行引号转换
                keyvalues[i] = string.Format("{0}=\"{1}\"", oBaseField.FieldName, sValue);
            }
            string sKeyValues = string.Join(",", keyvalues);
            container.SQLQuery = string.Format("update {0} set {1} where Guid=\"{2}\"", container.SQLTable, sKeyValues, container.SQLFields[container.SQLFields.Length - 1].Value);  // 基类的Guid在最后一位，故而这里使用.Length - 1
            return container;
        }

        public DbStructContainer Search<T>(T obj) where T : class, new()
        {
            string query = string.Empty;
            DbStructContainer container = TryGetTableFields(obj);
            if (container == null) return container;
            container.OperateType = 4;
            DbFieldBase[] oFields = container.SQLFields;

            string[] fields = new string[oFields.Length];
            for (int i = 0; i < oFields.Length; i++)
            {
                fields[i] = oFields[i].FieldName;
            }
            string sField = string.Join(",", fields);
            container.SQLQuery = string.Format("select {0} from {1} where Guid=\"{2}\"", sField, container.SQLTable, container.SQLFields[container.SQLFields.Length - 1].Value); // 基类的Guid在最后一位，故而这里使用.Length - 1
            return container;
        }

        public DbStructContainer SearchAll<T>() where T : class, new()
        {
            string query = string.Empty;
            DbStructContainer container = TryGetTableFields(new T());
            if (container == null) return container;
            container.OperateType = 5;
            DbFieldBase[] oFields = container.SQLFields;

            string[] fields = new string[oFields.Length];
            for (int i = 0; i < oFields.Length; i++)
            {
                fields[i] = oFields[i].FieldName;
            }
            string sField = string.Join(",", fields);
            container.SQLQuery = string.Format("select {0} from {1}", sField, container.SQLTable);
            return container;
        }

        public DbStructContainer TryGetTableFields(object obj)
        {
            string sSQLTable = string.Empty;
            DbStructContainer container = null;
            List<DbFieldBase> oFieldList = new List<DbFieldBase>();

            Type type = obj.GetType();
            object[] oSaveDBAttributes = type.GetCustomAttributes(typeof(DbAttribute), false);
            if (oSaveDBAttributes.Length > 0)
            {
                sSQLTable = (oSaveDBAttributes[0] as DbAttribute).TableName;
                FieldInfo[] fieldInfos = type.GetFields();
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    FieldInfo fieldInfo = fieldInfos[i];
                    object[] oFieldUnSaveDBAttributes = fieldInfo.GetCustomAttributes(typeof(DbUnsaveAttribute), false);
                    if (oFieldUnSaveDBAttributes.Length == 0)
                    {
                        DbFieldBase oField = mConvert.Type2Object(fieldInfo.FieldType);
                        oField.FieldName = fieldInfo.Name;
                        oField.Value = fieldInfo.GetValue(obj);
                        oFieldList.Add(oField);
                    }
                }
                container = new DbStructContainer();
                container.SQLTable = sSQLTable;
                container.SQLFields = oFieldList.ToArray();
            }
            return container;
        }
        #endregion
    }

    public class DbFieldConvert
    {
        public DbFieldBase Type2Object(string strType)
        {
            DbFieldBase oField = null;
            if (strType.Equals(typeof(Boolean).Name)) oField = new DbBooleanField();
            else if (strType.Equals(typeof(Int16).Name)) oField = new DbInt16Field();
            else if (strType.Equals(typeof(Int32).Name)) oField = new DbInt32Field();
            else if (strType.Equals(typeof(Int64).Name)) oField = new DbInt64Field();
            else if (strType.Equals(typeof(UInt16).Name)) oField = new DbUInt16Field();
            else if (strType.Equals(typeof(UInt32).Name)) oField = new DbUInt32Field();
            else if (strType.Equals(typeof(UInt64).Name)) oField = new DbUInt64Field();
            else if (strType.Equals(typeof(Single).Name)) oField = new DbSingleField();
            else if (strType.Equals(typeof(Double).Name)) oField = new DbDoubleField();
            else if (strType.Equals(typeof(String).Name)) oField = new DbStringField();
            return oField;
        }

        public DbFieldBase Type2Object(Type fieldType)
        {
            DbFieldBase oField = null;
            if (fieldType == typeof(Boolean)) oField = new DbBooleanField();
            else if (fieldType == typeof(Int16)) oField = new DbInt16Field();
            else if (fieldType == typeof(Int32)) oField = new DbInt32Field();
            else if (fieldType == typeof(Int64)) oField = new DbInt64Field();
            else if (fieldType == typeof(UInt16)) oField = new DbUInt16Field();
            else if (fieldType == typeof(UInt32)) oField = new DbUInt32Field();
            else if (fieldType == typeof(UInt64)) oField = new DbUInt64Field();
            else if (fieldType == typeof(Single)) oField = new DbSingleField();
            else if (fieldType == typeof(Double)) oField = new DbDoubleField();
            else if (fieldType == typeof(String)) oField = new DbStringField();
            return oField;
        }
    }

    public class DbFieldBase
    {
        public string FieldName { get; set; }

        public string TypeName { get; set; }

        public object Value { get; set; }

        public void CopyFrom(DbFieldBase oBaseField)
        {
            FieldName = oBaseField.FieldName;
            TypeName = oBaseField.TypeName;
            Value = oBaseField.Value;
        }
    }

    public class DbBooleanField : DbFieldBase
    {
        public DbBooleanField() : base() { TypeName = "Boolean"; }
    }

    public class DbInt16Field : DbFieldBase
    {
        public DbInt16Field() : base() { TypeName = "Int16"; }
    }

    public class DbInt32Field : DbFieldBase
    {
        public DbInt32Field() : base() { TypeName = "Int32"; }
    }

    public class DbInt64Field : DbFieldBase
    {
        public DbInt64Field() : base() { TypeName = "Int64"; }
    }

    public class DbUInt16Field : DbFieldBase
    {
        public DbUInt16Field() : base() { TypeName = "UInt16"; }
    }

    public class DbUInt32Field : DbFieldBase
    {
        public DbUInt32Field() : base() { TypeName = "UInt32"; }
    }

    public class DbUInt64Field : DbFieldBase
    {
        public DbUInt64Field() : base() { TypeName = "UInt64"; }
    }

    public class DbSingleField : DbFieldBase
    {
        public DbSingleField() : base() { TypeName = "Single"; }
    }

    public class DbDoubleField : DbFieldBase
    {
        public DbDoubleField() : base() { TypeName = "Double"; }
    }

    public class DbStringField : DbFieldBase
    {
        public DbStringField() : base() { TypeName = "String"; }
    }
}
