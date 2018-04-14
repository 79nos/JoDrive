using JoDrive.Info.Source;
using System;
using System.IO;

namespace JoDrive.Core
{
    public class ChunkSender
    {
        public void Send(SourceFileInfo info, Stream output, Stream fileinput)
        {
            BinaryWriter bw = new BinaryWriter(output);
            bw.Write(info.Length);
            bw.Write(info.Chunks.Length);
            foreach (var s in info.Chunks)
                send_chunkdata(s, bw, fileinput);
        }
        private void send_chunkdata(SourceChunkData data, BinaryWriter output, Stream fileinput)
        {
            output.Write(data.Position);
            output.Write(data.ChunkID);
            if (data.ChunkID == -1)
            {
                output.Write(data.Length);

                if (data.Data == null)
                {
                    if (fileinput.Position != data.Position)
                        fileinput.Position = data.Position;
                    byte[] buffer = new byte[8192];
                    int len = data.Length;
                    while (len > 0)
                    {
                        int read = fileinput.Read(buffer, 0, Math.Min(len, 8192));
                        output.Write(buffer, 0, read);
                        len -= read;
                    }
                }
                else
                {
                    output.Write(data.Data, 0, data.Data.Length);
                }
            }
        }
    }
}
