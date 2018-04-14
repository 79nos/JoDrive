using System.IO;

namespace JoDrive.Info
{
    public enum Operations { Upload, Download, Delete }
    public class RequestInfo
    {
        public string RelativePath;
        public Operations Operation;

        public RequestInfo() { }
        public RequestInfo(string relapath, Operations op)
        {
            RelativePath = relapath;
            Operation = op;
        }

        public void Write(BinaryWriter output)
        {
            //output.Write(Setting.RequestID);
            output.Write((int)Operation);
            output.Write(RelativePath);
        }
        public void Read(BinaryReader input)
        {
            Operation = (Operations)input.ReadInt32();
            RelativePath = input.ReadString();
        }
    }
}
