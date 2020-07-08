using System.Collections.Generic;
using NUnit.Framework;
using PacketSystem;

namespace PacketTest
{
    [TestFixture]
    public class Tests
    {
        private Packet sendPacket;
        private Packet receivePacket;


        [SetUp]
        public void TestSetup()
        {
            sendPacket = new Packet();
            receivePacket = new Packet();
        }

        [Test]
        public void TestInt()
        {
            sendPacket.Write(10);
            var data = sendPacket.Encode();

            receivePacket.Decode(data);
            var sendValue = receivePacket.Read<int>();
            Assert.AreEqual(10, sendValue);
        }

        [Test]
        public void TestIntArray()
        {
            sendPacket.Write(new int[] {10, 11, 12});
            var data = sendPacket.Encode();

            receivePacket.Decode(data);
            var sendValue = receivePacket.ReadIntArray();
            var expected = new int[] {10, 11, 12};
            Assert.AreEqual(expected, sendValue);
        }

        [Test]
        public void TestIntList()
        {
            sendPacket.Write(new List<int>() {10, 11, 12});
            var data = sendPacket.Encode();

            receivePacket.Decode(data);
            var sendValue = receivePacket.Read<List<int>>();
            var expected = new List<int>() {10, 11, 12};
            Assert.AreEqual(expected, sendValue);
        }

        [Test]
        public void TestString()
        {
            sendPacket.Write("ferhat");
            var data = sendPacket.Encode();

            receivePacket.Decode(data);
            var sendValue = receivePacket.ReadString();
            Assert.AreEqual("ferhat", sendValue);
        }

        [Test]
        public void TestFloat()
        {
            sendPacket.Write(10f);
            var data = sendPacket.Encode();

            receivePacket.Decode(data);
            var sendValue = receivePacket.ReadFloat();
            Assert.AreEqual(10f, sendValue);
        }
    }
}