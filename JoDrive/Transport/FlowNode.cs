using System.Collections.Generic;

namespace JoDriver.Handler
{
    public class FlowNode
    {
        public string Handler { get; set; }
        public IEnumerator<HandleResults> Itor { get; set; }

        public FlowNode[,] Nexts;

        public FlowNode(string handler)
        {
            Handler = handler;
            Nexts = new FlowNode[2, 5];
        }


        public FlowNode Next(FlowNode next, HandleResults lastResult)
        {
            Nexts[0, (int)lastResult] = next;
            return next;
        }
        public FlowNode Next(FlowNode next, FlowNode redir, HandleResults lastResult)
        {
            Nexts[0, (int)lastResult] = next;
            redir.Handler = next.Handler;
            Nexts[1, (int)lastResult] = redir;
            return redir;
        }

        public FlowNode DirectionalNext(FlowNode next)
        {
            for (int s = 0; s < Nexts.GetLength(1); s++)
                Nexts[0, s] = next;
            return next;
        }
        public FlowNode DirectionalNext(FlowNode next, FlowNode redir)
        {
            redir.Handler = next.Handler;
            for (int s = 0; s < Nexts.GetLength(1); s++)
            {
                Nexts[0, s] = next;
                Nexts[1, s] = redir;
            }
            return redir;
        }

        public FlowNode SuccessNext(FlowNode next)
        {
            return Next(next, HandleResults.Success);
        }
        public FlowNode SuccessNext(FlowNode next, FlowNode redir)
        {
            return Next(next, redir, HandleResults.Success);
        }

        public FlowNode FailNext(FlowNode next)
        {
            return Next(next, HandleResults.Failed);
        }
        public FlowNode FailNext(FlowNode next, FlowNode redir)
        {
            return Next(next, redir, HandleResults.Failed);
        }
        public FlowNode GetNext(HandleResults lastResult)
        {
            int index = (int)lastResult;
            if (0 <= index || index < Nexts.GetLength(1))
            {
                if (Nexts[1, index] != null)
                {
                    Nexts[1, index].Itor = Nexts[0, index]?.Itor;
                    return Nexts[1, index];
                }
                return Nexts[0, index];
            }
            return lastResult == HandleResults.Pause ? this : null;
        }
    }
}
