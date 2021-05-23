using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tools
{
    public class GenerateCode
    {
        public string REFERENCE =
            "using System;\r\n" +
            "using System.Collections.Generic;\r\n" +
            "using ZCSharpLib;\r\n" +
            "using ZCSharpLib.Nets;\r\n";

        public string CLASSCONTENT1 =
            "#region <PID> {0} <COMMENT>" + "\r\n" +
            "[Packet(<PID>)]\r\n" +
            "public class <CLASSNAME>Creator : IPacketCreator" + "\r\n" +
            "__LK__" + "\r\n" +
            "   public IPacket CreatePacket()" + "\r\n" +
            "   __LK__" + "\r\n" +
            "       return new <CLASSNAME>();" + "\r\n" +
            "   __RK__" + "\r\n" +
            "__RK__" + "\r\n" +
            "/// <summary> \r\n" +
            "/// <COMMENT> \r\n" +
            "/// </summary> \r\n" +
            "[Packet(<PID>)]" + "\r\n" +
            "public class <CLASSNAME> : Packet" + "\r\n" +
            "__LK__" + "\r\n" + 
                "{1}" + "\r\n" +
            "   public override void Serialization(ByteBuffer buffer, bool isSerialize)" + "\r\n" +
            "   __LK__" + "\r\n" +
            "       PacketID = <PID>;" + "\r\n" +
            "       if (isSerialize)" + "\r\n" +
            "       __LK__" + "\r\n" +
            "            buffer.WriteInt32(PacketID);" + "\r\n" +
                        "{2}" +
            "       __RK__" + "\r\n" +
            "       else" + "\r\n" +
            "       __LK__" + "\r\n" +
            "            PacketID = buffer.ReadInt32();" + "\r\n" +
                        "{3}" +
            "       __RK__" + "\r\n" +
            "   __RK__" + "\r\n" +
            //"   public int PacketID __LK__ get; set; __RK__" + "\r\n" +
            //"   public object Owner __LK__ get; set; __RK__" + "\r\n" +
            //"   public object Session __LK__ get; set; __RK__" + "\r\n" +
                "{4}" +
            "__RK__" + "\r\n" +
            "#endregion" + "\r\n";

        public string CLASSCONTENT2 =
            "   /// <summary> \r\n" +
            "   /// <COMMENT> \r\n" +
            "   /// </summary> \r\n" +
            "   public class {0}" + "\r\n" +
            "   __LK__" + "\r\n" +
                    "{1}" + "\r\n" +
            "       public void Serialization(ByteBuffer buffer, bool isSerialize)" + "\r\n" +
            "       __LK__" + "\r\n" +
            "           if (isSerialize)" + "\r\n" +
            "           __LK__" + "\r\n" +
                            "{2}" +
            "           __RK__" + "\r\n" +
            "           else" + "\r\n" +
            "           __LK__" + "\r\n" +
                            "{3}" +
            "           __RK__" + "\r\n" +
            "       __RK__" + "\r\n" +
            "   __RK__" + "\r\n";

        public string COMMENT1 =
            "   /// <summary> \r\n" +
            "   /// {0} \r\n" +
            "   /// </summary> \r\n";

        public string FIELDTYPE1 =
                "{0}" +
            "   public {1} {2} __LK__get; set; __RK__ \r\n";

        public string COMMENT2 =
            "       /// <summary> \r\n" +
            "       /// {0} \r\n" +
            "       /// </summary> \r\n";

        public string FIELDTYPE2 =
                    "{0}" +
            "       public {1} {2} __LK__get; set; __RK__ \r\n";

        public string Gen(IList<StructContainer> containers)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(REFERENCE);
            sb.Append("\r\n");

            int serial = 10000; // 协议序号
            foreach (var container in containers)
            {
                serial = serial + 1;
                string fileName = Path.GetFileNameWithoutExtension(container.File);

                TStruct tStruct = container.Struct;

                StringBuilder filedSB1 = new StringBuilder();
                StringBuilder methodSB1_R = new StringBuilder();
                StringBuilder methodSB1_W = new StringBuilder();

                foreach (var tField1 in tStruct.Fields)
                {
                    string sComment = string.Format(COMMENT1, tField1.Comment);
                    filedSB1.Append(string.Format(FIELDTYPE1, sComment, tField1.Type, tField1.Name));

                    methodSB1_R.Append($"{GetMethod(tField1.Type, tField1.Name, true, 3)}");
                    methodSB1_W.Append($"{GetMethod(tField1.Type, tField1.Name, false, 3)}");
                }

                StringBuilder structSB = new StringBuilder();
                foreach (var cStruct in tStruct.CStructs)
                {
                    StringBuilder filedSB2 = new StringBuilder();
                    StringBuilder methodSB2_R = new StringBuilder();
                    StringBuilder methodSB2_W = new StringBuilder();

                    foreach (var cField in cStruct.Fields)
                    {
                        string sComment = string.Format(COMMENT2, cField.Comment);
                        filedSB2.Append(string.Format(FIELDTYPE2, sComment, cField.Type, cField.Name));

                        methodSB2_R.Append($"{GetMethod(cField.Type, cField.Name, true, 4)}");
                        methodSB2_W.Append($"{GetMethod(cField.Type, cField.Name, false, 4)}");
                    }
                    string content2 = string.Format(CLASSCONTENT2, cStruct.Name, filedSB2, methodSB2_R, methodSB2_W);
                    content2 = content2.Replace("<COMMENT>", cStruct.Comment);
                    structSB.Append("\r\n");
                    structSB.Append(content2);
                }

                string content = string.Format(CLASSCONTENT1, fileName, filedSB1, methodSB1_R, methodSB1_W, structSB);
                content = content.Replace("<PID>", $"{serial}");
                content = content.Replace("<CLASSNAME>", $"{tStruct.Name}");
                content = content.Replace("<COMMENT>", $"{tStruct.Comment}");
                sb.Append(content);
                sb.Append("\r\n");
            }
            string final = sb.ToString();
            final = final.Replace("__LK__", "{");
            final = final.Replace("__RK__", "}");
            return final;
        }

        private string GetMethod(string sType, string fieldName, bool isSerialize, int indentation)
        {
            string sMethod = string.Empty;

            // 制表符操作
            string tableSymbol = string.Empty;
            for (int i = 0; i < indentation; i++)
            {
                tableSymbol = tableSymbol + "\t";
            }

            if (sType.Equals("byte"))
            {
                if (isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteByte({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadByte();\r\n";
            }
            else if (sType.Equals("bool"))
            {
                if (isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteBool({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadBool();\r\n";
            }
            else if (sType.Equals("short"))
            {
                if (isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteInt16({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadInt16();\r\n";
            }
            else if (sType.Equals("ushort"))
            {
                if (isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteUInt16({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadUInt16();\r\n";
            }
            else if (sType.Equals("int"))
            {
                if (isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteInt32({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadInt32();\r\n";
            }
            else if (sType.Equals("uint"))
            {
                if (isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteUInt32({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadUInt32();\r\n";
            }
            else if (sType.Equals("long"))
            {
                if (isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteInt64({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadInt64();\r\n";
            }
            else if (sType.Equals("ulong"))
            {
                if (isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteUInt64({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadUInt64();\r\n";
            }
            else if (sType.Equals("float"))
            {
                if(isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteFloat({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadFloat();\r\n";
            }
            else if (sType.Equals("string"))
            {
                if(isSerialize)
                    sMethod = tableSymbol + $"buffer.WriteUTF8({fieldName});\r\n";
                else
                    sMethod = tableSymbol + $"{fieldName} = buffer.ReadUTF8();\r\n";
            }
            else if (sType.Contains("[]"))
            {
                if (isSerialize)
                {
                    sMethod = sMethod +
                          tableSymbol + $"buffer.WriteInt32({fieldName}.Length);" + "\r\n" +
                          tableSymbol + $"for(int i = 0; i < {fieldName}.Length; i++)" + "\r\n" +
                          tableSymbol + $"__LK__" + "\r\n" +
                          tableSymbol + $"    <METHOD>({fieldName}[i]);" + "\r\n" +
                          tableSymbol + $"__RK__" + "\r\n";
                    if (sType.Equals("byte[]"))
                    {
                        sMethod =
                            tableSymbol + $"buffer.WriteInt32({fieldName}.Length);" + "\r\n" +
                            tableSymbol + $"buffer.WriteBytes({fieldName});" + "\r\n";
                    }
                    else if (sType.Equals("bool[]"))
                    {
                        sMethod = sMethod.Replace("<METHOD>", "buffer.WriteBool");
                    }
                    else if (sType.Equals("short[]"))
                    {
                        sMethod = sMethod.Replace("<METHOD>", "buffer.WriteInt16");
                    }
                    else if (sType.Equals("ushort[]"))
                    {
                        sMethod = sMethod.Replace("<METHOD>", "buffer.WriteUInt16");
                    }
                    else if (sType.Equals("int[]"))
                    {
                        sMethod = sMethod.Replace("<METHOD>", "buffer.WriteInt32");
                    }
                    else if (sType.Equals("uint[]"))
                    {
                        sMethod = sMethod.Replace("<METHOD>", "buffer.WriteUInt32");
                    }
                    else if (sType.Equals("long[]"))
                    {
                        sMethod = sMethod.Replace("<METHOD>", "buffer.WriteInt64");
                    }
                    else if (sType.Equals("ulong[]"))
                    {
                        sMethod = sMethod.Replace("<METHOD>", "buffer.WriteUInt64");
                    }
                    else if (sType.Equals("float[]"))
                    {
                        sMethod = sMethod.Replace("<METHOD>", "buffer.WriteFloat");
                    }
                    else if (sType.Equals("string[]"))
                    {
                        sMethod = sMethod.Replace("<METHOD>", "buffer.WriteUTF8");
                    }
                    else
                    {
                        // 自定义类型
                        sMethod =
                            tableSymbol + $"buffer.WriteInt32({fieldName}.Length);" + "\r\n" +
                            tableSymbol + $"for(int i = 0; i < {fieldName}.Length; i++)" + "\r\n" +
                            tableSymbol + $"__LK__" + "\r\n" +
                            tableSymbol + $"    {fieldName}[i].Serialization(buffer, isSerialize);" + "\r\n" +
                            tableSymbol + $"__RK__"+"\r\n";
                    }
                }
                else
                {
                    sMethod = sMethod +
                              tableSymbol + $"int o{fieldName}Count = buffer.ReadInt32();" + "\r\n" +
                              tableSymbol + $"{fieldName} = new <TYPE>[o{fieldName}Count];" + "\r\n" +
                              tableSymbol + $"for(int i = 0; i < o{fieldName}Count; i++)" + "\r\n" +
                              tableSymbol + $"__LK__" + "\r\n" +
                              tableSymbol + $"    {fieldName}[i] = <METHOD>;" + "\r\n" +
                              tableSymbol + $"__RK__" + "\r\n";
                    if (sType.Equals("byte[]"))
                    {
                        sMethod =
                            tableSymbol + $"int o{fieldName}Count = buffer.ReadInt32();" + "\r\n" +
                            tableSymbol + $"{fieldName} = buffer.ReadBytes(o{fieldName}Count);" + "\r\n";
                    }
                    else if (sType.Equals("bool[]"))
                    {
                        sMethod = sMethod.Replace("<TYPE>", "bool");
                        sMethod = sMethod.Replace("<METHOD>", "buffer.ReadBool()");
                    }
                    else if (sType.Equals("short[]"))
                    {
                        sMethod = sMethod.Replace("<TYPE>", "short");
                        sMethod = sMethod.Replace("<METHOD>", "buffer.ReadInt16()");
                    }
                    else if (sType.Equals("ushort[]"))
                    {
                        sMethod = sMethod.Replace("<TYPE>", "ushort");
                        sMethod = sMethod.Replace("<METHOD>", "buffer.ReadUInt16()");
                    }
                    else if (sType.Equals("int[]"))
                    {
                        sMethod = sMethod.Replace("<TYPE>", "int");
                        sMethod = sMethod.Replace("<METHOD>", "buffer.ReadInt32()");
                    }
                    else if (sType.Equals("uint[]"))
                    {
                        sMethod = sMethod.Replace("<TYPE>", "uint");
                        sMethod = sMethod.Replace("<METHOD>", "buffer.ReadUInt32()");
                    }
                    else if (sType.Equals("long[]"))
                    {
                        sMethod = sMethod.Replace("<TYPE>", "long");
                        sMethod = sMethod.Replace("<METHOD>", "buffer.ReadInt64()");
                    }
                    else if (sType.Equals("ulong[]"))
                    {
                        sMethod = sMethod.Replace("<TYPE>", "ulong");
                        sMethod = sMethod.Replace("<METHOD>", "buffer.ReadUInt64()");
                    }
                    else if (sType.Equals("float[]"))
                    {
                        sMethod = sMethod.Replace("<TYPE>", "float");
                        sMethod = sMethod.Replace("<METHOD>", "buffer.ReadFloat()");
                    }
                    else if (sType.Equals("string[]"))
                    {
                        sMethod = sMethod.Replace("<TYPE>", "string");
                        sMethod = sMethod.Replace("<METHOD>", "buffer.ReadUTF8()");
                    }
                    else
                    {
                        // 自定义类型
                        string kType = sType;
                        if (kType.Contains("[]"))
                            kType = kType.Replace("[]", "");
                        sMethod =
                            tableSymbol + $"int o{fieldName}Count = buffer.ReadInt32();" + "\r\n" +
                            tableSymbol + $"{fieldName} = new {kType}[o{fieldName}Count];" + "\r\n" +
                            tableSymbol + $"for(int i = 0; i < o{fieldName}Count; i++)" + "\r\n" +
                            tableSymbol + $"__LK__" + "\r\n" +
                            tableSymbol + $"    {fieldName}[i] = new {kType}();" + "\r\n" +
                            tableSymbol + $"    {fieldName}[i].Serialization(buffer, isSerialize);" + "\r\n" +
                            tableSymbol + $"__RK__" + "\r\n";
                    }
                }
            }
            else
            {
                // 自定义类型
                sMethod = tableSymbol + $"{fieldName}.Serialization(buffer, isSerialize);" + "\r\n";
            }
            return sMethod;
        }
    }
}
