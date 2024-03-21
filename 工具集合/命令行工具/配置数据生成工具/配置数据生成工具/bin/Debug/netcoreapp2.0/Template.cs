using System;
using System.Collections.Generic;
using System.Linq;
using SMCore;
using SMCore.Cores;
using SMCore.Logger;

public class Tpl
{ 
   public readonly static SkillMgr SkillMgr = new SkillMgr();
   public static void Setup(byte[] bytes)
   { 
       ByteBuffer buffer = new ByteBuffer(bytes);
       SkillMgr.SetupData(buffer);
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
           else IoC.Resolve<ILoggerS>().Error($"模板{typeof(T).Name}Mgr已经包含Tid={t.Tid}的对象!");
       } 
   } 

   public T Find(int id)
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
/// Sheet1 
/// </summary> 
public class SkillMgr
   : TemplateMgr<SkillTpl>
{ 
} 
public class SkillTpl : BaseTpl
{ 
   /// <summary> 
   /// 名称 
   /// </summary> 
   public string Name {get; private set; } 
   /// <summary> 
   /// 描述 
   /// </summary> 
   public string Desc {get; private set; } 
   /// <summary> 
   /// 释放模式
   /// ShuiMo:
   /// 释放方式
   /// 
   /// 1.范围类：(无位置，无方向，无目标)
   /// 2.射击类：(无位置，有方向，无目标)
   /// 3.投掷类：(有位置，无方向，无目标)
   /// 4.追踪类：(无位置，无方向，有目标) 
   /// </summary> 
   public int CastMode {get; private set; } 
   /// <summary> 
   /// 最小释放距离 
   /// </summary> 
   public float CastDistanceMin {get; private set; } 
   /// <summary> 
   /// 最大释放距离 
   /// </summary> 
   public float CastDistanceMax {get; private set; } 
   /// <summary> 
   /// 影响范围 
   /// </summary> 
   public float AffectRange {get; private set; } 
   /// <summary> 
   ///  条件状态 
   /// </summary> 
   public int[] PreStatuses {get; private set; } 
   /// <summary> 
   /// 影响状态 
   /// </summary> 
   public int[] AffectStatuses {get; private set; } 
   /// <summary> 
   /// 计算操作 
   /// </summary> 
   public int Compute {get; private set; } 
   /// <summary> 
   /// 倒计时 
   /// </summary> 
   public float CDTime {get; private set; } 

   public override void SetupData(ByteBuffer buffer)
   { 
       Tid = buffer.ReadInt32();
       Name = buffer.ReadUTF8();
       Desc = buffer.ReadUTF8();
       CastMode = buffer.ReadInt32();
        CastDistanceMin = buffer.ReadFloat();
        CastDistanceMax = buffer.ReadFloat();
        AffectRange = buffer.ReadFloat();
       int oPreStatusesCount = buffer.ReadInt32();
       PreStatuses = new int[oPreStatusesCount];
       for(int i = 0; i < oPreStatusesCount; i++)
       {
          PreStatuses[i] = buffer.ReadInt32();
       }
       int oAffectStatusesCount = buffer.ReadInt32();
       AffectStatuses = new int[oAffectStatusesCount];
       for(int i = 0; i < oAffectStatusesCount; i++)
       {
          AffectStatuses[i] = buffer.ReadInt32();
       }
       Compute = buffer.ReadInt32();
        CDTime = buffer.ReadFloat(); 
   } 
} 

