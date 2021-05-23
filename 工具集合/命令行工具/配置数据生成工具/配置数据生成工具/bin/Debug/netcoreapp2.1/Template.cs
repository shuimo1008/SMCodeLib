using System;
using System.Collections.Generic;
using System.Linq;
using ZCSharpLib;
using ZCSharpLib.Nets;

public class Template
{ 
   public readonly static MissionEntryMgr MissionEntryMgr = new MissionEntryMgr();
   public static void Load(byte[] bytes)
   { 
       ByteBuffer buffer = new ByteBuffer(bytes);
       MissionEntryMgr.SetupData(buffer);
    } 
} 

public abstract class BaseTpl
{ 
   public int Tid { get; protected set; }
   public abstract void SetupData(ByteBuffer buffer);
} 

public class TemplateMgr<T> where T : BaseTpl, new()
{ 
   public readonly Dictionary<int, T> Datas = new Dictionary<int, T>();

   public void SetupData(ByteBuffer buffer) 
   { 
       int dataCount = buffer.ReadInt32(); 
       for (int i = 0; i < dataCount; i++) 
       { 
           T t = new T();
           t.SetupData(buffer);
           if (!Datas.ContainsKey(t.Tid)) Datas.Add(t.Tid, t);
           else App.Error($"模板{typeof(T).Name}Mgr已经包含Tid={t.Tid}的对象!");
       } 
   } 

   public T Find(int id)
   { 
       if (!Datas.TryGetValue(id, out var tpl))
       {
           App.Error($"模板{typeof(T).Name}Mgr没有包含Tid={id}的对象!");
       }
       return tpl;
   } 
   public T Find(Predicate<T> match)
   {
       return Datas.Values.FirstOrDefault(t => match(t));
   }
   public IList<T> FindAll(Predicate<T> match)
   { 
       return Datas.Values.Where(t => match(t)).ToList();
   } 

   private static IList<T> dataList;
   public IList<T> DataList => dataList ?? (dataList = Datas.Values.ToList());

} 

/// <summary> 
/// 任务条目 
/// </summary> 
public class MissionEntryMgr
   : TemplateMgr<MissionEntry>
{ 
} 
public class MissionEntry : BaseTpl
{ 
   /// <summary> 
   /// 条目描述 
   /// </summary> 
   public string Describe {get; private set; } 
   /// <summary> 
   /// 完成条目所需项目ID 
   /// </summary> 
   public int ItemID {get; private set; } 
   /// <summary> 
   /// 完成条目所需物件个数 
   /// </summary> 
   public int NeedItemNum {get; private set; } 
   /// <summary> 
   /// 测试倍数 
   /// </summary> 
   public int Test {get; private set; } 
   /// <summary> 
   /// 总数 
   /// </summary> 
   public int Total {get; private set; } 

   public override void SetupData(ByteBuffer buffer)
   { 
       Tid = buffer.ReadInt32();
       Describe = buffer.ReadUTF8();
       ItemID = buffer.ReadInt32();
       NeedItemNum = buffer.ReadInt32();
       Test = buffer.ReadInt32();
       Total = buffer.ReadInt32(); 
   } 
} 

