using System.Collections.Generic;

namespace JoDriver.Handler
{
    class Delay : AbstractHandler
    {
        public override IEnumerator<HandleResults> Handle(HandlerEnvironment env, JoDriverService service)
        {
            yield return HandleResults.Pause;
            yield return HandleResults.Success;
        }
    }
}
