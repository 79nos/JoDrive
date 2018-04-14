using JoDrive.Core;
using System.IO;

namespace JoDrive.Info.Pattern
{
    public class PatternHashInfo
    {
        public ChunkHash[] Hashes;

        public PatternHashInfo() { }
        public void Write(BinaryWriter output)
        {
            output.Write(Hashes.Length);
            foreach (var s in Hashes)
                s.Write(output);
        }
        public void Read(BinaryReader input)
        {
            int len = input.ReadInt32();
            Hashes = new ChunkHash[len];
            for (int s = 0; s < len; s++)
            {
                Hashes[s] = new ChunkHash();
                Hashes[s].Read(input);
            }
        }
    }
}
