using System;
using System.Collections.Generic;
using System.Linq;
using SMCore;
using SMCore.Cores;
using SMCore.Logger;

public class Tpl
{ 
   public readonly static localizationMgr localizationMgr = new localizationMgr();
   public static void Setup(byte[] bytes)
   { 
       ByteBuffer buffer = new ByteBuffer(bytes);
       localizationMgr.SetupData(buffer);
    } 
} 

public abstract class BaseTpl
{ 
   public string Tid { get; protected set; }
   public abstract void SetupData(ByteBuffer buffer);
} 

public class TemplateMgr<T> where T : BaseTpl, new()
{ 
   private readonly Dictionary<string, T> DataTable = new Dictionary<string, T>();

   public void SetupData(ByteBuffer buffer) 
   { 
       int dataCount = buffer.ReadInt32(); 
       for (int i = 0; i < dataCount; i++) 
       { 
           T t = new T();
           t.SetupData(buffer);
           if (!DataTable.ContainsKey(t.Tid)) DataTable.Add(t.Tid, t);
           else IoC.Resolve<ILoggerS>().Error($"模板{typeof(T).Name}Mgr已经包含Tid={t.Tid}的对象!");
       } 
   } 

   public T Find(string id)
   { 
       if (!DataTable.TryGetValue(id, out var tpl))
       {
           IoC.Resolve<ILoggerS>().Error($"模板{typeof(T).Name}Mgr没有包含Tid={id}的对象!");
       }
       return tpl;
   } 
   public T Find(Predicate<T> match)
   {
       return DataTable.Values.FirstOrDefault(t => match(t));
   }
   public IList<T> FindAll(Predicate<T> match)
   { 
       return DataTable.Values.Where(t => match(t)).ToList();
   } 

   private IList<T> datas;
   public IList<T> Datas => datas ?? (datas = DataTable.Values.ToList());

} 

/// <summary> 
/// localization 
/// </summary> 
public class localizationMgr
   : TemplateMgr<localizationTpl>
{ 
} 
public class localizationTpl : BaseTpl
{ 
   /// <summary> 
   /// 中文 
   /// </summary> 
   public string Chinese {get; private set; } 
   /// <summary> 
   /// 英文 
   /// </summary> 
   public string English {get; private set; } 

   public override void SetupData(ByteBuffer buffer)
   { 
       Tid = buffer.ReadUTF8();
       Chinese = buffer.ReadUTF8();
       English = buffer.ReadUTF8(); 
   } 
} 

