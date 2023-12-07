using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.CommunicationLib
{
    public class FifoBuffer
    {
        #region Private Fields
        private byte[] _Buffer;
        private int _ReadPt;        // point to next data to be read
        private int _WritePt;       // point to next data to be writen
        private bool _Overlaped;
        private object _ThreadLock;
        #endregion Private Fields

        #region Constructor
        public FifoBuffer(int bufSize = 8192)
        {
            _WritePt = 0;
            _ReadPt = 0;
            _Overlaped = false;
            BufSize = bufSize;
            if (bufSize > 0)
            {
                _Buffer = new byte[bufSize];
            }
            _ThreadLock = new object();
        }
        #endregion Constructor

        #region Public Properties
        public int BufSize { get; private set; }
        public int StoredSize
        {
            get
            {
                if (_Overlaped)     // write point is less than read point
                {
                    return BufSize - _ReadPt + _WritePt;
                }
                else
                {
                    return _WritePt - _ReadPt;
                }
            }
        }
        public int FreeSize
        {
            get
            {
                return BufSize - StoredSize;
            }
        }
        #endregion Public Properties

        #region Public Functions
        public void WriteDataIn(byte[] source, int offset, int length)
        {
            if (offset + length > source.Length)
            {
                throw new ArgumentOutOfRangeException("Given offset & length would exceed the range of the source array");
            }
            if (length > FreeSize)
            {
                throw new ArgumentOutOfRangeException("Given length is larger than available buffer size");
            }

            if (_Overlaped)
            {
                Array.Copy(source, offset, _Buffer, _WritePt, length);
                _WritePt += length;
            }
            else
            {
                int lengthToEnd = BufSize - _WritePt;
                if (lengthToEnd >= length)
                {
                    Buffer.BlockCopy(source, offset, _Buffer, _WritePt, length);
                    //Array.Copy(source, offset, _Buffer, _WritePt, length);
                    _WritePt += length;
                    if (_WritePt == BufSize)
                    {
                        _WritePt = 0;
                        _Overlaped = true;
                    }
                }
                else
                {
                    Buffer.BlockCopy(source, offset, _Buffer, _WritePt, lengthToEnd);
                    Buffer.BlockCopy(source, offset + lengthToEnd, _Buffer, 0, length - lengthToEnd);
                    //Array.Copy(source, offset, _Buffer, _WritePt, lengthToEnd);
                    //Array.Copy(source, offset + lengthToEnd, _Buffer, 0, length - lengthToEnd);
                    _WritePt = length - lengthToEnd;
                    _Overlaped = true;
                }
            }
        }
        public void ReadDataOut(byte[] target, int offset, int length)
        {
            if (offset + length > target.Length)
            {
                throw new ArgumentOutOfRangeException("Given offset & length would exceed the range of the target array");
            }
            if (length > StoredSize)
            {
                throw new ArgumentOutOfRangeException("Gigen length is larger than available data");
            }
            if (_Overlaped)
            {
                int lengthToEnd = BufSize - _ReadPt;
                if (lengthToEnd >= length)
                {
                    Array.Copy(_Buffer, _ReadPt, target, offset, length);
                    _ReadPt += length;
                    if (_ReadPt == BufSize)
                    {
                        _ReadPt = 0;
                        _Overlaped = false;
                    }
                }
                else
                {
                    Array.Copy(_Buffer, _ReadPt, target, offset, lengthToEnd);
                    Array.Copy(_Buffer, 0, target, offset + lengthToEnd, length - lengthToEnd);
                    _ReadPt = length - lengthToEnd;
                    _Overlaped = false;
                }
            }
            else
            {
                Array.Copy(_Buffer, _ReadPt, target, offset, length);
                _ReadPt += length;
            }
        }
        public void Reset()
        {
            _ReadPt = 0;
            _WritePt = 0;
            _Overlaped = false;
        }
        #endregion Public Functions
    }
}
