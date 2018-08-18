using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Net;
using FlyingCube.Assist;

namespace FlyingCube.Service
{
    public class AsyncHttpService
    {
        public static MainForm current = null;
        /// <summary>
        /// 消息队列
        /// </summary>
        public static Queue<string> MsgQueue { set; get; }
        
        /// <summary>
        /// Http协议侦听器
        /// </summary>
        private HttpListener listener { set; get; }

        /// <summary>
        /// 监听地址
        /// </summary>
        public static string ServiceUrl;

        /// <summary>
        /// 最大异步作业数
        /// </summary>
        public static int TaskCount { set; get; }
        /// <summary>
        /// 异步作业超时时间
        /// </summary>
        public static int TaskTimeout { set; get; }

        /// <summary>
        /// AsyncHttpService构造函数
        /// </summary>
        /// <param name="url">监听地址</param>
        /// <param name="count">最大异步作业数</param>
        /// <param name="timeout">异步作业超时时间</param>
        /// <param name="form">窗体MainForm</param>
        public AsyncHttpService(string url,int count,int timeout,MainForm form)
        {
            ServiceUrl = url;
            current = form;
            TaskCount = count;
            TaskTimeout = timeout;
            
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 正在尝试初始化AsyncHttpService...\n";
            listener = new HttpListener();
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] AsyncHttpService初始化完成. \n";
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 正在尝试初始化MsgQueue...\n";
            MsgQueue = new Queue<string>();
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] MsgQueue初始化完成. \n";
            listener.Prefixes.Add(url);
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 正在尝试初始化AsyncTaskService... \n";
            AsyncTaskService taskService = new AsyncTaskService(TaskCount,TaskTimeout);
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] AsyncTaskService初始化完成. \n";

        }

        /// <summary>
        /// 开始监听,将消息加入消息队列
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            listener.Start();
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 开始监听"+ServiceUrl+"\n";
            while (true)
            {
                current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 监听中... \n";
                var context = await listener.GetContextAsync();
                current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 接收到一条新的请求... \n";
                await ReportContext(context);
                await ReturnResponse(context);
            }
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            listener.Stop();
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        /// <summary>
        /// 异步上报HttpListentContext实体
        /// </summary>
        /// <param name="context">HttpListentContext实体</param>
        /// <returns></returns>
        private async Task ReportContext(HttpListenerContext context)
        {
            StreamReader reader = new StreamReader(context.Request.InputStream);
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 正在读取请求... \n";
            string request = await reader.ReadToEndAsync();
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 请求读取完成,内容如下： \n"+request+"\n";
            reader.Close();
            MsgQueue.Enqueue(request);
            current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 已加入消息队列. \n";
            await AsyncTaskService.TaskEnqueue(request);
        }

        /// <summary>
        /// 异步提交HttpListenerResponse响应
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ReturnResponse(HttpListenerContext context)
        {
            Stream writer = context.Response.OutputStream;
            byte[] buffer = Encoding.UTF8.GetBytes("");
            await writer.WriteAsync(buffer, 0, buffer.Length);
            await writer.FlushAsync();
            writer.Close();
        }
    }
}
