using System;
using System.Collections.Generic;
using System.Linq;
using ZCSharpLib;
using ZCSharpLib.Nets;

public class Tpl
{ 
   public readonly static DefaultAssetMgr DefaultAssetMgr = new DefaultAssetMgr();
   public static void Load(byte[] bytes)
   { 
       ByteBuffer buffer = new ByteBuffer(bytes);
       DefaultAssetMgr.SetupData(buffer);
    } 
} 

public abstract class BaseTpl
{ 
   public int Tid { get; protected set; }
   public abstract void SetupData(ByteBuffer buffer);
} 

public class TemplateMgr<T> where T : BaseTpl, new()
{ 
   private readonly Dictionary<int, T> DataTable = new Dictionary<int, T>();

   public void SetupData(ByteBuffer buffer) 
   { 
       int dataCount = buffer.ReadInt32(); 
       for (int i = 0; i < dataCount; i++) 
       { 
           T t = new T();
           t.SetupData(buffer);
           if (!DataTable.ContainsKey(t.Tid)) DataTable.Add(t.Tid, t);
           else App.Error($"模板{typeof(T).Name}Mgr已经包含Tid={t.Tid}的对象!");
       } 
   } 

   public T Find(int id)
   { 
       if (!DataTable.TryGetValue(id, out var tpl))
       {
           App.Error($"模板{typeof(T).Name}Mgr没有包含Tid={id}的对象!");
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
/// Sheet1 
/// </summary> 
public class DefaultAssetMgr
   : TemplateMgr<DefaultAssetTpl>
{ 
} 
public class DefaultAssetTpl : BaseTpl
{ 
   /// <summary> 
   /// 名称 
   /// </summary> 
   public string Name {get; private set; } 

   public override void SetupData(ByteBuffer buffer)
   { 
       Tid = buffer.ReadInt32();
       Name = buffer.ReadUTF8(); 
   } 
} 

