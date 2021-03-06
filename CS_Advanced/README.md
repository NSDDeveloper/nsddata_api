﻿# Инструкция по подключению к NSDDATA API с помощью приложения на C#

## Введение

В данном примере демонстрируется создание консольного приложения с помощью MS Visual Studio.

Приложение:
* подключается к API NSDDATA
* запрашивает данные о собраниях и дивидендах за заданный период времени по бумагам, торгуемым на фондовой секции МБ
* формирует Excel файл с полученными данными.

![screen1](Screenshots/excel_doc1.png?raw=true "Информация о КД")

## Подготовка проекта

Создаем новое консольное приложение в Visual Studio
File->New->Project Выбираем Console Application

Взаимодействие с API NSDDATA (параметры запроса и возвращаемый результат) осуществляется в формате JSON.
С форматом JSON можно работать как с обычным текстом, одно из его основных преимуществ - "человекочитаемость".

Пример возвращаемых данных API:

```json
{
    "id": 276872,
    "corp_action_type": {
        "id": 42,
        "code": "MEET",
        "name": "Годовое общее собрание акционеров",
        "name_en": "Annual General Meeting"
    },
    "state": {
        "state_mn": "N",
        "name": "Не состоялось"
    },
    "securities": [
        {
            "id": 10008964,
            "isin": "RU0009029540",
            "code_nsd": "SBER\/03",
            "name_full": "Акции обыкновенные ПАО Сбербанк",
            "name_full_en": "SBERBANK ORD SHS",
            "instr_type": {
                "id": 1,
                "name": "акции",
                "name_en": "shares"
            }
        }
    ]
}
```

Однако рекомендуем использовать для работы с форматом JSON специализированную библиотеку. 
Например [Newtonsoft Json.NET](http://www.newtonsoft.com/json)
Библиотека позволяет выполнять сериализацию (преобразование объектов .NET в формат JSON) и десериализовывать JSON документы в привычные структуры .NET (типизированные объекты, перечисления, массивы и т.д. )
Это позволит сильно сократить количество программного кода и позволит в полной мере использовать средства платформы .net для работы с данными, в том числе LINQ

Для подключения пакеты из NuGet репозитория, открываем [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console) и выполняем команду

```
PM> Install-Package Newtonsoft.Json
```

Так как в данном примере данные будут выгружаться в Excel, подключим пакет [ClosedXML](https://github.com/closedxml/closedxml)
```
PM> Install-Package ClosedXML
```

С его помощью двумя строчками кода можно создать Excel документ и помесить на лист данные.



В секцию using файла Program.cs добавляем пространства имен, которые нам понадобятся:

```cs
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
```

В них мы будем использовать следующие классы:

Пространство имен  | Класс | Сценарий использования
--- | --- | ---
System.Data  | DataTable  | Таблица, для сохранения результатов
System.Diagnostics  | Process  | Открыть файл в Excel
System.Linq  |   | Методы расширения для работы с коллекциями
System.Net  | WebClient  | Выполнения HTTP запросов
System.Text  | Encoding  | Указать кодировку документа
ClosedXML.Excel  | XLWorkbook  | Формирования файла в формате Excel
Newtonsoft.Json  |   | Сериализация/Десериализация JSON документов
Newtonsoft.Json.Linq  |   | LINQ to JSON


## Подключение к API NSDDATA

Для получения данных в API NSDDATA используется методы GET или POST протокола HTTP. 

Формат запроса с использованием GET:

https://nsddata.ru/api/get/<продукт>?
apikey=<API-ключ>
[&skip=<количество документов>]
[&limit=<количество документов>]
[&filter=<фильтр>]
[&include=<список аттрибутов>]
[&exclude=<список аттрибутов>]


Параметр  | Обязательный | Формат | Описание | Пример
--- | --- | --- | --- | ---
продукт | Да  | Строка.<br>Возможные значения: securities, companies, corpactions |  Все 3 продукта содержат одинаковый набор данных, различия в корневой сущности.  Метод getSecurities возвращает список ценных бумаг. Каждый документ содержит информацию о ценной бумаге, а так же связанных с ней сущностях. В том числе организации и КД. Аналогично, методы getCompanies и getCorpActions содержат списки Организаций и корпоративных действий соответственно, с полной информацией и связанных с ними объектах. | https://nsddata.ru/api/get/securities
limit | Нет  | Число | Позволяет ограничить количество документов в ответе | &limit=10 Выдать не более 10 документов, удовлетворяющих условию
skip | Нет  | Число | Позволяет пропустить n документов в ответе. Совместно с limit можно использовать для реализации постраничной загрузки | &skip=1000 пропустить первые 1000 документов
filter| Нет  | JSON | JSON документ (массив JSON документов), описывающий условие(я), которым должны удовлетворять запрашиваемые данные| &filter={"isin":"RU0009029540"} <br><br>&filter={"$and":[{"nsd_date_from":{"$gt":"2017-05-22","$lt":"2017-05-24"}},{"isin":"RU0009029540"}]}
include | Нет  | Строка.<br> Список полей через запятую | Включить в ответ только указанные аттрибуты. Для вложенные аттрибутов  указываются полный путь через точку | &include=id,corp_action_type,coupon.number вернет документ следующей структуры: <br> { "id": 11111, "corp_action_type": {"id": 39,"code": "INTR", "name": "Выплата купонного дохода", "name_en": "Interest Payment"}, "coupon": { "number": 1 } }
exclude | Нет  | Строка.<br> Список полей через запятую | Исключить из ответа указанные аттрибуты. Может применяться для уменьшения объема получаемых данных, если элемент точно не нужен. Для вложенные аттрибутов  указываются полный путь через точку | &exclude=corp_action_type,securities.issuer,securities.bond.depository 


Так как нас интересуют корпоративные действия, выбираем продукт "corpactions"

```cs
    private const string ApiUrl =
        "https://nsddata.ru/api/get/corpactions";

    private const string ApiKey = "<INSERT_KEY>";
```

_В качестве тестового ключа (apikey) можете воспользоваться ключом "demo" (ApiKey = "demo") _

Для фильтрации данных используется параметр filter, представляющий собой массив условий, которым должны удовлетворять документы.
В условиях возможно использование следующих операторов:


Оператор  | Описание | Пример | Примечание
--- | --- | ---  | ---
$eq | равно | {"corp_action_type.code":{"$eq":"INTR"}} | краткая эквивалентная форма {"corp_action_type.code": "INTR"}
$ne | не равно | {"state.code":{"$ne":"C"}} | 
$in | равно одному из значений | {"securities.bond.currency.code":{"$in":["EUR","USD"]}} | 
$nin | не равно ни одному из значений | {"coupon.number":{"$nin":[1,3,5,7,9]}} | 
$gt, $gte, $lt, $lte | больше, больше либо равно, меньше, меньше либо равно | {"action_date_plan":{"$gt":"2017-05-01"}} | 
$and, $not, $nor, $or | логические операторы | {"$or":[{"securities.issuer.country.code":"RU"},{"securities.bond.currency.code":"RUB"}] | Для оператора $and можно использовать краткую форму { "action_date_plan": { "$gt": "2017-05-01", "$lt": "2017-05-31" } }
$exists | проверка существования элемента | {"securities.bond.pay_agent":{"$exists":false}} | 
$regex | соответствие регулярному выражению | {"securities.name_full":{"$regex":"^Документарные.*на предъявителя.*с обязательным"}} | 



С помощью JSON схемы или закладки "Персонализация" на [странице продукта](https://nsddata.ru/ru/products/getcorpactions/2) находим поля, по которым необходимо отфильтровать данные.


В нашем случае потребуется 4 условия:

* Бумага присутствует в листинге фондовой секции МБ<br/>
  Информация о бумагах находится в блоке securities. Листинги бумаги в блоке securities.listing. 
  Торгово-клиринговая система в элементе securities.listing.tks.
  Код фондовой секции МБ - MICEX_FOND
  Условие в JSON формате:
  {"securities.listing.tks.code": "MICEX_FOND"}

* Нас интересуют не все КД, а только собрания и дивиденды.<br/>
  Аналогично, видим что информация о типе КД находится в блоке corp_action_type
  { "corp_action_type.code": { "$in": "MEET", "XMET", "DVCA" } }

* Дата КД<br/>
  КД за период можно получить, применив 2 условия - дата КД больше начала диапазона и меньше окончания:
   { "action_date_plan": { "$gt": "2017-05-01", "$lt": "2017-05-31" } }

Условия можно сформировать в виде строки или создать объект JObject и сериализовать его в JSON.

```cs
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
                               {"$gt", dateFrom.ToString("yyyy-MM-dd")},
                               {"$lt", dateTo.ToString("yyyy-MM-dd")}
                           }
                       }
                   }
               }
           }
       };
```

Формируем строку подключения к API и выполняем запрос

```cs
    Console.WriteLine("Загрузка данных...");
    var wc = new WebClient {Encoding = Encoding.UTF8};

    var requestUrl = ApiUrl
                     + $"?filter={apiFilter.ToString(Formatting.None)}"
                     + $"&apikey={ApiKey}";


    var json = wc.DownloadString(requestUrl);
```

 В ответ получена строка, содержащая JSON документ. Десериализуем ее в массив JArray

```cs
    var corpActions = JArray.Parse(json);
    Console.WriteLine($"Загружено {corpActions.Count} документов ...");
```

Информацию о собраниях мы хотим поместить на первый лист книги Excel, о дивидендах на второй.
Для этого подготавливаем объекты DataTable:

```cs
    var meetingsTable = PrepareMeetingsDataTable("Meetings");
    var divTable = PrepareDivDataTable("Dividends");
```

Итерируемся по списку КД с использованием синтаксиса LINQ и заполняем таблицы

```cs
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
```

Создаем новый документ Excel, добавляем в него листы с информацией о КД, сохраняем на диск и открываем в приложении


```cs
    var wb = new XLWorkbook();

    wb.Worksheets.Add(meetingsTable);
    wb.Worksheets.Add(divTable);

    var fileName = $"{dateFrom: dd.MM.yyyy}- {dateTo: dd.MM.yyyy}.xlsx";

    wb.SaveAs(fileName);
    Console.WriteLine("Готово.");


    Process.Start(fileName);
```
