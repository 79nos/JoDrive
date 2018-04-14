using JoDriver.Info;
using JoDriver.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace JoDriver.Handler
{
    class RequestHandler : AbstractHandler
    {
        public override IEnumerator<HandleResults> Handle(HandlerEnvironment env, JoDriverService service)
        {
            yield return HandleResults.Success;
            yield return env.LastResult;
        }
    }
}
