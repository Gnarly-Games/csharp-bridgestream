using System.Collections.Generic;
using GnarlyGames.Serializers;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;

namespace PacketTest
{
    public class MatchInfo : IBridgeSerializer
    {
        public int matchId;
        public List<int> playerIds;
        public List<string> playerNames;

        public void Read(BridgeStream stream)
        {
            matchId = stream.ReadInt();
            playerIds = stream.ReadIntList();
            playerNames = stream.ReadStringList();
        }

        public void Write(BridgeStream stream)
        {
            stream.Write(matchId);
            stream.Write(playerIds);
            stream.Write(playerNames);
        }
    }

    [TestFixture]
    public class Tests
    {
        private BridgeStream _sendBridgeStream;


        [SetUp]
        public void TestSetup()
        {
            _sendBridgeStream = new BridgeStream();
        }

        [Test]
        public void TestInt()
        {
            _sendBridgeStream.Write(10);
            var data = _sendBridgeStream.Encode();
            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadInt();
            Assert.AreEqual(10, sendValue);
        }

        [Test]
        public void TestIntArray()
        {
            _sendBridgeStream.Write(new int[] {10, 11, 12});
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadIntArray();
            var expected = new int[] {10, 11, 12};
            Assert.AreEqual(expected, sendValue);
        }

        [Test]
        public void TestIntList()
        {
            _sendBridgeStream.Write(new List<int>() {10, 11, 12});
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadIntList();
            var expected = new List<int>() {10, 11, 12};
            Assert.AreEqual(expected, sendValue);
        }

        [Test]
        public void TestString()
        {
            _sendBridgeStream.Write("şerhat");
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadString();
            Assert.AreEqual("şerhat", sendValue);
        }


        [Test]
        public void TestStringList()
        {
            _sendBridgeStream.Write(new List<string>() {"ferhat", "mehmet", "seker", "seko"});
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadStringList();
            var expected = new List<string>() {"ferhat", "mehmet", "seker", "seko"};
            Assert.AreEqual(expected[0], sendValue[0]);
            Assert.AreEqual(expected[1], sendValue[1]);
            Assert.AreEqual(expected[2], sendValue[2]);
            Assert.AreEqual(expected[3], sendValue[3]);
        }

        [Test]
        public void TestFloatList()
        {
            _sendBridgeStream.Write(new List<float>() {19f, 20f, 25f, -10f});
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadFloatList();
            var expected = new List<float>() {19f, 20f, 25f, -10f};
            Assert.AreEqual(expected[0], sendValue[0]);
            Assert.AreEqual(expected[1], sendValue[1]);
            Assert.AreEqual(expected[2], sendValue[2]);
            Assert.AreEqual(expected[3], sendValue[3]);
        }

        [Test]
        public void TestByteArray()
        {
            _sendBridgeStream.Write(new byte[4] {19, 20, 25, 10});
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadByteArray();
            var expected = new byte[4] {19, 20, 25, 10};
            Assert.AreEqual(expected[0], sendValue[0]);
            Assert.AreEqual(expected[1], sendValue[1]);
            Assert.AreEqual(expected[2], sendValue[2]);
            Assert.AreEqual(expected[3], sendValue[3]);
        }

        [Test]
        public void TestFloat()
        {
            _sendBridgeStream.Write(10f);
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadFloat();
            Assert.AreEqual(10f, sendValue);
        }

        [Test]
        public void TestByte()
        {
            byte value = 1;
            _sendBridgeStream.Write(value);
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadByte();
            Assert.AreEqual((byte) 1, sendValue);
        }

        [Test]
        public void TestBool()
        {
            _sendBridgeStream.Write(true);
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadBool();
            Assert.AreEqual(true, sendValue);
        }


        [Test]
        public void TestVector3()
        {
            _sendBridgeStream.Write(new Vector3(04f, 05f, 06f));
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadVector3();
            Assert.AreEqual(04f, sendValue.x);
            Assert.AreEqual(05f, sendValue.y);
            Assert.AreEqual(06f, sendValue.z);
        }

        [Test]
        public void TestQuaternion()
        {
            _sendBridgeStream.Write(new Quaternion(04f, 05f, 0.1f, 1.5f));
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadQuaternion();
            Assert.AreEqual(04f, sendValue.x);
            Assert.AreEqual(05f, sendValue.y);
            Assert.AreEqual(0.1f, sendValue.z);
            Assert.AreEqual(1.5f, sendValue.w);
        }


        [Test]
        public void TestNullBridgeStream()
        {
            BridgeStream stream = null;
            _sendBridgeStream.Write(stream);
            var data = _sendBridgeStream.Encode();
            var receivePacket = new BridgeStream(data);
            var sendValue = receivePacket.ReadStream();
            Assert.IsTrue(sendValue.Empty);
        }

        [Test]
        public void TestCustomType()
        {
            var matchInfo = new MatchInfo()
            {
                matchId = 10,
                playerIds = new List<int> {1, 2, 3},
                playerNames = new List<string> {"fer", "meh", "şek", "sek"}
            };

            _sendBridgeStream.Write(matchInfo);
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var returnMatchInfo = receivePacket.Read<MatchInfo>();
            Assert.AreEqual(matchInfo.matchId, returnMatchInfo.matchId);
            Assert.AreEqual(matchInfo.playerIds, returnMatchInfo.playerIds);
            Assert.AreEqual(matchInfo.playerNames, returnMatchInfo.playerNames);
        }

        [Test]
        public void TestCustomTypeList()
        {
            var list = new List<MatchInfo>()
            {
                new MatchInfo()
                {
                    matchId = 10,
                    playerIds = new List<int> {1, 2, 3},
                    playerNames = new List<string> {"fer", "meh", "şek", "sek"}
                },
                new MatchInfo()
                {
                    matchId = 15,
                    playerIds = new List<int> {1, 2, 3},
                    playerNames = new List<string> {"fer", "meh", "şek", "sek"}
                }
            };

            _sendBridgeStream.WriteList(list);
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var returnMatchInfo = receivePacket.ReadList<MatchInfo>();

            Assert.AreEqual(list[0].matchId, returnMatchInfo[0].matchId);
            Assert.AreEqual(list[1].matchId, returnMatchInfo[1].matchId);
        }


        [Test]
        public void TestCustomTypeArray()
        {
            var array = new MatchInfo[2]
            {
                new MatchInfo()
                {
                    matchId = 10,
                    playerIds = new List<int> {1, 2, 3},
                    playerNames = new List<string> {"fer", "meh", "şek", "sek"}
                },
                new MatchInfo()
                {
                    matchId = 15,
                    playerIds = new List<int> {1, 2, 3},
                    playerNames = new List<string> {"fer", "meh", "şek", "sek"}
                }
            };

            _sendBridgeStream.WriteArray(array);
            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var returnMatchInfo = receivePacket.ReadArray<MatchInfo>();

            Assert.AreEqual(array[0].matchId, returnMatchInfo[0].matchId);
            Assert.AreEqual(array[1].matchId, returnMatchInfo[1].matchId);
        }

        [Test]
        public void TestPacket()
        {
            var a = new BridgeStream();
            a.Write("Ferhat");
            a.Write(10);
            a.Write(true);
            a.Write((byte) 255);
            a.Write(15f);
            _sendBridgeStream.Write(a);

            var b = new BridgeStream();
            b.Write("Mehmet");
            b.Write(15);
            b.Write(35f);
            _sendBridgeStream.Write(b);

            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);
            var readPacketA = receivePacket.ReadStream();
            var readPacketB = receivePacket.ReadStream();

            Assert.AreEqual("Ferhat", readPacketA.ReadString(),
                "Expected to get the same string as the first read on the received packet");
            Assert.AreEqual(10, readPacketA.ReadInt(),
                "Expected to get the same integer as the second read on the received packet");
            Assert.AreEqual(true, readPacketA.ReadBool(),
                "Expected to get the same bool as the second read on the received packet");
            Assert.AreEqual((byte) 255, readPacketA.ReadByte(),
                "Expected to get the same byte as the third read on the received packet");
            Assert.AreEqual("Mehmet", readPacketB.ReadString(),
                "Expected to get the same string as the first read on the received packet");
            Assert.AreEqual(15, readPacketB.ReadInt(),
                "Expected to get the same integer as the second read on the received packet");
            Assert.AreEqual(35f, readPacketB.ReadFloat(),
                "Expected to get the same float as the third read on the received packet");
        }

        [Test]
        public void TestHasMore()
        {
            _sendBridgeStream.Write("Ferhat");
            _sendBridgeStream.Write("Ferhat");
            _sendBridgeStream.Write("Ferhat");
            _sendBridgeStream.Write("Ferhat");
            _sendBridgeStream.Write("Ferhat");

            var data = _sendBridgeStream.Encode();

            var receivePacket = new BridgeStream(data);

            var readCount = 0;
            while (receivePacket.HasMore)
            {
                receivePacket.ReadString();
                readCount++;
            }

            Assert.AreEqual(5, readCount);
        }


        [Test]
        public void FalseIfNotAssignedTest()
        {
            BridgeStream stream = null;
            Assert.IsFalse(stream);
        }

        [Test]
        public void TrueIfAssignedTest()
        {
            BridgeStream stream = new BridgeStream();
            Assert.IsTrue(stream);
        }
    }
}