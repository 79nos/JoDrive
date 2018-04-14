using JoDrive.Core;
using System.Collections.Generic;

namespace JoDrive.Info.Source
{
    class CollidedChunk
    {
        public int Position;
        public byte[] Hash;
        public uint Adler32;
        public List<ChunkAdler32> PatternChunks;

        public CollidedChunk(int pos, uint adler32, List<ChunkAdler32> patternchunks)
        {
            Position = pos;
            Adler32 = adler32;
            PatternChunks = patternchunks;
        }
    }
}
