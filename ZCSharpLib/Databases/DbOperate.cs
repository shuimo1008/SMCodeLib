using System;
using System.Collections.Generic;
using System.Reflection;
using ZCSharpLib.Comms;

namespace ZCSharpLib.Databases
{
    public enum DbOperateType
    {
        None,
        Insert,
        Delete,
        Search,
        SearchAll,
        Modify,
    }

    public abstract class BaseDbOperate
    {
        public bool IsExecuted { get; protected set; }

        public bool IsOperateComplated { get; protected set; }

        public Action<object> OnFinished { get; protected set; }

        public DbMgr Mgr { get; protected set; }

        public abstract DbOperateType OpType { get; }

        public DbStructure.DbStructContainer Container { get; protected set; }

        public virtual void Execute()
        {
            if (IsExecuted) return;
            IsExecuted = true;
            DatabaseReader reader = Mgr.Query(Container.SQLQuery);
            OnReader(reader);
            IsOperateComplated = true;
        }

        public virtual void OnReader(DatabaseReader reader)
        {
        }

        protected DbFieldBase SearchField(DbFieldBase[] oBaseFields, string oCondName)
        {
            for (int i = 0; i < oBaseFields.Length; i++)
            {
                DbFieldBase oBaseField = oBaseFields[i];
                if (oBaseField.FieldName.Equals(oCondName))
                {
                    return oBaseField;
                }
            }
            return null;
        }

        public abstract void OnExecCallback();
    }

    public abstract class DbOperate<T> : BaseDbOperate where T : class
    {
        public object Obj { get; protected set; }

        public virtual void Setup(DbMgr mgr, T obj, DbStructure.DbStructContainer container, Action<object> onFinished = null)
        {
            Mgr = mgr;
            Obj = obj as T;
            Container = container;
            OnFinished = onFinished;
        }

        public override void OnExecCallback()
        {
            OnFinished?.Invoke(Obj);
        }
    }

    public class InsertData<T> : DbOperate<T> where T : class, new()
    {
        public override DbOperateType OpType { get { return DbOperateType.Insert; } }
    }

    public class DeleteData<T> : DbOperate<T> where T : class, new()
    {
        public override DbOperateType OpType { get { return DbOperateType.Delete; } }
    }

    public class ModifyData<T> : DbOperate<T> where T : class, new()
    {
        public override DbOperateType OpType { get { return DbOperateType.Modify; } }
    }

    public class SearchData<T> : DbOperate<T> where T : class, new()
    {
        public override DbOperateType OpType { get { return DbOperateType.Search; } }

        public override void OnReader(DatabaseReader reader)
        {
            if (reader == null) Obj = default(T);
            if (reader.ElementCount <= 0) Obj = default(T);
            foreach (var item in reader)
            {
                DatabaseReader.DatabaseElement oElement = item as DatabaseReader.DatabaseElement;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Container.SQLFields[i].Value = oElement.GetValue(i);
                }
                break;  // Search 限定只查询一个数据，因此这里执行一次后直接返回
            }
            FieldInfo[] fieldInfos = Obj.GetType().GetFields();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                DbFieldBase oBaseField = SearchField(Container.SQLFields, fieldInfo.Name);
                if (oBaseField != null)
                {
                    try
                    {
                        fieldInfo.SetValue(Obj, Convert.ChangeType(oBaseField.Value, fieldInfo.FieldType));
                    }
                    catch (Exception e)
                    {
                        App.Error($"设置参数出现异常! 表{Container.SQLTable} 索引位置{i}, 列名{oBaseField.FieldName} 异常：{e}");
                    }
                }
            }
        }
    }

    public class SearchAllData<T> : DbOperate<T> where T : class, new()
    {
        public override DbOperateType OpType { get { return DbOperateType.SearchAll; } }

        public override void OnReader(DatabaseReader reader)
        {
            IList<T> list = new List<T>();

            if (reader == null) return;

            DbStructure.DbStructContainer[] containers = new DbStructure.DbStructContainer[reader.ElementCount];
            for (int i = 0; i < containers.Length; i++)
            {
                DbStructure.DbStructContainer tmpConatiner = new DbStructure.DbStructContainer();
                tmpConatiner.CopyFrom(Container);
                containers[i] = tmpConatiner;
            }

            int index = 0;
            foreach (var item in reader)
            {
                DbStructure.DbStructContainer tmpContainer = containers[index];
                DatabaseReader.DatabaseElement oElement = item as DatabaseReader.DatabaseElement;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    tmpContainer.SQLFields[i].Value = oElement.GetValue(i);
                }
                index = index + 1;
            }

            for (int tmpIndex = 0; tmpIndex < containers.Length; tmpIndex++)
            {
                DbStructure.DbStructContainer tmpContainer = containers[tmpIndex];
                T t = new T();
                FieldInfo[] fieldInfos = t.GetType().GetFields();
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    FieldInfo fieldInfo = fieldInfos[i];
                    DbFieldBase oBaseField = SearchField(tmpContainer.SQLFields, fieldInfo.Name);
                    if (oBaseField != null)
                    {
                        try
                        {
                            fieldInfo.SetValue(t, Convert.ChangeType(oBaseField.Value, fieldInfo.FieldType));
                        }
                        catch (Exception e)
                        {
                            App.Error($"设置参数出现异常! 表{Container.SQLTable} 索引位置{i}, 列名{oBaseField.FieldName} 异常：{e}");
                        }
                    }
                }
                list.Add(t);
                Obj = list;
            }
        }
    }
}
