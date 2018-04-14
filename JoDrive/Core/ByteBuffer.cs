using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoDrive.Core
{
    public class ByteBuffer
    {
        public long StreamLength => stream.Length;
        public int StreamPosition => readPosition + bufPos;
        public int BufferRemains => read - bufPos;
        public byte this[int index] => buffer[bufPos + index];

        private Stream stream;
        private byte[] buffer;
        private int read, readPosition;
        private int bufPos = 0;

        public ByteBuffer(Stream stream, int buffersize)
        {
            this.stream = stream;
            buffer = new byte[buffersize];
        }
        public int Need(int len)
        {
            if (BufferRemains >= len)
                return len;
            stream.Position -= BufferRemains;
            readPosition = (int)stream.Position;
            read = stream.Read(buffer, 0, buffer.Length);
            bufPos = 0;
            return Math.Min(BufferRemains, len);
        }
        /// <summary>
        /// 只能前进
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool Move(uint next)
        {
            if (bufPos + next > read)
                return false;
            bufPos += (int)next;
            return true;
        }
        /// <summary>
        /// 不要修改buffer的值
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public byte[] GetByteBuffer(out int start)
        {
            start = bufPos;
            return buffer;
        }
    }
}
