using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.IO;

namespace FlyingCube.Assist
{
    public class SqlHelper
    {
        public SQLiteConnection conn;
        public string sqlFilePath { set; get; }

        /// <summary>
        /// SqlHelper构造函数
        /// </summary>
        /// <param name="rootPath">数据库文件路径</param>
        public SqlHelper(string rootPath)
        {
            sqlFilePath = rootPath;
            if (File.Exists(sqlFilePath) == true)
            {
                conn = new SQLiteConnection("Data Source=" + sqlFilePath + ";Version=3;");
                Connect();
            }
            else
            {
                try
                {
                    //Console.WriteLine("[" + DateTime.Now + "]" + "数据库文件不存在，正在尝试重构数据库...");
                    SQLiteConnection.CreateFile(sqlFilePath);
                    conn = new SQLiteConnection("Data Source=" + sqlFilePath + ";Version=3;");
                    Connect();
                    string sqlLoad = @"create table signedon (
                                 uid varchar(20) primary key,
                                 date datetime,
                                 gid varchar(50))";
                    SQLiteCommand cmd = new SQLiteCommand(sqlLoad, conn);
                    cmd.ExecuteNonQuery();
                    //Console.WriteLine("[" + DateTime.Now + "]" + "数据库重构完成...");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        //打开数据库连接
        public void Connect()
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }

        //关闭数据库连接
        public void DisConnect()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        //查询操作
        public DataTable Query(string sqlStr, SQLiteParameter[] parameters)
        {
            if (conn.State != ConnectionState.Open)
            {
                Connect();
            }
            SQLiteCommand cmd = new SQLiteCommand(sqlStr, conn);
            if (parameters != null)//判断一下parameters是否为空,不判断会出错
                cmd.Parameters.AddRange(parameters);
            SQLiteDataAdapter adp = new SQLiteDataAdapter(cmd);
            DataTable dt = new DataTable();
            adp.Fill(dt);
            return dt;
        }

        //存储操作
        public void Save(string sqlStr, SQLiteParameter[] parameters)
        {
            if (conn.State != ConnectionState.Open)
            {
                Connect();
            }
            SQLiteCommand cmd = new SQLiteCommand(sqlStr, conn);
            cmd.Parameters.AddRange(parameters);
            cmd.ExecuteNonQuery();
        }

        //删除操作
        public void Delete(string sqlStr)
        {
            if (conn.State != ConnectionState.Open)
            {
                Connect();
            }
            SQLiteCommand cmd = new SQLiteCommand(sqlStr, conn);
            cmd.ExecuteNonQuery();
        }
    }
}
