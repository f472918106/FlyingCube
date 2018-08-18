using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyingCube.Assist;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Data.SQLite;
using System.Data;

namespace FlyingCube.Service
{
    public class AsyncEventService
    {
        private static string ApiUrl = @"http://localhost:5700/";
        private static SqlHelper helper = null;
        public AsyncEventService(string path)
        {
            helper = new SqlHelper(path);
        }

        public static async Task EventHandler(string request)
        {
            //将json转化为json对象
            JObject obj = JObject.Parse(request);
            string message = "";
            string user_id = "";
            string group_id = "";
            if (request.Contains("message"))
            {
                message = obj["message"].ToString();
            }
            if (request.Contains("user_id"))
            {
                user_id = obj["user_id"].ToString();
            }
            if (request.Contains("group_id"))
            {
                group_id = obj["group_id"].ToString();
            }
            if (obj["post_type"].ToString() == "message")
            {
                if (obj["message_type"].ToString() == "group")
                {
                    switch (message)
                    {
                        case "签到":
                            await SignedOn(user_id, group_id);
                            break;
                        case "查看日报表":
                            await DaylilyReturn(group_id);
                            break;
                        case "查看周报表":
                            await WeeklyReturn(group_id);
                            break;
                        case "萌新手册":
                            await WelocomeWord(group_id);
                            break;
                        case "查看菜单":
                            await Menu(group_id);
                            break;
                    }
                }
                else if(obj["message_type"].ToString() == "private")
                {
                    IDictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("user_id", HttpUtility.UrlEncode(user_id));
                    parameters.Add("message", HttpUtility.UrlEncode("服务器调试中，暂不提供私聊服务")); //编码问题待解决
                    parameters.Add("auto_escape", HttpUtility.UrlEncode("false"));
                    await WebRequestHelper.CreatePostHttpResponseAsync(ApiUrl + "send_private_msg?", parameters);
                }
            }
        }

        public static async Task Menu(string gid)
        {
            string info = "菜单功能如下：\n[CQ:emoji,id=127881]签到\n[CQ:emoji,id=127881]查看日报表\n[CQ:emoji,id=127881]查看周报表\n[CQ:emoji,id=127881]萌新手册";
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("group_id", HttpUtility.UrlEncode(gid));
            parameters.Add("message", HttpUtility.UrlEncode(info)); //编码问题待解决
            parameters.Add("auto_escape", HttpUtility.UrlEncode("false"));
            await WebRequestHelper.CreatePostHttpResponseAsync(ApiUrl + "send_group_msg?", parameters);
        }

        public static async Task WelocomeWord(string gid)
        {
            string Info = "[CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024]\n";
            Info += "[CQ:emoji,id=10024]入群有福利\n";
            Info += "[CQ:emoji,id=10024]每日坚持签到\n";
            Info += "[CQ:emoji,id=10024]群主每周发放补助哟\n";
            Info += "[CQ:emoji,id=10024]详情请移步-[群公告]\n";
            Info += "[CQ:emoji,id=10024]敲黑板(请注意:签到时,请于输入框内输入'签到'然后点击发送按钮!)\n";
            Info += "[CQ:emoji,id=10024]更多功能,发送'菜单'查看";
            Info += "[CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024][CQ:emoji,id=10024]\n";
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("group_id", HttpUtility.UrlEncode(gid));
            parameters.Add("message", HttpUtility.UrlEncode(Info)); //编码问题待解决
            parameters.Add("auto_escape", HttpUtility.UrlEncode("false"));
            await WebRequestHelper.CreatePostHttpResponseAsync(ApiUrl + "send_group_msg?", parameters);
        }

        public static async Task WeeklyReturn(string gid)
        {
            helper.Connect();
            string sqlStr = "select uid,count(uid) as times from signedon where date BETWEEN datetime(date(datetime('now',strftime('-%w day', 'now'))),'+1 second') and datetime(date(datetime('now',(6 - strftime('%w day','now'))||' day','1 day')),'-1 second')  group BY uid";
            DataTable dt = helper.Query(sqlStr, null);
            string info = "本周群员签到信息如下：\n";
            foreach (DataRow dr in dt.Rows)
            {
                info += "群员[CQ:at,qq=" + dr["uid"].ToString() + "] 签到" + dr["times"].ToString() + "次\n";
            }
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("group_id", HttpUtility.UrlEncode(gid));
            parameters.Add("message", HttpUtility.UrlEncode(info)); //编码问题待解决
            parameters.Add("auto_escape", HttpUtility.UrlEncode("false"));
            await WebRequestHelper.CreatePostHttpResponseAsync(ApiUrl + "send_group_msg?", parameters);
            helper.DisConnect();
        }

        public static async Task DaylilyReturn(string gid)
        {
            helper.Connect();
            string sqlStr = "select * from signedon where date='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            DataTable dt = helper.Query(sqlStr, null);
            string info = "今日[" + DateTime.Now.ToString("yyyy-MM-dd") + "]\n群员签到信息如下：\n";
            foreach (DataRow dr in dt.Rows)
            {
                info += "[CQ:emoji,id=11088]群员[" + dr["uid"].ToString() + "]\n";
            }
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("group_id", HttpUtility.UrlEncode(gid));
            parameters.Add("message", HttpUtility.UrlEncode(info)); //编码问题待解决
            parameters.Add("auto_escape", HttpUtility.UrlEncode("false"));
            await WebRequestHelper.CreatePostHttpResponseAsync(ApiUrl + "send_group_msg?", parameters);
            helper.DisConnect();
        }

        public static async Task SignedOn(string uid,string gid)
        {
                string response;
                helper.Connect();
                string sqlStr = "select * from signedon where uid='" + uid + "' and date='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
                if (helper.Query(sqlStr, null).Rows.Count == 0) {
                    string insertStr = "insert into signedon (uid,date,gid) values(@uid,@date,@gid)";
                    SQLiteParameter[] sqlparameters = new SQLiteParameter[]
                    {
                       new SQLiteParameter("uid",uid),
                       new SQLiteParameter("date",DateTime.Now.ToString("yyyy-MM-dd")),
                       new SQLiteParameter("gid",Guid.NewGuid().ToString())
                    };
                    helper.Save(insertStr, sqlparameters);
                    response = "[CQ:at,qq="+uid+"] [CQ:emoji,id=127773]今日签到成功!";
                }
                else
                {
                    response ="[CQ:at,qq="+uid+ "] [CQ:emoji,id=127770]今日你已签到!";
                }
                helper.DisConnect();
                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("group_id", HttpUtility.UrlEncode(gid));
                parameters.Add("message", HttpUtility.UrlEncode(response)); //编码问题待解决
                parameters.Add("auto_escape", HttpUtility.UrlEncode("false"));
                await WebRequestHelper.CreatePostHttpResponseAsync(ApiUrl + "send_group_msg?", parameters);
        }
    }
}
