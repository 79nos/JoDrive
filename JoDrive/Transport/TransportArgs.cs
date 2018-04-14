using Flowchart.Runtime;
using JoDrive.Info;
using System.IO;

namespace JoDrive.Transport
{
    class TransportArgs : RuntimeArgs
    {
        public JoDriveService Service { get; set; }
        public Stream RemoteStream { get; set; }
        public string TargetRelativePath { get; set; }
        public string TargetFullPath { get; set; }
        public Operations Operation { get; set; }

        
        //public TransportArgs(JoDriverService service, Stream stream)
        //{
        //    Service = service;
        //    RemoteStream = stream;
        //}
    }
}
