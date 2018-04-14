using System.Collections.Generic;
using System.IO;

namespace JoDrive.Info.Source
{
    public class SourceAdler32Info
    {
        public int[] SelectedChunkID;

        public SourceAdler32Info() { }
        public SourceAdler32Info(List<int> selectedChunk)
        {
            SelectedChunkID = selectedChunk.ToArray();
        }
        public void Write(BinaryWriter output)
        {
            output.Write(SelectedChunkID.Length);
            for (int s = 0; s < SelectedChunkID.Length; s++)
                output.Write(SelectedChunkID[s]);
        }
        public void Read(BinaryReader input)
        {
            int len = input.ReadInt32();
            SelectedChunkID = new int[len];
            for (int s = 0; s < SelectedChunkID.Length; s++)
                SelectedChunkID[s] = input.ReadInt32();
        }
    }
}
