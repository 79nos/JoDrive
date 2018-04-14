using System;

namespace JoDrive.Info.Source
{
    public class SourceChunkData
    {
        public int Position;
        public int ChunkID;
        public int Length;
        public byte[] Data;

        public SourceChunkData(int pos, int len, byte[] data)
        {
            Position = pos;
            Length = len;
            if (data != null)
            {
                Data = new byte[len];
                Array.Copy(data, Data, len);
            }
            ChunkID = -1;
        }
        public SourceChunkData(int pos, int chunkid)
        {
            Position = pos;
            ChunkID = chunkid;
        }
    }
}
