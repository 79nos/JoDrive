using JoDrive.Info.Source;
using System;
using System.IO;

namespace JoDrive.Core
{
    class ChunkReceiver
    {

        public void Receive(Stream input, FileBuilder fb)
        {
            BinaryReader br = new BinaryReader(input);

            int filelength = br.ReadInt32();
            int chunkc = br.ReadInt32();
            for (int s = 0; s < chunkc; s++)
            {
                int pos = br.ReadInt32();
                int id = br.ReadInt32();
                if (id != -1)
                    fb.AddChunkData(new SourceChunkData(pos, id));
                else
                {
                    SourceChunkData data = new SourceChunkData(pos, 0, null);
                    int len = br.ReadInt32();
                    byte[] buffer = new byte[8192];
                    while (len > 0)
                    {
                        int read = br.Read(buffer, 0, Math.Min(len, 8192));
                        data.Length = read;
                        data.Data = buffer;
                        fb.AddChunkData(data);
                        data.Position += read;
                        len -= read;
                    }
                }
            }
        }
    }
}
