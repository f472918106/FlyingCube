using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace FlyingCube.Assist
{
    class BaiduImageInterface
    {
        string apiUrl = "https://image.baidu.com/search/acjson?ipn=rj&tn=resultjson_com&pn=str(1)&word=";
        public string GetResponseImageUrl(string keyword)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl + keyword);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            int startIndex = json.IndexOf("\"objURL\":");
            int endIndex = json.IndexOf("\"fromURL\":");
            if (startIndex != -1 && endIndex != -1)
            {
                json = json.Substring(startIndex + 10, endIndex - startIndex - 16);
                json = ImageRealUrlUncomplie(json);
            }
            else
            {
                json = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1534059311603&di=ca19bfc4fba4c1a2a702f6b83553444e&imgtype=0&src=http%3A%2F%2Fimg.zcool.cn%2Fcommunity%2F0119af5652db686ac7251c941ddd4d.png";
            }
            stream.Close();
            reader.Close();
            return json;
        }

        public static string ImageRealUrlUncomplie(string str)
        {
            string[] c = { "_z2C$q", "_z&e3B", "AzdH3F" };
            Dictionary<String, String> d = new Dictionary<string, string>();
            d.Add("w", "a"); d.Add("k", "b"); d.Add("v", "c"); d.Add("1", "d"); d.Add("j", "e"); d.Add("u", "f"); d.Add("2", "g");
            d.Add("i", "h"); d.Add("t", "i"); d.Add("3", "j"); d.Add("h", "k"); d.Add("s", "l"); d.Add("4", "m"); d.Add("g", "n");
            d.Add("5", "o"); d.Add("r", "p"); d.Add("q", "q"); d.Add("6", "r"); d.Add("f", "s"); d.Add("p", "t"); d.Add("7", "u");
            d.Add("e", "v"); d.Add("o", "w"); d.Add("8", "1"); d.Add("d", "2"); d.Add("n", "3"); d.Add("9", "4"); d.Add("c", "5");
            d.Add("m", "6"); d.Add("0", "7"); d.Add("b", "8"); d.Add("l", "9"); d.Add("a", "0"); d.Add("_z2C$q", ":"); d.Add("_z&e3B", "."); d.Add("AzdH3F", "/");
            if (!(str != null) || str.Contains("http"))
                return str;
            string j = str;
            foreach (string s in c)
            {
                j = j.Replace(s, d[s]);
            }
            string[] arr = SplitByLen(j, 1);
            for (int i = 0; i < arr.Length; i++)
            {
                try
                {
                    if (Regex.IsMatch(d[arr[i]], @"^[a-w\d]+$"))
                    {
                        arr[i] = d[arr[i]];
                    }
                }
                catch     //不匹配的不做处理:   ".   :   /"
                {
                }
            }
            string url = string.Join("", arr);
            return url;
        }

        //字符串按长度截图
        private static string[] SplitByLen(string str, int separatorCharNum)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= separatorCharNum)
            {
                return new string[] { str };
            }
            string tempStr = str;
            List<string> strList = new List<string>();
            int iMax = Convert.ToInt32(Math.Ceiling(str.Length / (separatorCharNum * 1.0)));//获取循环次数    
            for (int i = 1; i <= iMax; i++)
            {
                string currMsg = tempStr.Substring(0, tempStr.Length > separatorCharNum ? separatorCharNum : tempStr.Length);
                strList.Add(currMsg);
                if (tempStr.Length > separatorCharNum)
                {
                    tempStr = tempStr.Substring(separatorCharNum, tempStr.Length - separatorCharNum);
                }
            }
            return strList.ToArray();
        }
    }
}
