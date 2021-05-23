using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tools
{
    public class StructParser
    {
        private static Dictionary<string, StructContainer> SStructs
            = new Dictionary<string, StructContainer>();

        public static IList<StructContainer> Process(string[] files)
        {
            List<FileSymbol> fileSymbols = new List<FileSymbol>();
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                fileSymbols.Add(new FileSymbol(file));
            }

            foreach (var fileSymbol in fileSymbols)
            {
                Logger.Info($"解析:{Path.GetFileNameWithoutExtension(fileSymbol.File)}");
                fileSymbol.Process();
                foreach (var sstruct in fileSymbol.Structs)
                {
                    if (SStructs.TryGetValue(sstruct.Name, out var container))
                    {
                        throw new Exception($"{container.File}\n{fileSymbol.File}\n中包含相同的结构名:{sstruct.Name}!");
                    }
                    SStructs.Add(sstruct.Name, new StructContainer() {File = fileSymbol.File, Struct = sstruct});
                }
            }
            return new List<StructContainer>(SStructs.Values);
        }
    }

    public class StructContainer
    {
        public string File { get; set; }
        public TStruct Struct { get; set; }
    }

    public enum Define
    {
        Struct,
        InStruct,
    }

    public class FileSymbol
    {
        public string File { get; }
        public string FileName { get; }
        public string Content { get; private set; }
        public IList<TStruct> Structs { get; }

        public int Row { get; set; }
        /// <summary>
        /// 当前定义
        /// </summary>
        private Define define;
        /// <summary>
        /// 是否注释状态
        /// </summary>
        private bool isComment;
        /// <summary>
        /// 首行缩进
        /// </summary>
        private int indentation;

        private List<string> Symbols { get; set; }

        public FileSymbol(string filePath)
        {
            File = filePath;
            FileName = Path.GetFileNameWithoutExtension(File);
            Symbols = new List<string>();
            Structs = new List<TStruct>();
        }

        private int Index { get; set; }

        public void Process()
        {
            Content = System.IO.File.ReadAllText(File);
            Content = Content.Trim();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Content.Length; i++)
            {
                char c = Content[i];
                // 字符解析获取语法
                string sSymbol = string.Empty;
                if (c == '#') isComment = true;
                if (c == '\n') isComment = false;
                if (isComment)
                {
                    sb.Append(c);
                }
                else
                {
                    if (c == '\n') sSymbol = sb.ToString();
                    else if (c == ' ') sSymbol = sb.ToString();
                    else if (c == '\t') sSymbol = sb.ToString();
                    else sb.Append(c);
                }

                // 确保当完成所有字符读取后可以生成字符表
                if (i == Content.Length - 1)
                    sSymbol = sb.ToString();

                if (!string.IsNullOrEmpty(sSymbol))
                {
                    Symbols.Add(sSymbol);
                    sb.Clear();
                }

                if (c == '\n' || c == '\t')
                    Symbols.Add(c.ToString());
            }

            for (Index = 0; Index < Symbols.Count; Index++)
            {
                string symbol = Symbols[Index];

                if (symbol == "\n")
                    Row = Row + 1;

                if (symbol == "\n")
                    indentation = 0;

                // 计算首行缩进
                while (symbol == "\n")
                {
                    Index = Index + 1;
                    if (Index == Symbols.Count - 1)
                        break;
                    string indent = Symbols[Index];
                    if (indent == "\t") indentation++;
                    else break;
                }
                symbol = Symbols[Index];

                if (indentation == 0)
                {
                    if (symbol == "struct")
                    {
                        define = Define.Struct;
                        Structs.Add(new TStruct());
                        Structs.Last().Name = GetSName();
                        Structs.Last().Comment = GetComment();
                    }
                }

                if (indentation == 1)
                {
                    if (!string.IsNullOrEmpty(symbol))
                    {
                        // 首行缩进为1如果有不为空的字符则代表不处于内部结构中
                        if (define == Define.InStruct) define = Define.Struct;
                    }

                    // 跳过直接换行
                    if (symbol == "\r") continue;
                    if (symbol == "\n") continue;

                    if (symbol == "struct")
                    {
                        if (Structs.Last() != null)
                        {
                            define = Define.InStruct;
                            Structs.Last().CStructs.Add(new TStruct());
                            Structs.Last().CStructs.Last().Name = GetSName();
                            Structs.Last().CStructs.Last().Comment = GetComment();
                        }
                        else
                        {
                            throw new Exception(ErrorMessage("首行缩进为1的语法定义为内部嵌套结构, 子结构需要定义在父结构内!"));
                        }
                    }
                    else
                    {
                        int cIndex = Index;
                        TField tField = new TField()
                        {
                            Type = GetSType(), Name = GetSName(), Comment = GetComment()
                        };
                        Structs.Last().Fields.Add(tField);
                    }
                }

                if (indentation == 2)
                {
                    // 跳过直接换行
                    if (symbol == "\r") continue;
                    if (symbol == "\n") continue;

                    if (define == Define.InStruct)
                    {
                        if (symbol == "struct")
                        {
                            throw new Exception(ErrorMessage("内部结构不能在套内部结构!"));
                        }
                        TField tField = new TField()
                        {
                            Type = GetSType(), Name = GetSName(), Comment = GetComment()
                        };
                        Structs.Last().CStructs.Last().Fields.Add(tField);
                    }
                    else
                    {
                        throw new Exception(ErrorMessage("首行缩进为2的语法定义要求包含在内部结构中!"));
                    }
                }
            }

        }

        private string GetSType()
        {
            string sNext = Symbols[Index]; // 因为缩进结束后必然跟随类型,所以索引不加1
            if (sNext != "\n" && sNext != "#" && sNext != "\t" && sNext != "\r")
            {
                sNext = sNext.Replace("\r", "");
                return sNext;
            }
            throw new Exception(ErrorMessage("需要定义变量类型!"));
        }

        private string GetSName()
        {
            do
            {
                Index = Index + 1;
                string sNext = Symbols[Index];
                if (sNext != "\n" && sNext != "#" && sNext != "\t" && sNext != "\r")
                    return sNext = sNext.Trim();
                if (sNext == "\n") break; // 如果遇到换行则结束操作
            } while (Index < Symbols.Count - 1);

            throw new Exception(ErrorMessage("需要定义结构或变量名!"));
        }

        private string GetComment()
        {
            do
            {
                Index = Index + 1;
                if (Index >= Symbols.Count) // 最后一组没有注释
                    return string.Empty;

                string sNext = Symbols[Index];
                if (sNext != "\n")
                    sNext = sNext.Trim();   // "\r","\n","\t"在trim时都会被去掉,所以这里要判断是否是换行才能进行去空操作
                if (sNext.IndexOf("#", StringComparison.Ordinal) == 0)
                {
                    sNext = sNext.Replace("\r", "");
                    return sNext;
                }
                if (sNext == "\n") break; // 如果遇到换行则结束操作
            } while (Index < Symbols.Count - 1);

            if (Index < Symbols.Count - 1)
                Index--;    // 如果后面没有跟注释则索引回退

            return string.Empty;
        }

        private string ErrorMessage(string message)
        {
            return $"{File}\n第{Row + 1}行, {message}!";
        }
    }

    /// <summary>
    /// 主结构
    /// </summary>
    public class TStruct
    {
        /// <summary>
        /// 类型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 字段结构定义
        /// </summary>
        public IList<TField> Fields { get; set; }

        /// <summary>
        /// 子结构体
        /// </summary>
        public IList<TStruct> CStructs { get; set; }

        public TStruct()
        {
            Fields = new List<TField>();
            CStructs = new List<TStruct>();
        }
    }

    /// <summary>
    /// 字段结构
    /// </summary>
    public class TField
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; }
    }

}