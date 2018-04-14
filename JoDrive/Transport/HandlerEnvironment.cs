using JoDriver.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace JoDriver.Handler
{
    public class HandlerEnvironment
    {
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, AbstractHandler> Handlers { get; set; } = new Dictionary<string, AbstractHandler>();
        public FlowNode FlowStart { get; private set; }

        public Stream RemoteStream { get; set; }
        public bool Cancelled { get; private set; }
        public Exception LastException { get; private set; }
        public HandleResults LastResult { get; private set; } = HandleResults.Success;

        private FlowNode current;


        public HandlerEnvironment(FlowNode flow)
        {
            FlowStart = flow;
            current = flow;
        }
        public T GetValue<T>(string key)
        {
            return (T)Values[key];
        }
        public bool TryGetValue<T>(string key, out T obj)
        {
            object o;
            if (Values.TryGetValue(key, out o))
                if (o is T)
                {
                    obj = (T)o;
                    return true;
                }
            obj = default(T);
            return false;
        }
        public void AddValue(string key, object value)
        {
            Values.Add(key, value);
        }
        public void RemoveValue(string key)
        {
            Values.Remove(key);
        }

        public HandleResults Handle(JoDriverService service)
        {
            while (true)
            {
                if (current.Itor == null)
                    current.Itor = Handlers[current.Handler].Handle(this, service);

                HandleResults re = current.Itor.Run();
                if (re == HandleResults.Pause)
                    return HandleResults.Pause;
                LastResult = re;
                current = current.GetNext(LastResult);
                if (current == null)
                    return LastResult;
            }
        }
        public void Cancel()
        {
            Cancelled = true;
        }
    }
}
