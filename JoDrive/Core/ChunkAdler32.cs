using System.IO;

namespace JoDrive.Core
{
    public class ChunkAdler32
    {
        public int ID;
        public uint Adler32;

        public ChunkAdler32() { }
        public ChunkAdler32(int id, uint adler32)
        {
            ID = id;
            Adler32 = adler32;
        }
        public void Write(BinaryWriter output)
        {
            output.Write(ID);
            output.Write(Adler32);
        }
        public void Read(BinaryReader input)
        {
            ID = input.ReadInt32();
            Adler32 = input.ReadUInt32();
        }
    }
}
