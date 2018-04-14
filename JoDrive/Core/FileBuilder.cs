using JoDrive.Info.Pattern;
using JoDrive.Info.Source;
using System.IO;

namespace JoDrive.Core
{
    public class FileBuilder
    {
        private Stream patternstream;
        private Stream output;
        private PatternAdler32Info patterinfo;

        public FileBuilder(Stream patternstream, Stream output, PatternAdler32Info patterinfo)
        {
            this.patternstream = patternstream;
            this.output = output;
            this.patterinfo = patterinfo;
        }

        public void AddChunkData(SourceChunkData data)
        {
            if (data.Position != output.Position)
                output.Position = data.Position;
            if (data.ChunkID != -1)
            {
                patternstream.Position = data.ChunkID * patterinfo.ChunkSize;
                byte[] buffer = new byte[patterinfo.ChunkSize];
                patternstream.Read(buffer, 0, patterinfo.ChunkSize);// TODO 返回值必须=patterinfo.ChunkSize
                output.Write(buffer, 0, patterinfo.ChunkSize);
            }
            else
            {
                output.Write(data.Data, 0, data.Length);
            }
        }
    }
}
