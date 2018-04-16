# Примеры использования API nsddata.

## Tutorial ##

https://github.com/NSDDeveloper/nsddata_api/blob/master/CS_Advanced/README.md

## FAQ ##

### Что такое API ключ? Что такое DEMO? ###
API-ключ это Ваш идентификатор, который Вы дописываете в URL, чтобы сервер понимал кто обратился за данными и выдавал их в соответствии с приобретенным тарифом.
В примерах обычно написано &apikey=DEMO, это означает что будет выдан ограниченный десятью записями демонстрационный набор данных. Чтобы получить полный набор данных нужно:
1) Оформить подписку или пробный доступ (кнопки подписки и тестового доступа размещены на страницах API)
2) Залогиниться на nsddata.ru
3) В броузере открыть ссылку
https://nsddata.ru/ru/matrix
4) Переписать себе API ключ и далее подставлять его ко всем запросам вместо &apiKey=DEMO

### Как оставить в ответе только несколько полей? ###
Используйте параметр include. Например, чтобы вывести только ISIN бумаги и название вида бумаги, добавьте названия полей isin и instr_type.name в запрос через запятую после слова include.

https://nsddata.ru/api/get/securities?product=2&include=isin,instr_type.name&apikey=DEMO

Как называются другие поля и что они содержат можно узнать зайдя на страницу https://nsddata.ru/api/ на страницах соответствующих методов. 


### Как уменьшить объем ответа? ###
Включите в запрос заголовок:
Accept-encoding: gzip

### Как отфильтровать не по одному isin, а по двум? ###
Используйте оператор $in. Пример фильтрация по двум ISIN:

https://nsddata.ru/api/get/securities?product=2&filter={"isin":{"$in":["первый_isin_здесь","второй_isin_здесь"]}}&apikey=DEMO

## Документация разработчика ##
https://github.com/NSDDeveloper/nsddata_api/blob/master/Developer_Manual_NSDData_API.pdf

## Спецификация данных getSecurities, getCompanies, getCorporateActions ##
https://github.com/NSDDeveloper/nsddata_api/blob/master/API%20Data%20Description.pdf


## Примеры на C# ##
  * Пример №1 https://github.com/NSDDeveloper/nsddata_api/tree/master/CS_Advanced
  * Пример №2 https://github.com/NSDDeveloper/nsddata_api/tree/master/CS
  
## Пример на Java ##
https://github.com/NSDDeveloper/nsddata_api/tree/master/Java

## Пример на Python ##
https://github.com/NSDDeveloper/nsddata_api/tree/master/Python
