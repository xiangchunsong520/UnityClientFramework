using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using ProtoBuf;

public delegate void FindFileFunction(FileInfo file);

class ProtoExporter
{
    DataReader _reader;
    CsvStreamReader _csvReader;
    bool _isSingleLine;
    ExcelReader _excelReader;
    string _protoStr;
    MemoryStream _serializeStream;
    MemoryStream _tempStream;

    string _dataPath;
    string _metaPath;

    public ProtoExporter()
    {
        _csvReader = new CsvStreamReader();
        _excelReader = new ExcelReader();
        _protoStr = "syntax = \"proto3\";\n\npackage Data;\n\n";
        _serializeStream = new MemoryStream();
        _tempStream = new MemoryStream();
    }

    public void StartExport()
    {
        string path = Environment.CurrentDirectory;
        //string path = "D:/Work3.0/trunk/client/data";
        string[] texts = File.ReadAllLines(path + "/outPutPath.cfg");
        _dataPath = path + texts[0];
        _metaPath = path + texts[1];
        _dataPath = _dataPath.Replace("\\", "/");
        _metaPath = _metaPath.Replace("\\", "/");
        FileSystemInfo info = new DirectoryInfo(path);
        TraverseDirectory(info, FindCsvFile);
        //Console.Write(_protoStr);
        if (!Directory.Exists(_metaPath))
            Directory.CreateDirectory(_metaPath);
        string outFile = _metaPath + "ProtocolDatas.proto";
        File.WriteAllText(outFile, _protoStr);
        Console.WriteLine("导出完成!");
    }

    void FindCsvFile(FileInfo file)
    {
        if (!file.Name.ToLower().EndsWith(".csv") && !file.Name.ToLower().EndsWith(".xlsx") && !file.Name.ToLower().EndsWith(".xls") || file.Name.ToLower().StartsWith("~$"))
            return;

        Console.WriteLine("正在导出 : " + file.Name);
        _reader = file.Name.ToLower().EndsWith(".csv") ? (DataReader)_csvReader : (DataReader)_excelReader;
        _reader.FileName = file.FullName;
        _isSingleLine = IsSingleLine();
        _protoStr += CreateMetaString(file.Name);
        WriteFile(file.Name);
    }

    bool IsSingleLine()
    {
        for (int i = 2; i <= _reader.RowCount; ++i)
        {
            string typeName = _reader[i, 2].ToUpper();
            if (!string.IsNullOrEmpty(typeName) && !typeName.Equals("INT") && !typeName.Equals("BOOL") && !typeName.Equals("FLOAT") && !typeName.Equals("STRING"))
                return false;
        }
        return true;
    }

    string CreateMetaString(string fileName)
    {
        fileName = fileName.Replace("\\", "/");
        string name = fileName.Substring(fileName.LastIndexOf('/') + 1, fileName.LastIndexOf('.') - fileName.LastIndexOf('/') - 1);
        string meta = "message ";
        meta += name;
        meta += "\n{\n";
        int index = 1;
        int minindex = _isSingleLine ? 2 : 1;
        int maxindex = _isSingleLine ? _reader.RowCount : _reader.ColCount;
        for (int i = minindex; i <= maxindex; ++i)
        {
            string typeName = _isSingleLine ? _reader[i, 2].ToUpper() : _reader[2, i].ToUpper();
            if (!typeName.Equals("INT") && !typeName.Equals("BOOL") && !typeName.Equals("FLOAT") && !typeName.Equals("STRING"))
                continue;
            if (typeName.Equals("INT"))
                typeName = "int32";

            meta += "\t";
            meta += typeName.ToLower();
            meta += " ";
            meta += _isSingleLine ? _reader[i, 3] : _reader[3, i];
            meta += " = ";
            meta += (index++).ToString();
            meta += ";\n";
        }
        meta += "}\n\n";

        if (!_isSingleLine)
        {
            meta += "message ";
            meta += name + "List";
            meta += "\n{\n";
            meta += "\trepeated " + name + " datas = 1;\n";
            meta += "}\n\n";
        }

        return meta;
    }

    void SerializeCsvData()
    {
        _serializeStream.SetLength(0);
        _serializeStream.Position = 0;

        Assembly asb = Assembly.GetExecutingAssembly();
        int rowindex = 1;
        int startRow = _isSingleLine ? 2 : 4;
        for (int i = startRow; i <= _reader.RowCount; ++i)
        {
            _tempStream.SetLength(0);
            _tempStream.Position = 0;
            if (_isSingleLine)
            {
                string name = _reader[i, 2].ToUpper();
                if (!name.Equals("INT") && !name.Equals("BOOL") && !name.Equals("FLOAT") && !name.Equals("STRING"))
                    continue;

                string typeName = "DataType.Data" + name;
                Type type = Type.GetType(typeName);
                object obj = asb.CreateInstance(typeName);
                string propertyName = "val" + (rowindex++).ToString();
                PropertyInfo p_info = type.GetProperty(propertyName);
                p_info.SetValue(obj, GetVal(i, 4));
                Serializer.Serialize(_tempStream, obj);
            }
            else
            { 
                int index = 1;
                for (int j = 1; j <= _reader.ColCount; ++j)
                {
                    string name = _reader[2, j].ToUpper();
                    if (!name.Equals("INT") && !name.Equals("BOOL") && !name.Equals("FLOAT") && !name.Equals("STRING"))
                        continue;

                    string typeName = "DataType.Data" + name;
                    Type type = Type.GetType(typeName);
                    object obj = asb.CreateInstance(typeName);
                    string propertyName = "val" + (index++).ToString();
                    PropertyInfo p_info = type.GetProperty(propertyName);
                    p_info.SetValue(obj, GetVal(i, j));
                    Serializer.Serialize(_tempStream, obj);
                }
                _serializeStream.WriteByte(10);
                byte[] lb = GetLenthBytes(_tempStream.Length);
                _serializeStream.Write(lb, 0, lb.Length);
            }
            _serializeStream.Write(_tempStream.GetBuffer(), 0, (int)_tempStream.Length);
        }
        _serializeStream.Position = 0;
    }

    byte[] GetLenthBytes(long length)
    {
        List<byte> bytes = new List<byte>();
        long lowCheck = 0x7F;
        long lowAdd = 0x80;
        long num = length;
        while (num > 0)
        {
            if (num > lowCheck)
            {
                long low = num & lowCheck;
                long val = low | lowAdd;
                bytes.Add((byte)val);
            }
            else
                bytes.Add((byte)num);
            num = num >> 7;
        }
        return bytes.ToArray();
    }

    void WriteFile(string fileName)
    {
        fileName = fileName.Replace("\\", "/");
        string name = fileName.Substring(fileName.LastIndexOf('/') + 1, fileName.LastIndexOf('.') - fileName.LastIndexOf('/') - 1);

        if (!Directory.Exists(_dataPath))
            Directory.CreateDirectory(_dataPath);
        string outFile = _dataPath + name + ".bytes";

        FileStream output = new FileStream(outFile, FileMode.Create);

        SerializeCsvData();

        byte[] bytes = new byte[_serializeStream.Length];
        _serializeStream.Read(bytes, 0, bytes.Length);
        Rc4.rc4_go(ref bytes, bytes, bytes.Length, Rc4.key, Rc4.key.Length, 0);

        output.Write(bytes, 0, bytes.Length);

        output.Flush();
        output.Close();
    }

    object GetVal(int rol, int col)
    {
        string type = _isSingleLine ? _reader[rol, 2] : _reader[2, col];
        string val = _reader[rol, col];
        switch (type.ToLower())
        {
            case "int":
                int intVal;
                if (!int.TryParse(val, out intVal))
                {
                    Console.WriteLine("解析int数据类型错误!" + "文件:" + _reader.FileName + " 行:" + rol + " 列:" + col);
                    intVal = 0;
                }
                return intVal;
            case "float":
                float floatVal;
                if (!float.TryParse(val, out floatVal))
                {
                    Console.WriteLine("解析float数据类型错误!" + "文件:" + _reader.FileName + " 行:" + rol + " 列:" + col);
                    floatVal = 0f;
                }
                return floatVal;
            case "bool":
                bool boolVal;
                if (!bool.TryParse(val, out boolVal))
                {
                    if (val.Equals("1"))
                        boolVal = true;
                    else if (val.Equals("0"))
                        boolVal = false;
                    else
                    {
                        Console.WriteLine("解析float数据类型错误!" + "文件:" + _reader.FileName + " 行:" + rol + " 列:" + col);
                        boolVal = false;
                    }
                }
                return boolVal;
            case "string":
                return val;
            default:
                throw new Exception("文件:" + _reader.FileName + " 行:" + rol + " 数据类型字段错误!");
        }
    }

    static void TraverseDirectory(FileSystemInfo info, FindFileFunction func)
    {
        if (!info.Exists) return;
        DirectoryInfo dir = info as DirectoryInfo;
        if (dir == null) return;
        FileSystemInfo[] files = dir.GetFileSystemInfos();
        for (int i = 0; i < files.Length; i++)
        {
            FileInfo file = files[i] as FileInfo;
            if (file != null)
            {
                func(file);
            }
            else
                TraverseDirectory(files[i], func);
        }
    }
}
