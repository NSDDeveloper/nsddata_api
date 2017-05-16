using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiSample
{
    internal class Program
    {
        private const string ApiUrl =
            "https://nsddata.ru/api/get/corpactions";

        private const string ApiKey = "<INSERT_KEY>";

        private static void Main(string[] args)
        {
            var dateFrom = new DateTime(2017, 05, 01);
            var dateTo = new DateTime(2017, 05, 31);

            var apiFilter =
                new JObject
                {
                    {
                        "$and", new JArray
                        {
                            new JObject
                            {
                                {"securities.listing.tks.code", "MICEX_FOND"}
                            },
                            new JObject
                            {
                                {
                                    "corp_action_type.code", new JObject
                                    {
                                        {"$in", new JArray {"MEET", "XMET", "DVCA"}}
                                    }
                                }
                            },
                            new JObject
                            {
                                {
                                    "action_date_plan", new JObject
                                    {
                                        {"$gt", dateFrom.ToString("yyyy-MM-dd")}
                                    }
                                }
                            },
                            new JObject
                            {
                                {
                                    "action_date_plan", new JObject
                                    {
                                        {"$lt", dateTo.ToString("yyyy-MM-dd")}
                                    }
                                }
                            }
                        }
                    }
                };


            Console.WriteLine("Загрузка данных...");
            var wc = new WebClient {Encoding = Encoding.UTF8};

            var requestUrl = ApiUrl
                             + $"?filter={apiFilter.ToString(Formatting.None)}"
                             + $"&apikey={ApiKey}";


            var json = wc.DownloadString(requestUrl);

            var corpActions = JArray.Parse(json);

            Console.WriteLine($"Загружено {corpActions.Count} документов ...");

            Console.WriteLine("Формирование Excel...");

            var meetingsTable = PrepareMeetingsDataTable("Meetings");
            var divTable = PrepareDivDataTable("Dividends");

            foreach (var corpAction in corpActions.OrderBy(ca => ca["action_date_plan"]))
            {
                foreach (var security in corpAction["securities"]
                    .Where(s => s["listing"] != null && s["listing"].Any(l => l["tks"]["code"].ToString() == "MICEX_FOND"))
                    .ToList()
                    )
                {
                    if (corpAction["corp_action_type"]["code"].ToString() == "DVCA")
                    {
                        AddDividendRow(divTable, security, corpAction);
                    }
                    else
                    {
                        AddMeetingRow(meetingsTable, security, corpAction);
                    }
                }
            }
            var wb = new XLWorkbook();

            wb.Worksheets.Add(meetingsTable);
            wb.Worksheets.Add(divTable);

            var fileName = $"{dateFrom: dd.MM.yyyy}- {dateTo: dd.MM.yyyy}.xlsx";

            wb.SaveAs(fileName);
            Console.WriteLine("Готово.");


            Process.Start(fileName);
            Console.ReadKey();
        }

        private static DataTable PrepareMeetingsDataTable(string tableName)
        {
            var table = new DataTable {TableName = tableName};
            table.Columns.Add("TIKER", typeof (string));
            table.Columns.Add("SECURITY", typeof (string));
            table.Columns.Add("ISIN", typeof (string));
            table.Columns.Add("DATE", typeof (DateTime));
            table.Columns.Add("RECORD DATE", typeof (DateTime));
            table.Columns.Add("AGM/EGM", typeof (string));
            table.Columns.Add("AGENDA", typeof(string));
            return table;

        }

        private static void AddMeetingRow(DataTable meetingsTable, JToken security, JToken corpAction)
        {
            meetingsTable.Rows.Add(
                //"TIKER"
                security["listing"].First(l => l["tks"]["code"].ToString() == "MICEX_FOND")["code"],
                //"SECURITY"
                security["name_full_en"],
                //"ISIN"
                security["isin"],
                //"DATE"
                corpAction["action_date_plan"],
                //"RECORD DATE"
                corpAction["record_date_plan"],
                //"AGM/EGM"
                corpAction["corp_action_type"]["code"].ToString() == "MEET" ? "AGM" : "EGM",
                //"AGENDA"
                corpAction["meeting"]?["agenda"]
                );
        }

        private static void AddDividendRow(DataTable divTable, JToken security, JToken corpAction)
        {
            divTable.Rows.Add(
                //"TIKER"
                security["listing"].First(l => l["tks"]["code"].ToString() == "MICEX_FOND")["code"],
                //"SECURITY"
                security["name_full_en"],
                //"ISIN"
                security["isin"],
                //"DATE"
                corpAction["action_date_plan"],
                //"RECORD DATE"
                corpAction["record_date_plan"],
                //"DIV RATE"
                corpAction["dividend"]?["values"]?
                    .FirstOrDefault(
                        v => v["instr_type"]["id"].ToString() == security["instr_type"]["id"].ToString() &&
                             v["share_category"]?["id"].ToString() ==
                             security["share"]?["category"]["id"].ToString())?["size"],
                //"DIV CUR"
                corpAction["dividend"]?["payment_currency"]?["code"]
                );
        }


        private static DataTable PrepareDivDataTable(string tableName)
        {
            var table = new DataTable {TableName = tableName};
            table.Columns.Add("TIKER", typeof (string));
            table.Columns.Add("SECURITY", typeof (string));
            table.Columns.Add("ISIN", typeof (string));
            table.Columns.Add("DATE", typeof (DateTime));
            table.Columns.Add("RECORD DATE", typeof (DateTime));
            table.Columns.Add("DIV RATE", typeof (decimal));
            table.Columns.Add("DIV CUR", typeof (string));

            return table;
        }
    }
}