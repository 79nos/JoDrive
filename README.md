# JoDrive
基于rsync算法的增量同步程序，每个程序节点支持多进一出<del>（多进可能会有Bug）</del>。

**目前正在重构**

### 使用
首先需要设定同步文件的程序流程：（这里需要用到Flowchart库）
```C#
FlowMetadata request = XXXXX;
FlowMetadata response = XXXXX;
Setting.Initialization(request,response);
```
然后就可以使用了：
```C#
IPEndPoint remote = new IPEndPoint(remoteIP,remotePort);//远程节点的IP号和端口号
JoDriveService service = new JoDriveService(remote,syncFloder);//再设定同步的文件夹
service.Start(new IPEndPoint(lisIP,lisPort));//设定监听的IP与端口号，并开始监听

service.Upload("asd.txt");//向远程节点上传asd.txt文件，这个文件在本地的位置是：syncFloder/asd.txt
service.Download("asd.txt");//从远程节点下载asd.txt文件，会保存到本地位置：syncFloder/asd.txt
service.Delete("asd.txt");//删除远程节点的asd.txt文件，不会影响本地的asd.txt文件

service.Synchronize();//执行上面设定的那三个任务

service.Shutdown();//这个有Bug，还未修复（逃
```
### 依赖
**Flowchart库** 自制轮子，见另一个项目 
