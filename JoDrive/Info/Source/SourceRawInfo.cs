using System.Collections.Generic;

namespace JoDrive.Info.Source
{
    class SourceRawInfo
    {
        public int StreamLength;
        public int ChangedLength;
        public int ChunkSize;
        public List<CollidedChunk> Collideds;
        public List<SourceChunkData> Chunks;

        public SourceRawInfo(int streamLen, int chunkSize, List<CollidedChunk> collideds, List<SourceChunkData> chunks)
        {
            StreamLength = streamLen;
            ChunkSize = chunkSize;
            Collideds = collideds;
            Chunks = chunks;
        }
        public SourceAdler32Info GetAdler32Info()
        {
            HashSet<int> ids = new HashSet<int>();
            foreach (var s in Collideds)
                foreach (var d in s.PatternChunks)
                    ids.Add(d.ID);
            List<int> idss = new List<int>(ids);
            idss.Sort();
            SourceAdler32Info adler32 = new SourceAdler32Info(idss);
            return adler32;
        }
    }
}
