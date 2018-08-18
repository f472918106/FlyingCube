using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyingCube.Assist;

namespace FlyingCube.Service
{
    public class AsyncTaskService
    {
        /// <summary>
        /// 作业队列
        /// </summary>
        public static Queue<Task> TaskQueue { set; get; }

        /// <summary>
        /// 最大异步作业数
        /// </summary>
        public static int TaskCount { set; get; }

        /// <summary>
        /// 异步作业超时时间
        /// </summary>
        public static int TaskTimeout { set; get; }

        public AsyncEventService EventService = null;

        /// <summary>
        /// AsyncTaskService构造函数
        /// </summary>
        /// <param name="count">最大异步作业数</param>
        /// <param name="timeout">作业超时时间(单位毫秒)</param>
        public AsyncTaskService(int count,int timeout)
        {
            TaskQueue = new Queue<Task>();
            TaskCount = count;
            TaskTimeout = timeout;
            EventService = new AsyncEventService(GetDbFilePath()+"logger.sqlite");
        }

        /// <summary>
        /// 获取数据库文件路径
        /// </summary>
        /// <returns></returns>
        public static string GetDbFilePath()
        {
            string topPath = "";
            string[] rootPath = Environment.CurrentDirectory.Split(new char[1] { '\\' });
            for (int i = 0; i < rootPath.Length - 2; i++)
            {
                topPath += rootPath[i];
                topPath += "\\";
            }
            return topPath + "DB\\";
        }

        /// <summary>
        /// 为请求创建新的作业并加入作业队列
        /// </summary>
        /// <param name="request">请求字符串</param>
        /// <returns></returns>
        public static async Task TaskEnqueue(string request)
        {
            if (TaskQueue.Count == TaskCount)
            {
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 作业队列已满,正在等待作业队列中作业完成...\n";
                Task.WaitAll(TaskQueue.ToArray());
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 作业队列中作业均已完成. \n";
                TaskQueue.Clear();
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 作业队列已清空.\n";
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 正在为当前请求创建作业...\n";
                Task task = new Task(async () => await AsyncEventService.EventHandler(request));
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 作业创建完成. \n";
                TaskQueue.Enqueue(task);
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 已加入作业队列. \n";
                task.Start();
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 当前作业开始执行... \n";
                AsyncHttpService.MsgQueue.Dequeue();
            }
            else
            {
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 正在为当前请求创建作业...\n";
                Task task = new Task(async () => await AsyncEventService.EventHandler(request));
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 作业创建完成. \n";
                TaskQueue.Enqueue(task);
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 已加入作业队列. \n";
                task.Start();
                AsyncHttpService.current.richTextBox_MsgQueue.Text += "[" + DateTime.Now + "] 当前作业开始执行... \n";
                AsyncHttpService.MsgQueue.Dequeue();
            }
        }
    }
}
