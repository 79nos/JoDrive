using JoDriver.Info;
using System;
using System.Collections.Generic;
using System.IO;

namespace JoDriver.Handler
{
    class SendRequest : AbstractHandler
    {
        public override IEnumerator<HandleResults> Handle(HandlerEnvironment env, JoDriverService service)
        {
            BinaryWriter bw = new BinaryWriter(env.RemoteStream);
            BinaryReader br = new BinaryReader(env.RemoteStream);


            bool isupload = env.GetValue<bool>("is_upload");
            bool isdelete = isupload && env.GetValue<bool>("is_delete");
            RequestInfo request = new RequestInfo(env.GetValue<string>("relative_path"), isupload, isdelete);
            ResponseInfo response = new ResponseInfo();
            bool fail = false;
            try
            {
                request.Write(bw);
                service.Log.Debug("发送传输请求");
                response.Read(br);
                service.Log.Debug("获得回应");
            }
            catch (Exception ex)
            {
                service.Log.Warning("传输请求发送失败，原因：" + ex.ToString());
                fail = true;
            }
            if (!isupload)
                env.AddValue("is_delete", response.IsDelete);
            if (fail || response.NeedDelay)
                yield return HandleResults.Failed;
            yield return HandleResults.Success; //请求发送成功，对方也准备完毕
        }
    }
}
