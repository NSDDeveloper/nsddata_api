# Примеры использования API nsddata.

## Tutorial ##

https://github.com/NSDDeveloper/nsddata_api/blob/master/CS_Advanced/README.md

## FAQ ##

### Где узнать свой API ключ? ###
Залогиньтесь на nsddata.ru, затем в броузере откройте ссылку
https://nsddata.ru/ru/matrix

### Как оставить в ответе только несколько полей? ###
Используйте параметр include, добавьте названия полей через запятую
https://nsddata.ru/api/get/securities?product=2&include=isin,instr_type.name&apikey=API_ключ_здесь

### Как уменьшить объем ответа? ###
Включите в запрос заголовок
Accept-encoding: gzip

### Как отфильтровать не по одному isin, а по двум? ###
Используйте оператор $in
https://nsddata.ru/api/get/securities?product=2&filter={"isin":{"$in":["первый_isin_здесь","второй_isin_здесь"]}}&apikey=API_ключ_здесь

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
