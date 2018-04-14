using Flowchart.Model;
using Flowchart.Runtime;
using JoDrive.Core;
using JoDrive.Info;
using JoDrive.Info.Pattern;
using JoDrive.Info.Source;
using JoDrive.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace JoDrive.Transport
{
    static class TransportMethods
    {
        public static FlowOutput RequestBranch(FlowRuntime<TransportArgs> rt, TransportArgs args)
        {
            return new FlowOutput(args.Operation.ToString());
        }
        public static IEnumerator<FlowOutput> PrepareSendStream(FlowRuntime<TransportArgs> rt, TransportArgs args)
        {
            if (args.Service.IsSynchronizing(args.TargetRelativePath))
            {
                yield return FlowOutput.Failed;
                yield break;
            }

            FileStream local = null;
            if (!Utility.TryGet(() => new FileStream(args.TargetFullPath, FileMode.Open, FileAccess.Read, FileShare.Read), out local, out _))
            {
                args.Service.Log.Debug($"打开发送文件：{args.TargetRelativePath} 失败");
                yield return FlowOutput.Failed;
                yield break;
            }
            args.Service.AddSynchronizingPath(args.TargetRelativePath);
            args.Service.Log.Debug($"打开发送文件：{args.TargetRelativePath} 成功");
            rt.SetValue("local_file", local);
            yield return new FlowOutput("Open");
            args.Service.Log.Debug($"关闭文件：{args.TargetRelativePath}");
            local?.Dispose();
            args.Service.RemoveSynchronizingPath(args.TargetRelativePath);
            rt.RemoveValue("local_file");
            yield return new FlowOutput("Close");
        }
        public static IEnumerator<FlowOutput> PrepareReceiveStream(FlowRuntime<TransportArgs> rt, TransportArgs args)
        {
            string temp_path = args.TargetFullPath;


            if (args.Service.IsSynchronizing(args.TargetRelativePath))
            {
                yield return FlowOutput.Failed;
                yield break;
            }

            FileStream local = null;
            FileStream temp = null;
            bool file_existed = File.Exists(args.TargetFullPath);
            bool suc = true;
            try
            {
                if (file_existed)
                {
                    local = new FileStream(args.TargetFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    temp_path = Path.Combine(args.Service.BaseFloderPath, Setting.TemporaryDirectoryName, Utility.AddRandomTail(args.TargetRelativePath));
                }
                temp = new FileStream(temp_path, FileMode.Create, FileAccess.Write, FileShare.Read);
                args.Service.AddSynchronizingPath(args.TargetRelativePath);
            }
            catch (Exception ex)
            {
                args.Service.Log.Warning("传输准备失败，原因：" + ex.ToString());
                suc = false;
            }
            if (!suc)
            {
                yield return FlowOutput.Failed;
                yield break;
            }
            args.Service.Log.Message("传输准备完成");

            rt.SetValue("local_file", local);
            rt.SetValue("temp_path", temp_path);
            rt.SetValue("temp_file", temp);
            yield return new FlowOutput("Open");

            //收尾

            args.Service.Log.Message("关闭文件流");
            temp?.Dispose();
            local?.Dispose();
            if (rt.LastOutput.Equals(FlowOutput.Success))
            {
                if (file_existed)
                {
                    File.Delete(args.TargetFullPath);
                    File.Move(temp_path, args.TargetFullPath);
                }
            }
            else
            {
                File.Delete(temp_path);
            }

            args.Service.RemoveSynchronizingPath(args.TargetRelativePath);
            rt.RemoveValue("local_file");
            rt.RemoveValue("temp_path");
            rt.RemoveValue("temp_file");

            yield return new FlowOutput("Close");
        }

        public static FlowOutput RequestAndResponse(FlowRuntime<TransportArgs> rt, TransportArgs args)
        {
            BinaryWriter bw = new BinaryWriter(args.RemoteStream);
            BinaryReader br = new BinaryReader(args.RemoteStream);


            RequestInfo request = new RequestInfo(args.TargetRelativePath, args.Operation);
            ResponseInfo response = new ResponseInfo();
            try
            {
                request.Write(bw);
                args.Service.Log.Debug("发送传输请求");
                response.Read(br);
                args.Service.Log.Debug("获得回应");
            }
            catch (Exception ex)
            {
                args.Service.Log.Warning("传输请求发送失败，原因：" + ex.ToString());
                return FlowOutput.Failed;
            }
            if (response.NeedDelay)
                return FlowOutput.Failed;
            return FlowOutput.Success;
        }
        public static FlowOutput ReadRequest(FlowRuntime<TransportArgs> rt, TransportArgs args)
        {
            BinaryReader br = new BinaryReader(args.RemoteStream);
            RequestInfo request = new RequestInfo();
            try
            {
                request.Read(br);
                rt.Args.TargetRelativePath = request.RelativePath;
                rt.Args.TargetFullPath = Path.Combine(args.Service.BaseFloderPath, request.RelativePath);

                args.Service.Log.Message($"处理一个传输请求，目标：{request.RelativePath} 动作：{request.Operation}");
                return new FlowOutput(request.Operation.ToString());
            }
            catch (Exception ex)
            {
                args.Service.Log.Warning("传输关闭：" + ex.ToString());
                return FlowOutput.Failed;
            }
        }
        public static FlowOutput SendResponse(FlowRuntime<TransportArgs> rt, TransportArgs args)
        {
            BinaryWriter bw = new BinaryWriter(args.RemoteStream);
            try
            {
                if (rt.LastOutput.Equals(new FlowOutput("Open")) || rt.LastOutput.Equals(FlowOutput.Success))
                {
                    ResponseInfo ready = new ResponseInfo(true);
                    ready.Write(bw);
                    args.Service.Log.Message("准备工作完成");
                }
                else if (rt.LastOutput.Equals(FlowOutput.Failed))
                {
                    ResponseInfo delay = new ResponseInfo(false);
                    delay.Write(bw);
                    args.Service.Log.Warning("准备工作失败，通知对方推迟传输");
                }
                return FlowOutput.Success;
            }
            catch (Exception ex)
            {
                //TODO 打印异常
                return FlowOutput.Failed;
            }
        }


        public static FlowOutput SendFile(FlowRuntime<TransportArgs> rt, TransportArgs args)
        {
            Stream local_file = rt.GetValue<Stream>("local_file");
            BinaryWriter bw = new BinaryWriter(args.RemoteStream);
            BinaryReader br = new BinaryReader(args.RemoteStream);

            try
            {
                PatternAdler32Info pattern = new PatternAdler32Info();
                pattern.Read(br);//获取对方模板文件Adler32结果
                args.Service.Log.Debug($"接收Adler32数据，块大小：{pattern.ChunkSize} 共：{pattern.Adler32s.Length}块");

                SourceFileInfo src = null;
                if (!pattern.AsNewFile)//对方有模板文件
                {
                    string hashname = br.ReadString(); //获取对方使用的强哈希算法
                    args.Service.Log.Debug("获取哈希算法：" + hashname);

                    SourceInfoBuilder sb = new SourceInfoBuilder();

                    SourceRawInfo raw = sb.BuildRaw(local_file, pattern);//寻找可疑块
                    args.Service.Log.Debug($"可疑块：{raw.Collideds.Count}块");

                    SourceAdler32Info adler32info = raw.GetAdler32Info();
                    adler32info.Write(bw);//发送可疑块
                    args.Service.Log.Debug($"发送可疑块");

                    sb.ComputeHash(local_file, hashname, raw);//计算hash值
                    args.Service.Log.Debug($"计算本地文件哈希值");

                    PatternHashInfo patternHashes = new PatternHashInfo();
                    patternHashes.Read(br);//获取对方计算的哈希值
                    args.Service.Log.Debug("双方计算哈希值");

                    src = sb.BuildFileInfo(patternHashes, raw);//比对两份哈希值，计算需要上传的数据
                    args.Service.Log.Debug($"要发送的数据：{raw.ChangedLength}字节");

                }
                else//对方没有模板文件
                {
                    List<SourceChunkData> chunks = new List<SourceChunkData>();
                    chunks.Add(new SourceChunkData(0, (int)local_file.Length, null));
                    src = new SourceFileInfo((int)local_file.Length, chunks);
                    args.Service.Log.Debug($"直接发送整个文件：{local_file.Length}字节");

                }

                ChunkSender sender = new ChunkSender();
                sender.Send(src, args.RemoteStream, local_file);//发送文件数据
                args.Service.Log.Debug("文件数据成功发送");

                return FlowOutput.Success;
            }
            catch (Exception ex)
            {
                //TODO 打印异常
                return FlowOutput.Failed;
            }
        }
        public static FlowOutput DeleteFile(FlowRuntime<TransportArgs> rt, TransportArgs args)
        {
            try
            {
                File.Delete(args.TargetFullPath);
            }
            catch (Exception ex)
            {
                //TODO 打印错误信息
                return FlowOutput.Failed;
            }
            return FlowOutput.Success;
        }
        public static FlowOutput ReceiveFile(FlowRuntime<TransportArgs> rt, TransportArgs args)
        {
            BinaryWriter bw = new BinaryWriter(args.RemoteStream);
            BinaryReader br = new BinaryReader(args.RemoteStream);

            Stream local_file = rt.GetValue<Stream>("local_file");
            Stream temp_file = rt.GetValue<Stream>("temp_file");

            try
            {
                PatternAdler32Info adler32info = null;
                if (local_file != null)//有模板文件
                {
                    PatternInfoBuilder pb = new PatternInfoBuilder();
                    adler32info = pb.BuildAdler32Info(local_file, Setting.ChunkSize);

                    adler32info.Write(bw);//发送本地文件的Adler32数据
                    args.Service.Log.Debug($"发送Adler32数据，块大小：{adler32info.ChunkSize} 共：{adler32info.Adler32s.Length}块");

                    if (!adler32info.AsNewFile)
                    {
                        bw.Write(Setting.MD5HashName);//发送本地文件使用的哈希算法
                        args.Service.Log.Debug("发送哈希算法：" + Setting.MD5HashName);

                        SourceAdler32Info srcAdler32 = new SourceAdler32Info();
                        srcAdler32.Read(br);//读取对方发现的可疑块
                        args.Service.Log.Debug($"收到可疑块：{srcAdler32.SelectedChunkID.Length}块");

                        PatternHashInfo hashinfo = pb.BuildHashInfo(local_file, Setting.ChunkSize, Setting.MD5HashName, srcAdler32.SelectedChunkID);//计算可疑块的哈希值

                        hashinfo.Write(bw);//发送可疑块的哈希值
                        args.Service.Log.Debug("发送可疑块的哈希值");
                    }
                }
                else//没有模板文件
                {
                    adler32info = new PatternAdler32Info(0);
                    adler32info.Adler32s = new ChunkAdler32[0];
                    adler32info.Write(bw);//发送空的Adler32数据
                    args.Service.Log.Debug("发送空的Adler32数据");
                }


                FileBuilder fb = new FileBuilder(local_file, temp_file, adler32info);
                ChunkReceiver sfr = new ChunkReceiver();
                sfr.Receive(args.RemoteStream, fb);//接收数据，写入新文件

                args.Service.Log.Message("文件接收完成");
                return FlowOutput.Success;
            }
            catch (Exception ex)
            {
                //TODO 打印异常
                return FlowOutput.Failed;
            }
        }
    }
}
