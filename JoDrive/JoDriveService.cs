using Flowchart.Runtime;
using JoDrive.Transport;
using JoDrive.Info;
using JoDrive.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JoDrive
{
    public class JoDriveService
    {
        public string BaseFloderPath { get; private set; }
        public HashSet<string> Synchronizing { get; private set; } = new HashSet<string>();

        public IPEndPoint Remote { get; private set; }

        public Logger Log { get; private set; }

        private Queue<FlowRuntime<TransportArgs>> tasks = new Queue<FlowRuntime<TransportArgs>>();

        private TcpListener listener;
        private TcpClient remote;
        private Thread accept_thd, send_thd;
        private ManualResetEvent addtaskevent = new ManualResetEvent(false);
        private volatile bool started;

        public JoDriveService(string basePath, IPEndPoint remote)
        {
            BaseFloderPath = basePath;
            Remote = remote;
            Log = new Logger();
        }

        public void Start(IPEndPoint listen)
        {
            if (started)
                throw new Exception("服务已经启动");

            listener = new TcpListener(listen);
            listener.Start();

            accept_thd = new Thread(new ThreadStart(accept_connect));
            accept_thd.IsBackground = true;
            accept_thd.Start();

            send_thd = new Thread(new ThreadStart(send_file));
            send_thd.IsBackground = true;
            send_thd.Start();

            Directory.CreateDirectory(Path.Combine(BaseFloderPath, Setting.TemporaryDirectoryName));



            started = true;
        }
        public void Shutdown()
        {
            listener.Stop();
            if (remote != null)
                remote.Dispose();
            accept_thd.Abort();
            send_thd.Abort();
            tasks.Clear();
            started = false;
        }
        public void Upload(string relative_path)
        {
            lock (tasks)
            {
                FlowRuntime<TransportArgs> he = FlowRuntimeBuilder.BuildRequestFlowEnv(relative_path, Path.Combine(BaseFloderPath, relative_path), Operations.Upload);
                tasks.Enqueue(he);
            }
        }
        public void Download(string relative_path)
        {
            lock (tasks)
            {
                FlowRuntime<TransportArgs> he = FlowRuntimeBuilder.BuildRequestFlowEnv(relative_path, Path.Combine(BaseFloderPath, relative_path), Operations.Download);
                tasks.Enqueue(he);
            }
        }
        public void Delete(string relative_path)
        {
            lock (tasks)
            {
                FlowRuntime<TransportArgs> he = FlowRuntimeBuilder.BuildRequestFlowEnv(relative_path, Path.Combine(BaseFloderPath, relative_path), Operations.Delete);
                tasks.Enqueue(he);
            }
        }
        public void Synchronize()
        {
            addtaskevent.Set();
        }

        public void AddSynchronizingPath(string rela_path)
        {
            lock (Synchronizing)
            {
                Synchronizing.Add(rela_path);
            }
        }
        public void RemoveSynchronizingPath(string rela_path)
        {
            lock (Synchronizing)
            {
                Synchronizing.Remove(rela_path);
            }
        }
        public bool IsSynchronizing(string rela_path)
        {
            return Synchronizing.Contains(rela_path);
        }

        private void accept_connect()
        {
            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Log.Message("接受一个传输文件请求，来自：" + client.Client.RemoteEndPoint.ToString());
                    Task.Run(() =>
                     {
                         NetworkStream ns = client.GetStream();
                         FlowRuntime<TransportArgs> env = FlowRuntimeBuilder.BuildResponseFlowEnv();
                         env.Args.Service = this;
                         env.Args.RemoteStream = ns;
                         env.Run();

                         client.Close();
                         Log.Message("断开远程传输连接");
                     });
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    break;
                }
            }
        }

        private void send_file()
        {
            while (true)
            {
                addtaskevent.WaitOne();
                addtaskevent.Reset();
                FlowRuntime<TransportArgs>[] ts = null;
                lock (tasks)
                {
                    ts = tasks.ToArray();
                    tasks.Clear();
                }
                if (ts.Length <= 0)
                    continue;
                try
                {
                    remote = new TcpClient();
                    remote.Connect(Remote);
                }
                catch (Exception ex)
                {
                    if (remote != null)
                    {
                        remote.Close();
                        remote = null;
                    }
                    Log.Message("无法连接远程节点：" + Remote.Address);
                    continue;
                }
                Log.Message("连接到远程节点：" + Remote.Address);
                NetworkStream ns = remote.GetStream();
                foreach (var s in ts)
                {
                    s.Args.Service = this;
                    s.Args.RemoteStream = ns;
                    s.Run();
                    Log.Message($"文件：{s.Args.TargetRelativePath} 同步结束");
                    //if (re == HandleResults.Failed)
                    //{
                    //    //TODO 触发事件
                    //    break;
                    //}
                    //else if (re == HandleResults.Success)
                    //{
                    //    Log.Message($"文件：{s.GetValue<string>("relative_path")} 同步结束");
                    //}
                }
                Log.Message("与远程节点断开连接\n");
                remote.Close();
                remote = null;
            }
        }


    }
}
