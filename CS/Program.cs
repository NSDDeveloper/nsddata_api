using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using ClosedXML.Excel;

namespace ApiSample
{
    class Program
    {
        public const string ApiUrl = "http://0.0.0.0/api/get/securities?limit=50&skip=20150"; 
        public static HashSet<string> SetOfIsins = new HashSet<string>(new[] { "XS0342241295", "RU000A0JS9F8" });
        public const string FileName = "coupons.xlsx";

        static void Main(string[] args)
        {
            Console.WriteLine("Загрузка данных...");
            var wc = new WebClient() { Encoding = Encoding.UTF8, };
            var json = wc.DownloadString(ApiUrl);

            var bonds = JArray.Parse(json);
            var filteredBonds = bonds.Where(b => SetOfIsins.Contains((string)b["isin"])).ToList();

            Console.WriteLine("Формирование Excel...");
            GenerateExcel(filteredBonds);
            Console.WriteLine("Готово!");
        }

        private static void GenerateExcel(List<JToken> input)
        {
            var workbook = new XLWorkbook();
            workbook.Use1904DateSystem = true;
            var ws = workbook.Worksheets.Add("Coupons info");

            ws.Column(1).Width = 16;
            ws.Column(2).Width = 70;
            ws.Column(3).Width = 50;
            ws.Column(4).Width = 35;
            ws.Column(5).Width = 16;

            var columns = new[] { "ISIN", "Название", "Эмитент", "Корпоративное действие", "Дата" };
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
                        data.Add(new List<JToken>() { bond["isin"], bond["name_full"], bond["issuer"]["name_full"],
                            c["corp_action_type"]["name"], c["action_date_plan"] });
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
