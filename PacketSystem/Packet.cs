using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using PacketSystem;

namespace PacketSystem
{
    public interface IPacketPrimitiveSerializer
    {
        byte[] Write(object value);
        object Read(byte[] buffer, ref int readIndex);
    }

    public interface IPacketCollectionSerializer
    {
        byte[] Write(IList value);
        object Read(Type elementType, byte[] buffer, ref int readIndex);
    }


    public class IntegerPrimitiveSerializer : IPacketPrimitiveSerializer
    {
        public byte[] Write(object value)
        {
            return BitConverter.GetBytes((int) value);
        }

        public object Read(byte[] buffer, ref int readIndex)
        {
            var value = BitConverter.ToInt32(buffer, readIndex);
            readIndex += 4;
            return value;
        }
    }

    public class ListPrimitiveSerializer : IPacketCollectionSerializer
    {
        public byte[] Write(IList value)
        {
            var array = value;
            var lengthArray = PacketSerializeManager.PrimitiveSerializers[typeof(int)].Write(array.Count);
            var elementType = array.GetType().GetElementType();
            var serializer = PacketSerializeManager.PrimitiveSerializers[elementType];
            var size = Marshal.SizeOf(elementType) * array.Count + 4;
            var buffer = new byte[size];

            WriteBuffer(lengthArray, buffer, 0);
            for (var i = 0; i < array.Count; i++)
            {
                WriteBuffer(serializer.Write(array[i]), buffer, 4);
            }

            return buffer;
        }

        public object Read(Type elementType, byte[] buffer, ref int readIndex)
        {
            var arrayLength =
                (int) PacketSerializeManager.PrimitiveSerializers[typeof(int)].Read(buffer, ref readIndex);
            var value = new int[arrayLength];
            var list = new List<object>();
            var elementSerializer = PacketSerializeManager.PrimitiveSerializers[elementType];
            for (var i = 0; i < value.Length; i++)
            {
                list.Add(elementSerializer.Read(buffer, ref readIndex));
            }

            return list;
        }

        private void WriteBuffer(byte[] source, byte[] destination, int destinationOffset)
        {
            for (var i = 0; i < source.Length; i++)
            {
                destination[destinationOffset] = source[i];
                destinationOffset++;
            }
        }
    }

    public static class PacketSerializeManager
    {
        public static readonly Dictionary<Type, IPacketPrimitiveSerializer> PrimitiveSerializers =
            new Dictionary<Type, IPacketPrimitiveSerializer>()
            {
                {typeof(int), new IntegerPrimitiveSerializer()},
            };

        public static readonly Dictionary<Type, IPacketCollectionSerializer> CollectionSerializers =
            new Dictionary<Type, IPacketCollectionSerializer>()
            {
                {typeof(List<>), new ListPrimitiveSerializer()}
            };
    };
}

public class Packet
{
    private byte[] _buffer;
    private int _readIndex = 0;
    private int _writeIndex = 0;

    public Packet()
    {
        _buffer = new byte[512];
    }

    private void WriteBuffer(byte[] bytes)
    {
        for (var i = 0; i < bytes.Length; i++)
        {
            _buffer[_writeIndex] = bytes[i];
            _writeIndex++;
        }
    }

    public void Write(string value)
    {
        Write(value.Length);
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteBuffer(bytes);
    }

    public void Write(object value)
    {
        var type = value.GetType();
        if (type.IsPrimitive)
        {
            WriteBuffer(PacketSerializeManager.PrimitiveSerializers[type].Write(value));
        }
        else if (type.IsArray)
        {
            // WriteBuffer(PacketSerializeManager.CollectionSerializers[type].Write(value));
        }
        else if (type.IsClass)
        {
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                WriteBuffer(PacketSerializeManager.CollectionSerializers[genericType]
                    .Write(value as IList));
            }
        }
    }

    // public void Write(int value)
    // {
    //     var byteValue = BitConverter.GetBytes(value);
    //     WriteBuffer(byteValue);
    // }

    // public void Write(float value)
    // {
    //     var byteValue = BitConverter.GetBytes(value);
    //     WriteBuffer(byteValue);
    // }

    public void Write(int[] value)
    {
        Write(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            Write(value[i]);
        }
    }

    // public void Write(List<int> value)
    // {
    //     Write(value.Count);
    //     for (var i = 0; i < value.Count; i++)
    //     {
    //         Write(value[i]);
    //     }
    // }

    public byte[] Encode()
    {
        return _buffer;
    }

    public void Decode(byte[] data)
    {
        _buffer = data;
    }

    public T Read<T>()
    {
        return (T) Read(typeof(T));
    }

    private object Read(Type type)
    {
        if (type.IsPrimitive)
        {
            return PacketSerializeManager.PrimitiveSerializers[type].Read(_buffer, ref _readIndex);
        }
        else if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return PacketSerializeManager.CollectionSerializers[type].Read(elementType, _buffer, ref _readIndex);
        }

        return null;
    }


    public int ReadInt()
    {
        var value = BitConverter.ToInt32(_buffer, _readIndex);
        _readIndex += 4;
        return value;
    }

    public int[] ReadIntArray()
    {
        var length = ReadInt();
        var value = new int[length];
        for (var i = 0; i < value.Length; i++)
        {
            value[i] = ReadInt();
        }

        return value;
    }

    public List<int> ReadIntList()
    {
        var length = ReadInt();
        var value = new List<int>(length);
        for (var i = 0; i < length; i++)
        {
            value.Add(ReadInt());
        }

        return value;
    }

    public float ReadFloat()
    {
        var value = BitConverter.ToSingle(_buffer, _readIndex);
        _readIndex += 4;
        return value;
    }

    public object ReadString()
    {
        var length = ReadInt();
        var value = Encoding.UTF8.GetString(_buffer, _readIndex, length);
        _readIndex += length;
        return value;
    }
}