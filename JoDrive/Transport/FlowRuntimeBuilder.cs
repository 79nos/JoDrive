using Flowchart.Runtime;
using Flowchart.Runtime.Methods;
using JoDrive.Info;

namespace JoDrive.Transport
{
    static class FlowRuntimeBuilder
    {
        public static FlowRuntime<TransportArgs> BuildRequestFlowEnv(string rela_path, string full_path, Operations ops)
        {
            FlowRuntime<TransportArgs> fe = new FlowRuntime<TransportArgs>(Setting.RequestMetadata);

            fe.Methods.Add("RequestBranch", new SimpleMethod<TransportArgs>(TransportMethods.RequestBranch));
            fe.Methods.Add("PrepareSendStream", new EnumeratorMethod<TransportArgs>(TransportMethods.PrepareSendStream));
            fe.Methods.Add("PrepareReceiveStream", new EnumeratorMethod<TransportArgs>(TransportMethods.PrepareReceiveStream));
            fe.Methods.Add("RequestAndResponse", new SimpleMethod<TransportArgs>(TransportMethods.RequestAndResponse));
            fe.Methods.Add("SendFile", new SimpleMethod<TransportArgs>(TransportMethods.SendFile));
            fe.Methods.Add("ReceiveFile", new SimpleMethod<TransportArgs>(TransportMethods.ReceiveFile));

            fe.Args = new TransportArgs();
            fe.Args.TargetRelativePath = rela_path;
            fe.Args.TargetFullPath = full_path;
            fe.Args.Operation = ops;
            return fe;
        }
        public static FlowRuntime<TransportArgs> BuildResponseFlowEnv()
        {

            FlowRuntime<TransportArgs> fe = new FlowRuntime<TransportArgs>(Setting.ResponseMetadata);
            fe.Methods.Add("ReadRequest", new SimpleMethod<TransportArgs>(TransportMethods.ReadRequest));
            fe.Methods.Add("PrepareSendStream", new EnumeratorMethod<TransportArgs>(TransportMethods.PrepareSendStream));
            fe.Methods.Add("PrepareReceiveStream", new EnumeratorMethod<TransportArgs>(TransportMethods.PrepareReceiveStream));
            fe.Methods.Add("SendResponse", new SimpleMethod<TransportArgs>(TransportMethods.SendResponse));
            fe.Methods.Add("SendFile", new SimpleMethod<TransportArgs>(TransportMethods.SendFile));
            fe.Methods.Add("ReceiveFile", new SimpleMethod<TransportArgs>(TransportMethods.ReceiveFile));
            fe.Methods.Add("DeleteFile", new SimpleMethod<TransportArgs>(TransportMethods.DeleteFile));
            fe.Args = new TransportArgs();
            return fe;
        }
    }
}
