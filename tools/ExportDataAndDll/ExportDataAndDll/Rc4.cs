//auth: Xiang ChunSong 2015/10/08
//purpose:

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class Rc4
{
    static byte idx = 0;
    public static byte[] key = { 1, 9, 8, 6, 0, 9, 0, 2 };
    static byte keylen = 8;
    public struct rc4_state
    {
        public int x;
        public int y;
        public int[] m;
    };

    static void rc4_setup(ref rc4_state s, byte[] key, int length)
    {
        int i, j, k, a;
        int[] m = new int[256];

        s.x = 0;
        s.y = 0;
        m = s.m;

        for (i = 0; i < 256; i++)
        {
            m[i] = i;
        }

        j = k = 0;

        for (i = 0; i < 256; i++)
        {
            a = m[i];
            j = (byte)(j + a + key[k]);
            m[i] = m[j]; m[j] = a;
            if (++k >= length) k = 0;
        }
    }

    static void rc4_crypt(ref rc4_state s, ref byte[] data, int length)
    {
        int i, x, y, a, b;
        int[] m = new int[256];

        x = s.x;
        y = s.y;
        m = s.m;

        for (i = 0; i < length; i++)
        {
            x = (byte)(x + 1); a = m[x];
            y = (byte)(y + a);
            m[x] = b = m[y];
            m[y] = a;
            data[i] ^= (byte)m[(byte)(a + b)];
        }

        s.x = x;
        s.y = y;
    }

    static void rc4_encrypt(byte[] key, ref byte[] data, int key_length, int data_len)
    {
        rc4_state s;
        s.x = 0;
        s.y = 0;
        s.m = new int[256];
        rc4_setup(ref s, key, key_length);
        rc4_crypt(ref s, ref data, data_len);
    }

    static void rc4_decrypt(byte[] key, ref byte[] data, int key_length, int data_len)
    {
        rc4_state s;
        s.x = 0;
        s.y = 0;
        s.m = new int[256];
        rc4_setup(ref s, key, key_length);
        rc4_crypt(ref s, ref data, data_len);
    }

    public static bool rc4_go(ref byte[] Out, byte[] In, long datalen, byte[] Key, int keylen, byte Type)
    {
        if (Type == 0)
        {
            //  memcpy(Out, In, datalen);
            Out = In;

            rc4_encrypt(Key, ref Out, keylen, (int)datalen);
            return true;
        }
        else
        {
            //    memcpy(Out, In, datalen);
            Out = In;

            rc4_decrypt(Key, ref Out, keylen, (int)datalen);
            return true;
        }
    }

    static void addsize(ref byte[] sendBytes, UInt16 offset)
    {
        byte arry1 = (byte)(offset & 0xFF);
        byte arry2 = (byte)((offset & 0xFF00) >> 8);

        sendBytes[0] = arry1;
        sendBytes[1] = arry2;

    }

    static byte getbyte(ref byte[] sendBytes, ref UInt16 offset)
    {
        byte ret = sendBytes[offset];
        offset += 1;
        return ret;
    }

    static Int16 getint16(ref byte[] sendBytes, ref UInt16 offset)
    {
        byte arry1 = sendBytes[offset];
        byte arry2 = sendBytes[offset + 1];
        offset += 2;

        return (Int16)(arry1 + arry2 * 256);
    }

    static Int32 getint32(ref byte[] sendBytes, ref UInt16 offset)
    {
        byte arry1 = sendBytes[offset];
        byte arry2 = sendBytes[offset + 1];
        byte arry3 = sendBytes[offset + 2];
        byte arry4 = sendBytes[offset + 3];
        offset += 4;

        return (Int32)(arry1 + arry2 * 256 + arry3 * 256 * 256 + arry4 * 256 * 256 * 256);
    }

    static string getstring(ref byte[] sendBytes, ref UInt16 offset)
    {
        Int16 len = getint16(ref sendBytes, ref  offset);
        byte[] str = new byte[len];
        for (int i = 0; i < len; i++)
        {
            str[i] = getbyte(ref sendBytes, ref offset);
        }

        return System.Text.Encoding.Unicode.GetString(str); ;
    }

    static void addbyte(ref byte[] sendBytes, ref UInt16 offset, byte bValue)
    {
        sendBytes[offset] = bValue;
        offset += 1;
    }

    static void addint16(ref byte[] sendBytes, ref UInt16 offset, Int16 bValue)
    {
        byte arry1 = (byte)(bValue & 0xFF);
        byte arry2 = (byte)((bValue & 0xFF00) >> 8);

        addbyte(ref sendBytes, ref offset, arry1);
        addbyte(ref sendBytes, ref offset, arry2);
    }

    static void addint32(ref byte[] sendBytes, ref UInt16 offset, Int32 bValue)
    {
        byte arry1 = (byte)(bValue & 0xFF);
        byte arry2 = (byte)((bValue & 0xFF00) >> 8);
        byte arry3 = (byte)((bValue & 0xFF0000) >> 16);
        byte arry4 = (byte)((bValue >> 24) & 0xFF);

        addbyte(ref sendBytes, ref offset, arry1);
        addbyte(ref sendBytes, ref offset, arry2);
        addbyte(ref sendBytes, ref offset, arry3);
        addbyte(ref sendBytes, ref offset, arry4);
    }

    static void addstring(ref byte[] sendBytes, ref UInt16 offset, string bValue)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(bValue);       //服务器用unicode编码，转一下
        addint16(ref sendBytes, ref offset, (Int16)bytes.Length);
        for (int i = 0; i < bytes.Length; i++)
        {
            addbyte(ref sendBytes, ref offset, bytes[i]);
        }
    }

    public static string StringToUnicode(string s)
    {
        char[] charbuffers = s.ToCharArray();
        byte[] buffer;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < charbuffers.Length; i++)
        {
            buffer = System.Text.Encoding.Unicode.GetBytes(charbuffers[i].ToString());
            sb.Append(String.Format("//u{0:X2}{1:X2}", buffer[1], buffer[0]));
        }
        return sb.ToString();
    }  
}
