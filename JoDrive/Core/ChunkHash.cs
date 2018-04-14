using System.IO;

namespace JoDrive.Core
{
    public class ChunkHash
    {
        public int ID;
        public byte[] Hash;

        public ChunkHash() { }
        public ChunkHash(int id, byte[] hash)
        {
            ID = id;
            Hash = new byte[hash.Length];
            hash.CopyTo(Hash, 0);
        }
        public void Write(BinaryWriter output)
        {
            output.Write(ID);
            output.Write(Hash.Length);
            output.Write(Hash);
        }
        public void Read(BinaryReader input)
        {
            ID = input.ReadInt32();
            int len = input.ReadInt32();
            Hash = input.ReadBytes(len);
        }
    }
}
