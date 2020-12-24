using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using WordCloudSharp;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace FinalJob
{
    class Program
    {
        enum REFER_TYPE { YEAR_RANGE, QUARTER_RANGE, MONTH_RANGE };
        static SortedDictionary<int, SortedDictionary<string, int>> dic = new SortedDictionary<int, SortedDictionary<string, int>>();
        static HashSet<string> stop_words = new HashSet<string>();
        static char[] dchar = new char[] { ' ', ',', '.', '!', '|', '(', ')', '?', ':', ';', '\\', '/', '[', ']' ,'@','©',
            '+','-','/','<','>','=','%'};
        static StreamReader data = null, stp = null;
        static Dictionary<string, int> monthToInt = new Dictionary<string, int>();
        static SortedDictionary<int, int> paperCount = new SortedDictionary<int, int>();

        static void createMonthMap()
        {
            monthToInt.Add("Jan", 1);
            monthToInt.Add("Feb", 2);
            monthToInt.Add("Mar", 3);
            monthToInt.Add("Apr", 4);
            monthToInt.Add("May", 5);
            monthToInt.Add("Jun", 6);
            monthToInt.Add("Jul", 7);
            monthToInt.Add("Aug", 8);
            monthToInt.Add("Sep", 9);
            monthToInt.Add("Oct", 10);
            monthToInt.Add("Nov", 11);
            monthToInt.Add("Dec", 12);
        }

        static void readOnePaper()
        {
            string linestr;
            do
            {
                linestr = data.ReadLine();
            } while (linestr == "");
            int year = 0, month = 0;
            string[] strs = linestr.Split(dchar);
            for (int i = 0; i < strs.Length; i++)
            {
                if (Regex.IsMatch(strs[i], @"^\d{4}$") && int.Parse(strs[i]) >= 2000 && int.Parse(strs[i]) <= 2020)
                {
                    year = int.Parse(strs[i]);
                }
                else if (monthToInt.ContainsKey(strs[i]))
                {
                    month = monthToInt[strs[i]];
                }
            }
            int timeKey = year * 100 + month;
            if(!paperCount.ContainsKey(timeKey))
			{
                paperCount.Add(timeKey, 0);
			}
            paperCount[timeKey] += 1;
            if (!dic.ContainsKey(timeKey))
            {
                dic.Add(timeKey, new SortedDictionary<string, int>());
            }
            while (!data.EndOfStream)
            {
                linestr = data.ReadLine();
                strs = linestr.Split(dchar);
                if (strs[0] == "DOI")
                {
                    continue;
                }
                else if (strs[0] == "PMID")
                {
                    return;
                }
                foreach (string cur in strs)
                {
                    string tmp = cur.ToLower();
                    if (!stop_words.Contains(tmp) && !Regex.IsMatch(tmp, @"^\d+[-]?\d*$"))
                    {
                        if (dic[timeKey].ContainsKey(tmp))
                        {
                            dic[timeKey][tmp] += 1;
                        }
                        else
                        {
                            dic[timeKey].Add(tmp, 1);
                        }
                    }
                }
            }
        }

        static void initWordCloudData()
        {
            dic.Clear();
            stop_words.Clear();
            createMonthMap();
            try
            {
                data = new StreamReader("./../../data/data_for_leukemia.txt");
                stp = new StreamReader("./../../stop_words/stop_words.txt");
                while(!stp.EndOfStream)
				{
                    stop_words.Add(stp.ReadLine());
				}
                while (!data.EndOfStream)
                {
                    readOnePaper();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if (data != null)
                {
                    data.Close();
                }
                if (stp != null)
                {
                    stp.Close();
                }
            }
        }

        static void drawWordCloud(SortedDictionary<string,int> freqData)
		{
            Console.WriteLine("正在生成词云...");
            List<string> keys = new List<string>(freqData.Keys);
            List<int> vals = new List<int>(freqData.Values);
            List<int> ord = new List<int>();
            for(int i=0;i<keys.Count;i++)
			{
                ord.Add(i);
			}
            ord.Sort((a, b) => vals[b] - vals[a]);
            List<string> words = new List<string>();
            List<int> freqs = new List<int>();
            for(int i=0;i<ord.Count;i++)
			{
                words.Add(keys[ord[i]]);
                freqs.Add(vals[ord[i]]);
			}
            WordCloud wc = new WordCloud(800, 600);
            Image img = wc.Draw(words.GetRange(0, 300), freqs.GetRange(0, 300));
            Form f = new Form();
            f.BackgroundImage = img;
            f.SetBounds(0, 0, 800, 600);
            f.BackgroundImageLayout = ImageLayout.Stretch;
            f.ShowDialog();
            Console.WriteLine("词云已关闭");
		}

        static void createYearWordCloud()
        {
            int year=0;
            Console.WriteLine("请输入年份：(年份应位于2010~2020之间）");
            string str = Console.ReadLine();
            while (!Regex.IsMatch(str, @"^\d{4}$") || int.Parse(str) < 2010 || int.Parse(str) >2020)
			{
                Console.WriteLine("年份输入不合规范！\n");
                Console.WriteLine("请重新输入年份：(年份应位于2010~2020之间）");
                str = Console.ReadLine();
			}
            year = int.Parse(str);
            int timeKey = year * 100;
            SortedDictionary<string, int> sd = new SortedDictionary<string, int>();
            for(int i=timeKey+1;i<=timeKey+12;i++)
			{
                if(!dic.ContainsKey(i))
				{
                    continue;
				}
                foreach(var cur in dic[i])
				{
                    if(sd.ContainsKey(cur.Key))
					{
                        sd[cur.Key] += cur.Value;
					}
                    else
					{
                        sd.Add(cur.Key, cur.Value);
					}
				}
			}
            drawWordCloud(sd);
		}

        static void createQuarterWordCloud()
		{
            int year = 0;
            Console.WriteLine("请输入年份：(年份应位于2010~2020之间）");
            string str = Console.ReadLine();
            while (!Regex.IsMatch(str, @"^\d{4}$") || int.Parse(str) < 2010 || int.Parse(str) > 2020)
            {
                Console.WriteLine("年份输入不合规范！");
                Console.WriteLine("请重新输入年份：(年份应位于2010~2020之间）");
                str = Console.ReadLine();
            }
            year = int.Parse(str);

            int quarter = 0;
            Console.WriteLine("请输入季度：（季度应位于1~4之间）");
            str = Console.ReadLine();
            while(!Regex.IsMatch(str,@"^\d{1}$") || int.Parse(str)<1 || int.Parse(str)>4)
			{
                Console.WriteLine("季度输入不合规范");
                Console.WriteLine("请重新输入季度：（季度应位于1~4之间）");
                str = Console.ReadLine();
			}
            quarter = int.Parse(str);

            int timeKey = year * 100;
            SortedDictionary<string, int> sd = new SortedDictionary<string, int>();
            for (int i = timeKey + 1 + 3 * (quarter - 1); i <= timeKey + 3 * quarter; i++)
            {
                if(!dic.ContainsKey(i))
				{
                    continue;
				}
                foreach (var cur in dic[i])
                {
                    if (sd.ContainsKey(cur.Key))
                    {
                        sd[cur.Key] += cur.Value;
                    }
                    else
                    {
                        sd.Add(cur.Key, cur.Value);
                    }
                }
            }
            drawWordCloud(sd);
        }

        static void createMonthWordCloud()
		{
            int year = 0;
            Console.WriteLine("请输入年份：(年份应位于2010~2020之间）");
            string str = Console.ReadLine();
            while (!Regex.IsMatch(str, @"^\d{4}$") || int.Parse(str) < 2010 || int.Parse(str) > 2020)
            {
                Console.WriteLine("年份输入不合规范！\n");
                Console.WriteLine("请重新输入年份：(年份应位于2010~2020之间）");
                str = Console.ReadLine();
            }
            year = int.Parse(str);


            int month = 0;
            Console.WriteLine("请输入月份：（月份应位于1~12之间）");
            str = Console.ReadLine();
            while(!Regex.IsMatch(str,@"^\d{1,2}$") || int.Parse(str)<1 || int.Parse(str)>12)
			{
                Console.WriteLine("月份不合规范！\n");
                Console.WriteLine("请重新输入月份：（月份应位于1~12之间）");
                str = Console.ReadLine();
			}
            month = int.Parse(str);

            int timeKey = year * 100 + month;
            SortedDictionary<string, int> sd = new SortedDictionary<string, int>();
            if (dic.ContainsKey(timeKey))
            {
                foreach (var cur in dic[timeKey])
                {
                    if (sd.ContainsKey(cur.Key))
                    {
                        sd[cur.Key] += cur.Value;
                    }
                    else
                    {
                        sd.Add(cur.Key, cur.Value);
                    }
                }
            }
            drawWordCloud(sd);
        }

        static void createWordCloud(REFER_TYPE type)
		{
            if(type==REFER_TYPE.YEAR_RANGE)
			{
                createYearWordCloud();
			}
            else if(type==REFER_TYPE.MONTH_RANGE)
			{
                createMonthWordCloud();
			}
            else if(type==REFER_TYPE.QUARTER_RANGE)
			{
                createQuarterWordCloud();
			}
		}

        static void createWordCloud()
		{
            Console.WriteLine("请选择生成词云涵盖的时间范围（输入1或2或3）：（1）按年分生成 （2）按季度生成 （3）按月生成");
            int type;
            string str = Console.ReadLine();
            while(!Regex.IsMatch(str,@"^\d{1}$") || int.Parse(str)<1 || int.Parse(str)>3)
			{
                Console.WriteLine("范围选择不合规范");
                Console.WriteLine("请重新选择词云要涵盖的范围（输入1或2或3）：（1）按年份生成 （2）按季度生成 （3）按月生成");
                str = Console.ReadLine();
			}
            type = int.Parse(str);
            Console.WriteLine();
            if(type==1)
			{
                createWordCloud(REFER_TYPE.YEAR_RANGE);
			}
            else if(type==2)
			{
                createWordCloud(REFER_TYPE.QUARTER_RANGE);
			}
            else if(type==3)
			{
                createWordCloud(REFER_TYPE.MONTH_RANGE);
			}
		}

        static void countPaperByYear()
		{
            int year = 0;
            Console.WriteLine("请输入年份：(年份应位于2010~2020之间）");
            string str = Console.ReadLine();
            while (!Regex.IsMatch(str, @"^\d{4}$") || int.Parse(str) < 2010 || int.Parse(str) > 2020)
            {
                Console.WriteLine("年份输入不合规范！\n");
                Console.WriteLine("请重新输入年份：(年份应位于2010~2020之间）");
                str = Console.ReadLine();
            }
            year = int.Parse(str);
            int timeKey = year * 100;
            int res = 0;
            for(int i=timeKey+1;i<=timeKey+12;i++)
			{
                if(!paperCount.ContainsKey(i))
				{
                    continue;
				}
                res += paperCount[i];
			}
            Console.WriteLine("在" + year + "年有" + res + "篇关于白血病的文献被发表\n");
            Console.WriteLine();
        }

        static void countPaperByQuarter()
		{
            int year = 0;
            Console.WriteLine("请输入年份：(年份应位于2010~2020之间）");
            string str = Console.ReadLine();
            while (!Regex.IsMatch(str, @"^\d{4}$") || int.Parse(str) < 2010 || int.Parse(str) > 2020)
            {
                Console.WriteLine("年份输入不合规范！");
                Console.WriteLine("请重新输入年份：(年份应位于2010~2020之间）");
                str = Console.ReadLine();
            }
            year = int.Parse(str);

            int quarter = 0;
            Console.WriteLine("请输入季度：（季度应位于1~4之间）");
            str = Console.ReadLine();
            while (!Regex.IsMatch(str, @"^\d{1}$") || int.Parse(str) < 1 || int.Parse(str) > 4)
            {
                Console.WriteLine("季度输入不合规范");
                Console.WriteLine("请重新输入季度：（季度应位于1~4之间）");
                str = Console.ReadLine();
            }
            quarter = int.Parse(str);

            int timeKey = year * 100;
            int res = 0;
            for(int i=timeKey+3*(quarter-1)+1;i<=timeKey+3*quarter;i++)
			{
                if(!paperCount.ContainsKey(i))
				{
                    continue;
				}
                res += paperCount[i];
			}

            Console.WriteLine("在" + year + "年第" + quarter + "季度有" + res + "篇关于白血病的文献被发表");
            Console.WriteLine();
        }

        static void countPaperByMonth()
		{
            int year = 0;
            Console.WriteLine("请输入年份：(年份应位于2010~2020之间）");
            string str = Console.ReadLine();
            while (!Regex.IsMatch(str, @"^\d{4}$") || int.Parse(str) < 2010 || int.Parse(str) > 2020)
            {
                Console.WriteLine("年份输入不合规范！\n");
                Console.WriteLine("请重新输入年份：(年份应位于2010~2020之间）");
                str = Console.ReadLine();
            }
            year = int.Parse(str);


            int month = 0;
            Console.WriteLine("请输入月份：（月份应位于1~12之间）");
            str = Console.ReadLine();
            while (!Regex.IsMatch(str, @"^\d{1,2}$") || int.Parse(str) < 1 || int.Parse(str) > 12)
            {
                Console.WriteLine("月份不合规范！\n");
                Console.WriteLine("请重新输入月份：（月份应位于1~12之间）");
                str = Console.ReadLine();
            }
            month = int.Parse(str);

            int timeKey = year * 100 + month;
            int res = 0;
            if(paperCount.ContainsKey(timeKey))
			{
                res += paperCount[timeKey];
			}
            Console.WriteLine("在" + year + "年" + month + "月有" + res + "篇关于白血病的文献被发表");
            Console.WriteLine();
        }

        static void countPaper()
		{
            Console.WriteLine("请选择待查询时间段的范围(输入1或2或3）：（1）按年查询 （2）按季度查询 （3）按月查询");
            int type;
            string str = Console.ReadLine();
            while (!Regex.IsMatch(str, @"^\d{1}$") || int.Parse(str) < 1 || int.Parse(str) > 3)
            {
                Console.WriteLine("范围选择不合规范");
                Console.WriteLine("请重新选择想要查询的时间范围（输入1或2或3）：（1）按年份生成 （2）按季度生成 （3）按月生成");
                str = Console.ReadLine();
            }
            type = int.Parse(str);
            Console.WriteLine();
            if(type==1)
			{
                countPaperByYear();
			}
            else if(type==2)
			{
                countPaperByQuarter();
			}
            else if(type==3)
			{
                countPaperByMonth();
			}
        }

        static void printPaperCountPerYear()
		{
            for(int i=2010;i<=2020;i++)
			{
                int res = 0;
                for(int j=1;j<=12;j++)
				{
                    int timeKey = i * 100 + j;
                    if (paperCount.ContainsKey(timeKey))
                    {
                        res += paperCount[timeKey];
                    }
				}
                Console.WriteLine(i+"年共有"+res+"篇有关白血病的论文被发表");
			}
		}

        static void Main(string[] args)
        {
            Console.WriteLine("此程序负责统计PubMed数据库中关于白血病（血癌）近10年的论文数据");
            Console.WriteLine("程序初期需要导入数据，请等待一小段时间...");
            Console.WriteLine();
            initWordCloudData();
            while (true)
            {
                Console.WriteLine("请选择要进行的操作(输入1或2或3）:(1)生成词云 （2）查询时间段内文献数量 （3）退出");
                int op;
                string str = Console.ReadLine();
                while (!Regex.IsMatch(str, @"^\d{1}$") || int.Parse(str) < 1 || int.Parse(str) > 3)
                {
                    Console.WriteLine("操作输入不合规范");
                    Console.WriteLine("请重新选择要进行的操作（输入1或2或3）：（1）生成词云 （2）查询时间段内文献数量 （3）退出");
                    str = Console.ReadLine();
                }
                op = int.Parse(str);
                if(op==1)
				{
                    createWordCloud();
                    Console.WriteLine();
				}
                else if(op==2)
				{
                    countPaper();
                    Console.WriteLine();
				}
                else if(op==3)
				{
                    Console.WriteLine("bye");
                    break;
				}
            }
            return;
        }

    }
}