using JoDrive.Info.Pattern;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace JoDrive.Core
{
    public class PatternInfoBuilder
    {
        public PatternAdler32Info BuildAdler32Info(Stream input, int chunksize)
        {
            PatternAdler32Info info = new PatternAdler32Info(chunksize);

            int ccount = (int)(input.Length / chunksize);
            info.Adler32s = new ChunkAdler32[ccount];
            byte[] buffer = new byte[chunksize];

            for (int s = 0; s < ccount; s++)
            {
                input.Read(buffer, 0, chunksize);
                uint adler32sum = Algorithm.Adler32(buffer, 0, chunksize);
                info.Adler32s[s] = new ChunkAdler32(s, adler32sum);
            }
            return info;
        }
        public PatternHashInfo BuildHashInfo(Stream input, int chunksize, string hashname, int[] selectedChunkID)
        {
            MD5 md5 = MD5.Create(hashname);
            PatternHashInfo hashinfo = new PatternHashInfo();

            HashAlgorithm halg = HashAlgorithm.Create(hashname);

            Dictionary<int, byte[]> memhashes = new Dictionary<int, byte[]>();
            List<ChunkHash> hashes = new List<ChunkHash>();
            byte[] buffer = new byte[chunksize];
            for (int s = 0; s < selectedChunkID.Length; s++)
            {
                byte[] hash = null;
                if (!memhashes.TryGetValue(selectedChunkID[s], out hash))
                {
                    int pos = chunksize * selectedChunkID[s];
                    if (input.Position != pos)
                        input.Position = pos;
                    input.Read(buffer, 0, chunksize);
                    hash = halg.ComputeHash(buffer, 0, chunksize);
                    memhashes.Add(selectedChunkID[s], hash);
                }
                ChunkHash ch = new ChunkHash();
                ch.ID = selectedChunkID[s];
                ch.Hash = hash;
                hashes.Add(ch);
            }
            hashinfo.Hashes = hashes.ToArray();
            return hashinfo;
        }
    }
}
