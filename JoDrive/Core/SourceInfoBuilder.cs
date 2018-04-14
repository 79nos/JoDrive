using JoDrive.Info.Pattern;
using JoDrive.Info.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace JoDrive.Core
{
    class SourceInfoBuilder
    {
        public SourceRawInfo BuildRaw(Stream input, PatternAdler32Info info)
        {
            List<CollidedChunk> collideds = new List<CollidedChunk>();
            List<SourceChunkData> chunks = new List<SourceChunkData>();
            Dictionary<uint, List<ChunkAdler32>> patternAdler32s = new Dictionary<uint, List<ChunkAdler32>>();
            foreach (var s in info.Adler32s)
            {
                List<ChunkAdler32> list = null;
                if (!patternAdler32s.TryGetValue(s.Adler32, out list))
                    patternAdler32s.Add(s.Adler32, list = new List<ChunkAdler32>());
                list.Add(s);
            }
            ByteBuffer buf = new ByteBuffer(input, Setting.BufferSize);

            bool rolling = false;
            uint adler32 = 0;
            int fail = 0;
            int changed = 0;
            while (true)
            {
                if (!rolling)
                {
                    if (buf.Need(Setting.ChunkSize) < Setting.ChunkSize)
                        break;
                    adler32 = Algorithm.Adler32(buf, +Setting.ChunkSize);
                    rolling = true;
                }
                else
                {
                    if (buf.Need(Setting.ChunkSize + 1) < Setting.ChunkSize + 1)
                        break;
                    adler32 = Algorithm.Adler32Rolling(adler32, Setting.ChunkSize, buf[0], buf[Setting.ChunkSize]);
                    buf.Move(1);
                }

                List<ChunkAdler32> selected = null;
                if (patternAdler32s.TryGetValue(adler32, out selected))
                {
                    if (fail != buf.StreamPosition)
                    {
                        chunks.Add(new SourceChunkData(fail, buf.StreamPosition - fail, null));
                        changed += buf.StreamPosition - fail;
                    }
                    collideds.Add(new CollidedChunk(buf.StreamPosition, adler32, selected));
                    buf.Move((uint)Setting.ChunkSize);
                    rolling = false;

                    fail = buf.StreamPosition;
                }
            }
            if (buf.StreamLength != fail)
            {
                chunks.Add(new SourceChunkData(fail, (int)(buf.StreamLength - fail), null));
                changed += buf.StreamPosition - fail;
            }
            var raw = new SourceRawInfo((int)input.Length, info.ChunkSize, collideds, chunks);
            raw.ChangedLength = changed;
            return raw;
        }
        public void ComputeHash(Stream input, string hashname, SourceRawInfo raw)
        {
            HashAlgorithm hash = HashAlgorithm.Create(hashname);
            byte[] buffer = new byte[raw.ChunkSize];
            foreach (var s in raw.Collideds)
            {
                input.Position = s.Position;
                input.Read(buffer, 0, raw.ChunkSize);
                s.Hash = hash.ComputeHash(buffer, 0, raw.ChunkSize);
            }
        }
        public SourceFileInfo BuildFileInfo(PatternHashInfo pattern, SourceRawInfo raw)
        {
            Dictionary<int, ChunkHash> hashes = new Dictionary<int, ChunkHash>();
            foreach (var s in pattern.Hashes)
                hashes.Add(s.ID, s);

            foreach (var s in raw.Collideds)
            {
                int id = -1;
                foreach (var d in s.PatternChunks)
                    if (hashes[d.ID].Hash.SequenceEqual(s.Hash))
                    {
                        id = d.ID;
                        break;
                    }
                if (id != -1)
                    raw.Chunks.Add(new SourceChunkData(s.Position, id));
                else
                {
                    raw.Chunks.Add(new SourceChunkData(s.Position, raw.ChunkSize, null));
                    raw.ChangedLength += raw.ChunkSize;
                }
            }
            raw.Chunks.Sort(new Comparison<SourceChunkData>((x, y) => x.Position - y.Position));
            return new SourceFileInfo(raw.StreamLength, raw.Chunks);
        }
    }
}
