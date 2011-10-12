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
using Client.Network;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Client.Network
{
    public class NetState
    {
        private static BufferPool m_ReceiveBufferPool = new BufferPool("Receive", 2048, 2048);

        private Socket m_Socket;
        private IPAddress m_Address;
        private ByteQueue m_Buffer;
        private SendQueue m_SendQueue;
        private MessagePump m_MessagePump;
        private bool m_Seeded;
        private bool m_Running;
        protected AsyncCallback m_OnReceive, m_OnSend;
        private byte[] m_RecvBuffer;
        private int m_Sequence;
        private bool m_CompressionEnabled;
        private string m_ToString;
        private bool m_SentFirstPacket;
        private bool m_BlockAllPackets;
        private DateTime m_ConnectedOn;
        private int m_Seed;
        //private AsyncState m_AsyncState;
        private object m_AsyncLock = new object();
        //private IPacketEncoder m_Encoder = null;
        //private static NetStateCreatedCallback m_CreatedCallback;
        private static int m_CoalesceSleep = -1;

        public SendQueue SendQueue
        {
            get { return m_SendQueue; }
        }

        public DateTime ConnectedOn
        {
            get { return m_ConnectedOn; }
        }

        public TimeSpan ConnectedFor
        {
            get { return (DateTime.Now - m_ConnectedOn); }
        }

        public int Seed
        {
            get { return m_Seed; }
            set { m_Seed = value; }
        }

        public IPAddress Address
        {
            get { return m_Address; }
        }

        //public AsyncState AsyncState
        //{
        //    get { return m_AsyncState; }
        //    set { m_AsyncState = value; }
        //}

        public object AsyncLock
        {
            get { return m_AsyncLock; }
        }
        
        //public IPacketEncoder PacketEncoder
        //{
        //    get { return m_Encoder; }
        //    set { m_Encoder = value; }
        //}
        
        //public static NetStateCreatedCallback CreatedCallback
        //{
        //    get { return m_CreatedCallback; }
        //    set { m_CreatedCallback = value; }
        //}

        public bool SentFirstPacket
        {
            get { return m_SentFirstPacket; }
            set {  m_SentFirstPacket = value; }
        }

        public bool BlockAllPackets
        {
            get { return m_BlockAllPackets; }
            set { m_BlockAllPackets = value; }
        }

        public bool CompressionEnabled
        {
            get { return m_CompressionEnabled; }
            set { m_CompressionEnabled = value; }
        }

        public int Sequence
        {
            get { return m_Sequence; }
            set { m_Sequence = value; }
        }

        public static int CoalesceSleep
        {
            get { return m_CoalesceSleep; }
            set { m_CoalesceSleep = value; }
        }

        public void BeginReceive()
        {
            //m_AsyncState |= AsyncState.Pending;
            //m_Socket.BeginReceive(m_RecvBuffer, 0, m_RecvBuffer.Length, SocketFlags.None, m_OnReceive, m_Socket);
        }

        public event EventHandler Connected;

        public NetState()
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_MessagePump = new MessagePump();
            m_Buffer = new ByteQueue();
            m_Seeded = false;
            m_Running = false;
            m_RecvBuffer = m_ReceiveBufferPool.AcquireBuffer();

            m_SendQueue = new SendQueue();

            m_NextCheckActivity = DateTime.Now + TimeSpan.FromMinutes(0.5);

            //try
            //{
            //    m_Address = Utility.Intern(((IPEndPoint)m_Socket.RemoteEndPoint).Address);
            //    m_ToString = m_Address.ToString();
            //}
            //catch (Exception ex)
            //{
            //    TraceException(ex);
            //    m_Address = IPAddress.None;
            //    m_ToString = "(error)";
            //}
        }

        public void Connect(string ip, int port)
        {
            if (!IPAddress.TryParse(ip, out m_Address))
            {
                throw new Exception("Invalid IP, the ip must be a valid ip address and cannot be a host name.");
            }

            IPEndPoint endPoint = new IPEndPoint(m_Address, port);
            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();

            connectArgs.UserToken = m_Socket;
            connectArgs.RemoteEndPoint = endPoint;
            connectArgs.Completed += OnConnected;

            m_Socket.ConnectAsync(connectArgs);
        }

        void OnConnected(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= OnConnected;

            SocketError errorCode = e.SocketError;

            if (errorCode != SocketError.Success)
            {
                throw new SocketException((Int32)errorCode);
            }

            m_ConnectedOn = DateTime.Now;

            var handler = Connected;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public virtual void Send(Packet p)
        {
            if (m_Socket == null || m_BlockAllPackets)
            {
                p.OnSend();
                return;
            }

            //PacketSendProfile prof = PacketSendProfile.Acquire(p.GetType());

            int length;
            byte[] buffer = p.Compile(m_CompressionEnabled, out length);

            if (buffer != null)
            {
                if (buffer.Length <= 0 || length <= 0)
                {
                    p.OnSend();
                    return;
                }

                //if (prof != null)
                //{
                //    prof.Start();
                //}

                //if (m_Encoder != null)
                //{
                //    m_Encoder.EncodeOutgoingPacket(this, ref buffer, ref length);
                //}

                try
                {
                    lock (m_SendQueue)
                    {
                        try
                        {
                            //m_Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, m_OnSend, m_Socket);
                        }
                        catch (Exception ex)
                        {
                            TraceException(ex);
                            Dispose(false);
                        }
                    }
                }
                catch (CapacityExceededException)
                {
                    Debug.WriteLine("Client: {0}: Too much data pending, disconnecting...", this);
                    Dispose(false);
                }

                p.OnSend();

                //if (prof != null)
                //{
                //    prof.Finish(length);
                //}
            }
            else
            {
                Debug.WriteLine("Client: {0}: null buffer send, disconnecting...", this);
                //using (StreamWriter op = new StreamWriter("null_send.log", true))
                //{
                    Debug.WriteLine(string.Format("{0} Client: {1}: null buffer send, disconnecting...", DateTime.Now, this));
                    Debug.WriteLine(new System.Diagnostics.StackTrace().ToString());
                //}
                Dispose();
            }
        }

        public bool Flush()
        {
            if (m_Socket == null || !m_SendQueue.IsFlushReady)
            {
                return false;
            }

            SendQueue.Gram gram;

            lock (m_SendQueue)
            {
                gram = m_SendQueue.CheckFlushReady();
            }

            if (gram != null)
            {
                try
                {
                    //m_Socket.BeginSend(gram.Buffer, 0, gram.Length, SocketFlags.None, m_OnSend, m_Socket);
                    return true;
                }
                catch (Exception ex)
                {
                    TraceException(ex);
                    Dispose(false);
                }
            }

            return false;
        }

        private void OnSend(IAsyncResult asyncResult)
        {
            Socket s = (Socket)asyncResult.AsyncState;

            try
            {
                //int bytes = s.EndSend(asyncResult);

                //if (bytes <= 0)
                //{
                //    Dispose(false);
                //    return;
                //}

                m_NextCheckActivity = DateTime.Now + TimeSpan.FromMinutes(1.2);

                if (m_CoalesceSleep >= 0)
                {
                    Thread.Sleep(m_CoalesceSleep);
                }

                SendQueue.Gram gram;

                lock (m_SendQueue)
                {
                    gram = m_SendQueue.Dequeue();
                }

                if (gram != null)
                {
                    try
                    {
                        //s.BeginSend(gram.Buffer, 0, gram.Length, SocketFlags.None, m_OnSend, s);
                    }
                    catch (Exception ex)
                    {
                        TraceException(ex);
                        Dispose(false);
                    }
                }
            }
            catch (Exception)
            {
                Dispose(false);
            }
        }

        public virtual void Start()
        {
            m_OnReceive = new AsyncCallback(OnReceive);
            m_OnSend = new AsyncCallback(OnSend);

            m_Running = true;

            if (m_Socket == null)
            {
                return;
            }

            try
            {
                lock (m_AsyncLock)
                {
                    //if ((m_AsyncState & (AsyncState.Pending | AsyncState.Paused)) == 0)
                    {
                        BeginReceive();
                    }
                }
            }
            catch (Exception ex)
            {
                TraceException(ex);
                Dispose(false);
            }
        }

        public void LaunchBrowser(string url)
        {

        }

        private DateTime m_NextCheckActivity;

        public bool CheckAlive()
        {
            if (m_Socket == null)
            {
                return false;
            }

            if (DateTime.Now < m_NextCheckActivity)
            {
                return true;
            }

            Console.WriteLine("Client: {0}: Disconnecting due to inactivity...", this);

            Dispose();
            return false;
        }

        private void OnReceive(IAsyncResult asyncResult)
        {
            Socket s = (Socket)asyncResult.AsyncState;

            try
            {
                //int byteCount = s.EndReceive(asyncResult);

                //if (byteCount > 0)
                //{
                //    m_NextCheckActivity = DateTime.Now + TimeSpan.FromHours(12);

                //    byte[] buffer = m_RecvBuffer;

                //    if (m_Encoder != null)
                //        m_Encoder.DecodeIncomingPacket(this, ref buffer, ref byteCount);

                //    lock (m_Buffer)
                //        m_Buffer.Enqueue(buffer, 0, byteCount);

                //    m_MessagePump.OnReceive(this);

                //    lock (m_AsyncLock)
                //    {
                //        m_AsyncState &= ~AsyncState.Pending;

                //        if ((m_AsyncState & AsyncState.Paused) == 0)
                //        {
                //            try
                //            {
                //                BeginReceive();
                //            }
                //            catch (Exception ex)
                //            {
                //                TraceException(ex);
                //                Dispose(false);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    Dispose(false);
                //}
            }
            catch
            {
                Dispose(false);
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }

        public static void TraceException(Exception ex)
        {
            try
            {
                //using (StreamWriter op = new StreamWriter("network-errors.log", true))
                //{
                    Debug.WriteLine(string.Format("# {0}", DateTime.Now));

                    Debug.WriteLine(ex);

                    //op.WriteLine();
                    //op.WriteLine();
                //}
            }
            catch
            {
            }

            try
            {
                Console.WriteLine(ex);
            }
            catch
            {
            }
        }

        private bool m_Disposing;

        public virtual void Dispose(bool flush)
        {
            if (m_Socket == null || m_Disposing)
            {
                return;
            }

            m_Disposing = true;

            if (flush)
                flush = Flush();

            try
            {
                m_Socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException ex)
            {
                TraceException(ex);
            }

            try
            {
                m_Socket.Close();
            }
            catch (SocketException ex)
            {
                TraceException(ex);
            }

            if (m_RecvBuffer != null)
                m_ReceiveBufferPool.ReleaseBuffer(m_RecvBuffer);

            m_Socket = null;

            m_Buffer = null;
            m_RecvBuffer = null;
            m_OnReceive = null;
            m_OnSend = null;
            m_Running = false;

            m_Disposed.Enqueue(this);

            if ( /*!flush &&*/ !m_SendQueue.IsEmpty)
            {
                lock (m_SendQueue)
                    m_SendQueue.Clear();
            }
        }

        private static Queue<NetState> m_Disposed = new Queue<NetState>();

        public bool Running
        {
            get
            {
                return m_Running;
            }
        }

        public bool Seeded
        {
            get
            {
                return m_Seeded;
            }
            set
            {
                m_Seeded = value;
            }
        }

        public Socket Socket
        {
            get
            {
                return m_Socket;
            }
        }

        public ByteQueue Buffer
        {
            get
            {
                return m_Buffer;
            }
        }
    }
}
