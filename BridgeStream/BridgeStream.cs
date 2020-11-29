using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GnarlyGames.Serializers
{
    public class BridgeStream
    {
        private byte[] _buffer;
        private int _readIndex = 0;
        private int _writeIndex = 0;
        private int _capacity;
        private const int DefaultCapacity = 16;

        public bool Empty => _buffer.Length == 0;
        public bool HasMore => _buffer.Length > _readIndex;

        public BridgeStream()
        {
            Clear();
        }

        public BridgeStream(byte[] data)
        {
            _buffer = data;
            _capacity = _buffer.Length;
        }

        private void WriteByteArray(byte[] bytes)
        {
            GrowBuffer(bytes.Length);
            Buffer.BlockCopy(bytes, 0, _buffer, _writeIndex, bytes.Length);
            _writeIndex += bytes.Length;
        }

        private void GrowBuffer(int length)
        {
            var isNeedResize = false;
            while (_writeIndex + length > _capacity)
            {
                _capacity *= 2;
                isNeedResize = true;
            }

            if (isNeedResize)
            {
                var resizeBuffer = new byte[_capacity];
                Buffer.BlockCopy(_buffer, 0, resizeBuffer, 0, _writeIndex);
                _buffer = resizeBuffer;
            }
        }

        public void Write(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes.Length);
            WriteByteArray(bytes);
        }

        public unsafe void Write(int value)
        {
            GrowBuffer(4);

            fixed (byte* bufferPointer = _buffer)
            {
                *(int*) (bufferPointer + _writeIndex) = value;
            }

            _writeIndex += 4;
        }

        public void Write(byte value)
        {
            GrowBuffer(1);
            _buffer[_writeIndex] = value;
            _writeIndex += 1;
        }

        public byte ReadByte()
        {
            var value = _buffer[_readIndex];
            _readIndex++;
            return value;
        }

        public unsafe void Write(float value)
        {
            GrowBuffer(4);

            fixed (byte* bufferPointer = _buffer)
            {
                *(float*) (bufferPointer + _writeIndex) = value;
            }

            _writeIndex += 4;
            // var byteValue = BitConverter.GetBytes(value);
            // WriteBuffer(byteValue);
        }

        public void Write(int[] value)
        {
            Write(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }

        public void Write(List<int> value)
        {
            Write(value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                Write(value[i]);
            }
        }

        public void Write(List<string> value)
        {
            Write(value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                Write(value[i]);
            }
        }

        public void Write(List<float> value)
        {
            Write(value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                Write(value[i]);
            }
        }


        public byte[] Encode()
        {
            var finalArray = new byte[_writeIndex];
            Buffer.BlockCopy(_buffer, 0, finalArray, 0, _writeIndex);
            return finalArray;
        }

        public int ReadInt()
        {
            var value = ToInt(_buffer, _readIndex);
            _readIndex += 4;
            return value;
        }

        private int ToInt(byte[] buffer, int startIndex)
        {
            return buffer[startIndex] | (buffer[startIndex + 1] << 8) | (buffer[startIndex + 2] << 16) |
                   (buffer[startIndex + 3] << 24);
        }

        private unsafe float ToFloat(byte[] buffer, int startIndex)
        {
            var val = ToInt(buffer, startIndex);
            return *(float*) &val;
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

        public List<string> ReadStringList()
        {
            var length = ReadInt();
            var value = new List<string>(length);
            for (var i = 0; i < length; i++)
            {
                value.Add(ReadString());
            }

            return value;
        }

        public List<float> ReadFloatList()
        {
            var length = ReadInt();
            var value = new List<float>(length);
            for (var i = 0; i < length; i++)
            {
                value.Add(ReadFloat());
            }

            return value;
        }

        public float ReadFloat()
        {
            var value = ToFloat(_buffer, _readIndex);
            _readIndex += 4;
            return value;
        }

        public string ReadString()
        {
            var length = ReadInt();
            var value = Encoding.UTF8.GetString(_buffer, _readIndex, length);
            _readIndex += length;
            return value;
        }

        public void Write(byte[] data)
        {
            Write(data.Length);
            WriteByteArray(data);
        }

        public void Write(Vector3 vector3)
        {
            Write(vector3.x);
            Write(vector3.y);
            Write(vector3.z);
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public byte[] ReadByteArray()
        {
            var length = ReadInt();
            var data = new byte[length];
            if (length != 0)
                Array.Copy(_buffer, _readIndex, data, 0, data.Length);
            _readIndex += length;
            return data;
        }


        public void Write(BridgeStream bridgeStream)
        {
            if (bridgeStream == null)
            {
                Write(0);
                return;
            }

            var data = bridgeStream.Encode();
            Write(data);
        }

        public BridgeStream ReadStream()
        {
            var packet = new BridgeStream(ReadByteArray());
            return packet;
        }

        public void Write(IBridgeSerializer bridgeSerializer)
        {
            var packet = new BridgeStream();
            bridgeSerializer.Write(packet);
            Write(packet);
        }

        public T Read<T>() where T : IBridgeSerializer, new()
        {
            var returnObject = new T();
            var packet = ReadStream();
            returnObject.Read(packet);

            return returnObject;
        }
        
        
        public IBridgeSerializer Read(Type type)
        {
            var returnObject = (IBridgeSerializer) Activator.CreateInstance(type);
            var packet = ReadStream();
            returnObject.Read(packet);
            return returnObject;
        }


        public void Clear()
        {
            _buffer = new byte[DefaultCapacity];
            _capacity = DefaultCapacity;
            _writeIndex = 0;
            _readIndex = 0;
        }

        public void Write(Quaternion quaternion)
        {
            Write(quaternion.x);
            Write(quaternion.y);
            Write(quaternion.z);
            Write(quaternion.w);
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public void WriteArray<T>(T[] bridgeSerializer) where T : IBridgeSerializer
        {
            Write(bridgeSerializer.Length);
            foreach (var serializer in bridgeSerializer)
            {
                Write(serializer);
            }
        }

        public T[] ReadArray<T>() where T : IBridgeSerializer, new()
        {
            var length = ReadInt();
            var array = new T[length];

            for (var i = 0; i < length; i++)
            {
                array[i] = Read<T>();
            }

            return array;
        }

        public void WriteList<T>(List<T> bridgeSerializer) where T : IBridgeSerializer
        {
            Write(bridgeSerializer.Count);
            foreach (var serializer in bridgeSerializer)
            {
                Write(serializer);
            }
        }

        public List<T> ReadList<T>() where T : IBridgeSerializer, new()
        {
            var length = ReadInt();
            var list = new List<T>(length);

            for (var i = 0; i < length; i++)
            {
                list.Add(Read<T>());
            }

            return list;
        }

        public void Write(bool data)
        {
            Write((byte) (data ? 1 : 0));
        }

        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        public static implicit operator bool(BridgeStream stream)
        {
            return stream != null;
        }
    }
}