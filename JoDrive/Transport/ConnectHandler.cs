using JoDriver.Handler.Receiver;
using JoDriver.Handler.Sender;
using JoDriver.Info;
using JoDriver.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace JoDriver.Handler
{
    class ConnectHandler : AbstractHandler
    {
        public override IEnumerator<HandleResults> Handle(HandlerEnvironment env, JoDriverService service)//Stream remote, JoDriverService service
        {
            BinaryReader br = new BinaryReader(env.RemoteStream);
            while (true)
            {
                RequestInfo request = new RequestInfo();
                try
                {
                    request.Read(br);
                    service.Log.Message($"处理一个传输请求，IsUpload：{request.IsUpload}  IsDelete：{request.IsDelete}");
                }
                catch (Exception ex)
                {
                    service.Log.Warning("传输关闭：" + ex.ToString());
                    break;
                }
                if (request.IsDownload)
                {
                    service.Log.Debug("请求下载文件：" + request.RelativePath);
                    createDownloadEnv(env, request, service.BaseFloderPath);
                }
                else
                {
                    service.Log.Debug("请求上传文件：" + request.RelativePath);
                    createUploadEnv(env, request, service.BaseFloderPath);
                }
                if (request.IsUpload)
                    env.AddValue("is_delete", request.IsDelete);
                yield return HandleResults.Success;
                if (request.IsUpload)
                    env.RemoveValue("is_delete");
            }
            yield return HandleResults.Failed;

            yield return HandleResults.Success;
        }

        private void createDownloadEnv(HandlerEnvironment env, RequestInfo request, string rootPath)
        {
            env.Handlers.Clear();
            env.Values.Clear();

            string full_path = Path.Combine(rootPath, request.RelativePath);
            env.AddValue("full_path", full_path);
            env.AddValue("relative_path", request.RelativePath);
            env.AddValue("is_upload", false);

            env.Handlers.Add("recv_request", this);
            env.Handlers.Add("prepare_stream", new SenderStream());
            env.Handlers.Add("send_response", new ResponseHandler());
            env.Handlers.Add("transport", new SendFile());
        }
        private void createUploadEnv(HandlerEnvironment env, RequestInfo request, string rootPath)
        {
            env.Handlers.Clear();
            env.Values.Clear();

            string full_path = Path.Combine(rootPath, request.RelativePath);
            env.AddValue("full_path", full_path);
            env.AddValue("relative_path", request.RelativePath);
            env.AddValue("is_upload", true);

            env.Handlers.Add("recv_request", this);
            env.Handlers.Add("prepare_stream", new ReceiverStream());
            env.Handlers.Add("send_response", new ResponseHandler());
            env.Handlers.Add("transport", new ReceiveFile());
        }
    }
}
