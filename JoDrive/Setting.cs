using Flowchart.Importer.PosFileImpoter;
using Flowchart.Model;
using Flowchart.Model.Generator;
using JoDrive.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoDrive
{
    public static class Setting
    {
        public static int ChunkSize = 512;
        public static int BufferSize = ChunkSize * 8;

        public static string MD5HashName { get; private set; } = "MD5";
        public static string TemporaryDirectoryName { get; private set; } = "download";

        public static FlowMetadata RequestMetadata { get; set; }
        public static FlowMetadata ResponseMetadata { get; set; }

        public static void Initialization(FlowMetadata request, FlowMetadata response)
        {
            RequestMetadata = request;
            ResponseMetadata = response;


        }
    }
}
