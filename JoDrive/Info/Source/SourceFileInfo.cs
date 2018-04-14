using System.Collections.Generic;

namespace JoDrive.Info.Source
{
    public class SourceFileInfo
    {
        public int Length;
        public int ChangedLength;
        public bool Deleted => Length == 0;
        public SourceChunkData[] Chunks;

        public SourceFileInfo() { }
        public SourceFileInfo(int length, List<SourceChunkData> chunks)
        {
            Length = length;
            Chunks = chunks.ToArray();
        }
    }
}
