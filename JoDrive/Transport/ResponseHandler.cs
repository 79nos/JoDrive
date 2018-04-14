using JoDriver.Info;
using System.Collections.Generic;
using System.IO;

namespace JoDriver.Handler
{
    class ResponseHandler : AbstractHandler
    {

        public override IEnumerator<HandleResults> Handle(HandlerEnvironment env, JoDriverService service)
        {
            BinaryWriter bw = new BinaryWriter(env.RemoteStream);
            bool isdelete = !env.GetValue<bool>("is_upload") && env.GetValue<bool>("is_delete");
            if (env.LastResult == HandleResults.Success)
            {
                ResponseInfo ready = new ResponseInfo(true, isdelete);
                ready.Write(bw);
                service.Log.Message("准备工作完成");
            }
            else if (env.LastResult == HandleResults.Failed)
            {
                ResponseInfo delay = new ResponseInfo(false, isdelete);
                delay.Write(bw);
                service.Log.Warning("准备工作失败，通知对方推迟传输");
            }
            yield return HandleResults.Success;
        }
    }
}
