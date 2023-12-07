using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Avocado.FwUpgradeLIb
{
    public class UpgradeComm
    {
        public enum BusStatusTypes
        {
            Idle,
            Waiting,
            Timeout,
        }

        #region Private fields
        private Socket _CommandSocket;
        private object _SendThreadLock;
        private byte[] _ReceiveBuf;
        private int _ReceivedIndex;
        #endregion Private fields

        #region Constructor
        private static UpgradeComm _Instance;
        private static object _InstanceCreateLock = new object();
        public static UpgradeComm GetInstance()
        {
            lock (_InstanceCreateLock)
            {
                if (_Instance == null)
                {
                    _Instance = new UpgradeComm();
                }
                return _Instance;
            }
        }
        protected UpgradeComm()
        {
            _SendThreadLock = new object();
            _ReceiveBuf = new byte[1024000];
        }
        #endregion Constructor

        #region Public Properties
        public BusStatusTypes BusStatus { get; protected set; }
        public bool IsConnected { get; protected set; }
        public string ErrorMessage { get; protected set; }

        public EPCSIdTypes EPCSId { get { return UpgradeProtocol.EPCSId; } }
        public byte ReconfigTrigger { get { return UpgradeProtocol.ReconfigTrigger; } }
        public string UpgradeInfo { get { return UpgradeProtocol.UpgradeInfo; } }
        public EPCSFlashContent FlashContent { get { return UpgradeProtocol.FlashContent; } }
        public string LastUpgradeInfo { get; protected set; }
        #endregion Public Properties

        #region Public Functions
        public bool Connect()
        {
            try
            {
                if (IsConnected) { return true; }
                _CommandSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                EndPoint pt = new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 110 }), 5000);
                _CommandSocket.Bind(new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 100 }), 0));
                _CommandSocket.Connect(pt);
                _CommandSocket.ReceiveTimeout = 1000;

                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
        public bool Disconnect()
        {
            try
            {
                if (!IsConnected) { return true; }
                _CommandSocket.Close();
                IsConnected = false;
                return true;
            }
            catch(Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
        public bool UpgraderReadEpcsId()
        {
            byte[] cmd = UpgradeProtocol.UpgraderReadEpcsId();
            return SendBytes(cmd);
        }
        public bool GetUserImageVersions()
        {
            byte[] cmd = { 0x6a, 0x01, 0x01, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x6e };
            return SendBytes(cmd);
        }
        public bool UpgraderReadEpcsContent(int startAddr, ushort length, byte[] rxbuf, int offset)
        {
            byte[] cmd = UpgradeProtocol.UpgraderReadEpcsContent(startAddr, length);
            if (SendBytes(cmd))
            {
                Buffer.BlockCopy(UpgradeProtocol.FlashContent.Content, 0, rxbuf, offset, length);
                return true;
            }
            return false;
        }
        public bool UpgraderWriteEpcsContent(int startAddr, ushort length, byte[] txbuf, int offset)
        {
            byte[] dat = new byte[length];
            Buffer.BlockCopy(txbuf, offset, dat, 0, length);
            byte[] cmd = UpgradeProtocol.UpgraderWriteEpcsContent(startAddr, length, dat);
            return SendBytes(cmd);
        }
        public bool UpgraderEraseSector(int addr)
        {
            byte[] cmd = UpgradeProtocol.UpgraderEraseSector(addr);
            return SendBytes(cmd);
        }
        public bool UpgraderReconfigFPGA(bool toUserMode)
        {
            byte[] cmd = UpgradeProtocol.UpgraderReconfigFPGA(toUserMode);
            return SendBytes(cmd);
        }
        public bool UserImageSwitchToUpgrader()
        {
            byte[] cmd = UpgradeProtocol.UserImageSwitchToUpgrader();
            lock (_SendThreadLock)
            {
                if (!IsConnected) { return false; }
                try
                {
                    _CommandSocket.Send(cmd);   // FPGA should be reconfiged to factory mode now, the TCP socket is closed
                    Disconnect();
                    return true;
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        return false;
                    }

                    IsConnected = false;
                    return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return SendBytes(cmd);
        }
        public bool UpgraderReadLastUpgradeInfo()
        {
            byte[] info = new byte[256];
            if(UpgraderReadEpcsContent(0x00200000 - 512, 256, info, 0))
            {
                int infoEnd = 0;
                for(; infoEnd < 256; infoEnd++)
                {
                    if (info[infoEnd] == 0xff)
                    {
                        break;
                    }
                }
                LastUpgradeInfo = Encoding.ASCII.GetString(info, 0, infoEnd);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion Public Functions

        protected bool SendBytes(byte[] cmd)
        {
            lock (_SendThreadLock)
            {
                if (!IsConnected) { return false; }
                try
                {
                    _CommandSocket.Send(cmd);
                    _ReceivedIndex = _CommandSocket.Receive(_ReceiveBuf);
                    return UpgradeProtocol.ResponseDecoding(_ReceiveBuf, _ReceivedIndex);
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    if(ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        return false;
                    }

                    IsConnected = false;
                    Connect();
                    return false;
                }
                catch (Exception ex)
                {
                    return false;
                }

            }
        }
    }
}
