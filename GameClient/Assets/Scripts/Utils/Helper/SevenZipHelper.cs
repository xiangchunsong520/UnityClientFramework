using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class SevenZipHelper {
    public static void CompressFile(string inFile, string outFile)
    {
        try
        {
            FileStream input = new FileStream(inFile, FileMode.Open);
            FileStream output = new FileStream(outFile, FileMode.Create);

            output.Write(BitConverter.GetBytes((int)input.Length), 0, 4);

            SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();
            coder.WriteCoderProperties(output);

            coder.Code(input, output, input.Length, -1, null);

            output.Flush();
            output.Close();
            input.Flush();
            input.Close();
        }
        catch (System.Exception ex)
        {
            Debugger.LogError(ex.Message);
        }
    }

    public static bool DecompressFile(byte[] inFile, FileStream output)
    {
        bool rsl = true;
        MemoryStream input = new MemoryStream(inFile);
        input.Position = 0;

        byte[] fileLengthBytes = new byte[4];
        input.Read(fileLengthBytes, 0, 4);
        int fileLength = BitConverter.ToInt32(fileLengthBytes, 0);

        byte[] properties = new byte[5];
        input.Read(properties, 0, 5);

        try
        {
            SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
            coder.SetDecoderProperties(properties);
            coder.Code(input, output, input.Length, fileLength, null);
        }
        catch (System.Exception ex)
        {
            Debugger.LogError(ex.Message);
            rsl = false;
        }

        output.Flush();
        output.Close();

        return rsl;
    }

    public static bool DecompressFile(string inPath , string outFile)
    {
        bool rsl = true;
        FileStream output = new FileStream(outFile, FileMode.Create);
        FileStream input = new FileStream(inPath, FileMode.Open);
        input.Position = 0;

        byte[] fileLengthBytes = new byte[4];
        input.Read(fileLengthBytes, 0, 4);
        int fileLength = BitConverter.ToInt32(fileLengthBytes, 0);

        byte[] properties = new byte[5];
        input.Read(properties, 0, 5);

        try
        {

            SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
            coder.SetDecoderProperties(properties);
            coder.Code(input, output, input.Length, fileLength, null);
        }
        catch (System.Exception ex)
        {
            Debugger.LogError(ex.Message);
            rsl = false;
        }

        input.Flush();
        input.Close();
        output.Flush();
        output.Close();

        return rsl;
    }

    public static byte[] DecompressBuffer(byte[] inbuffer)
    {
        MemoryStream input = new MemoryStream(inbuffer);
        MemoryStream output = new MemoryStream();
        input.Position = 0;

        byte[] fileLengthBytes = new byte[4];
        input.Read(fileLengthBytes, 0, 4);
        int fileLength = BitConverter.ToInt32(fileLengthBytes, 0);

        byte[] properties = new byte[5];
        input.Read(properties, 0, 5);

        try
        {

            SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
            coder.SetDecoderProperties(properties);
            coder.Code(input, output, input.Length, fileLength, null);

            output.Position = 0;
            byte[] bytes = new byte[output.Length];
            output.Read(bytes, 0, bytes.Length);

            return bytes;
        }
        catch (System.Exception ex)
        {
            Debugger.LogError(ex.Message);
            return null;
        }
    }
}
