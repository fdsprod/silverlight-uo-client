using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Client.Network
{
    public class MessagePump
    {
        private byte[] m_Peek;

        public MessagePump()
        {
            m_Peek = new byte[4];
        }

        private const int BufferSize = 4096;
        private BufferPool m_Buffers = new BufferPool("Processor", 4, BufferSize);

        public bool OnReceive(INetState ns)
        {
            ByteQueue buffer = ns.Buffer;

            if (buffer == null || buffer.Length <= 0)
                return true;

            lock (buffer)
            {
                int length = buffer.Length;

                while (length > 0 && ns.Running)
                {
                    int packetID = buffer.GetPacketID();

                    PacketHandler handler = ns.GetHandler(packetID);

                    if (handler == null)
                    {
                        byte[] data = new byte[length];
                        length = buffer.Dequeue(data, 0, length);

                        new PacketReader(data, length, false).Trace(ns);

                        break;
                    }

                    int packetLength = handler.Length;

                    if (packetLength <= 0)
                    {
                        if (length >= 3)
                        {
                            packetLength = buffer.GetPacketLength();

                            if (packetLength < 3)
                            {
                                ns.Dispose();
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (length >= packetLength)
                    {
                        byte[] packetBuffer;

                        if (BufferSize >= packetLength)
                            packetBuffer = m_Buffers.AcquireBuffer();
                        else
                            packetBuffer = new byte[packetLength];

                        packetLength = buffer.Dequeue(packetBuffer, 0, packetLength);

                        PacketReader r = new PacketReader(packetBuffer, packetLength, handler.Length != 0);

                        handler.OnReceive(ns, r);

                        length = buffer.Length;

                        if (BufferSize >= packetLength)
                            m_Buffers.ReleaseBuffer(packetBuffer);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return true;
        }
    }
}
