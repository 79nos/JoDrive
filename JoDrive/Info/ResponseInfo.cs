using System.IO;

namespace JoDrive.Info
{
    class ResponseInfo
    {
        public bool Ready { get => ready; set => ready = value; }
        public bool NeedDelay { get => !ready; set => ready = !value; }

        private bool ready;

        public ResponseInfo() { }
        public ResponseInfo(bool ready)
        {
            this.ready = ready;
        }

        public void Write(BinaryWriter output)
        {
            output.Write(ready);
        }
        public void Read(BinaryReader input)
        {
            ready = input.ReadBoolean();
        }
    }
}
