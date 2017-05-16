using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ClosedXML.Excel;

namespace ApiSample
{
    class Program
    {
        public const string ApiUrl = "https://nsddata.ru/api/get/securities";
        private const string ApiKey = "<INSERT_KEY>";
        public const string FileName = "coupons.xlsx";

        static void Main(string[] args)
        {
            Console.WriteLine("Загрузка данных...");
            var wc = new WebClient() { Encoding = Encoding.UTF8, };

            var apiFilter = new JObject {
                {
                    "isin", new JObject
                    {
                        {"$in", new JArray { "RU000A0GKZJ8", "RU000A0JPKW6", "RU0001707002", "RU0002867854"}}
                    }
                }
            };

            var requestUrl = $"{ApiUrl}?filter={apiFilter.ToString(Formatting.None)}&apikey={ApiKey}";
            var json = wc.DownloadString(requestUrl);

            var bonds = JArray.Parse(json);
            Console.WriteLine("Формирование Excel...");
            GenerateExcel(bonds);
            Console.WriteLine("Готово!");
        }

        private static void GenerateExcel(JArray input)
        {
            var workbook = new XLWorkbook();
            workbook.Use1904DateSystem = true;
            var ws = workbook.Worksheets.Add("Coupons info");

            ws.Column(1).Width = 16;
            ws.Column(2).Width = 65;
            ws.Column(3).Width = 30;
            ws.Column(4).Width = 16;
            ws.Column(5).Width = 10;

            var columns = new[] { "ISIN", "Эмитент", "Корпоративное действие", "Дата", "Размер" };
            foreach (var j in Enumerable.Range(0, columns.Length))
            {
                ws.Cell(1, j + 1).Value = columns[j];
                ws.Cell(1, j + 1).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent6);
            }

            var data = new List<List<JToken>>();
            foreach (var bond in input)
            {
                var corpActions = bond["bond"]?["income_payments"].ToArray();
                if (corpActions == null || corpActions.Length == 0)
                    continue;

                foreach (var c in corpActions)
                    try
                    {
                        if (c["corp_action_type"]["name_en"].ToString() == "Interest Payment")
                        {
                            data.Add(new List<JToken>() { bond["isin"], bond["issuer"]["name_full"],
                            c["corp_action_type"]["name"], c["action_date_plan"], c["coupon"]["size"] });
                        }
                        else if (c["corp_action_type"]["name_en"].ToString() == "Principal repayment")
                        {
                            data.Add(new List<JToken>() { bond["isin"], bond["issuer"]["name_full"],
                            c["corp_action_type"]["name"], c["action_date_plan"], c["repayment"]["size_cur"] });
                        }
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        continue;
                    }
            }

            for (int i = 0; i < data.Count; i++)
                for (int j = 0; j < columns.Length; j++)
                {
                    ws.Cell(i + 2, j + 1).SetValue<string>(Convert.ToString(data[i][j]));
                    ws.Cell(i + 2, j + 1).Style.Fill.BackgroundColor = i % 2 == 0 ? XLColor.FromTheme(XLThemeColor.Accent6, 0.5) : XLColor.FromTheme(XLThemeColor.Accent6, 0.8);
                }

            ws.RangeUsed().SetAutoFilter();

            workbook.SaveAs(FileName);
            System.Diagnostics.Process.Start(FileName);
        }
    }
}
