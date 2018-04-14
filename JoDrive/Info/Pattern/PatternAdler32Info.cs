using JoDrive.Core;
using System.IO;

namespace JoDrive.Info.Pattern
{
    public class PatternAdler32Info
    {
        public int ChunkSize;
        public bool AsNewFile => Adler32s == null || Adler32s.Length == 0;
        public ChunkAdler32[] Adler32s;

        public PatternAdler32Info() { }
        public PatternAdler32Info(int chunkSize)
        {
            ChunkSize = chunkSize;
        }

        public void Write(BinaryWriter output)
        {
            output.Write(ChunkSize);
            output.Write(Adler32s.Length);
            foreach (var s in Adler32s)
                s.Write(output);
        }
        public void Read(BinaryReader input)
        {
            ChunkSize = input.ReadInt32();
            int chunkc = input.ReadInt32();
            Adler32s = new ChunkAdler32[chunkc];
            for (int s = 0; s < chunkc; s++)
            {
                Adler32s[s] = new ChunkAdler32();
                Adler32s[s].Read(input);
            }
        }
    }
}
