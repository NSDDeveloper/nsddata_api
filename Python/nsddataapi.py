import json
import requests
import xlsxwriter

def create_excel():
    """Загрузка данных и формирование Excel-файла"""
    ApiUrl = "https://nsddata.ru/api/get/securities"
    ApiKey = "<INSERT_KEY>"
    FileName = "coupons.xlsx"
    Payments = ["INTR", "INTR/DD", "MCAL", "MCAL/BN", "BPUT", "REDM", "REDM/BN", "REDM/PS", "REDM/UN", "PRII",
                "DRAW", "PRED", "DVCA", "DVOP", "DVSE", "DRIP", "CAPG", "DFLT", "CREV", "SHPR", "LIQU"]

    print("Загрузка данных...")

    api_filter = {"isin":{"$in":[ "RU000A0GKZJ8", "RU000A0JPKW6", "RU0001707002", "RU0002867854"]}}
    raw_api_filter = json.dumps(api_filter, separators=(',', ':'))

    request_url = "%s?filter=%s&apikey=%s" % (ApiUrl, raw_api_filter, ApiKey)
    json_data = requests.get(request_url).json()

    print("Выбор данных...")

    data = []
    for bond in json_data:
        if 'corp_actions' not in bond or len(bond['corp_actions']) == 0:
            continue
        corp_actions = bond['corp_actions']
        for ca in corp_actions:
            if ca["corp_action_type"]["code"] not in Payments:
                continue
            if ca["corp_action_type"]["name_en"] == "Interest Payment":
                data.append( (bond["isin"], bond["issuer"]["name_full"],
                ca["corp_action_type"]["name"], ca["action_date_plan"], ca["coupon"]["size"]) )
            elif ca["corp_action_type"]["name_en"] == "Principal repayment":
                data.append( (bond["isin"], bond["issuer"]["name_full"],
                ca["corp_action_type"]["name"], ca["action_date_plan"], ca["repayment"]["size_cur"]) )

    print("Формирование Excel...")

    workbook = xlsxwriter.Workbook('coupons.xlsx')
    worksheet = workbook.add_worksheet()

    columns = ["ISIN", "Эмитент", "Корпоративное действие", "Дата", "Размер"]
    for i, col in enumerate(columns):
        worksheet.write(0, i, col)

    for i, row in enumerate(data):
        for j, col in enumerate(row):
            worksheet.write(i + 1, j, col)

    workbook.close()

    print("Готово!")

if __name__ == "__main__":
    create_excel()