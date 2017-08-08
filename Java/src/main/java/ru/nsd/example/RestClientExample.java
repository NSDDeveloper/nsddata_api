package ru.nsd.example;

import com.sun.jersey.api.client.*;
import com.sun.jersey.api.client.config.ClientConfig;
import com.sun.jersey.api.client.config.DefaultClientConfig;
import com.sun.jersey.api.client.filter.ClientFilter;
import org.apache.poi.hssf.usermodel.HSSFRow;
import org.apache.poi.hssf.usermodel.HSSFSheet;
import org.apache.poi.hssf.usermodel.HSSFWorkbook;
import org.json.JSONArray;
import org.json.JSONObject;

import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.UriBuilder;
import java.io.FileOutputStream;
import java.io.IOException;

public class RestClientExample {

    private static final String NSD_DATA_HOST = "http://nsddata.ru/api/get";
    private static final String METHOD_NAME = "securities";
    private static final String RESULT_EXCEL_PATH = "C:/Temp/securities100.xls";

    public static void main(String[] args) {
        saveExcel(getFirstHundredSecurities());
    }

    private static JSONArray getFirstHundredSecurities() {
        ClientConfig config = new DefaultClientConfig();
        config.getProperties().put(ClientConfig.PROPERTY_FOLLOW_REDIRECTS, true);
        Client client = Client.create(config);
        client.setFollowRedirects(true);
        WebResource resource = client.resource(UriBuilder.fromUri(NSD_DATA_HOST).build());
        resource.addFilter(new ClientFilter() {
            @Override
            public ClientResponse handle(ClientRequest cr) throws ClientHandlerException {
                ClientHandler ch = getNext();
                ClientResponse resp = ch.handle(cr);

                if (resp.getStatusInfo().getFamily() != Response.Status.Family.REDIRECTION) {
                    return resp;
                }
                else {
                    // try location
                    String redirectTarget = resp.getHeaders().getFirst("Location");
                    cr.setURI(UriBuilder.fromUri(redirectTarget).build());
                    return ch.handle(cr);
                }
            }
        });
        String jsonData = resource.path(METHOD_NAME).queryParam("limit", "100").queryParam("apikey", "DEMO")
                .accept(MediaType.APPLICATION_JSON_TYPE).get(String.class);
        System.out.println(jsonData);
        return new JSONArray(jsonData);
    }

    private static void saveExcel(JSONArray securities) {
        try {
            FileOutputStream stream = new FileOutputStream(RESULT_EXCEL_PATH);
            HSSFWorkbook workbook = generateExcel(securities);
            workbook.write(stream);
            stream.close();
        } catch (IOException ex) {
            System.out.println(ex.getLocalizedMessage());
        }
    }

    private static HSSFWorkbook generateExcel(JSONArray securities) {
        HSSFWorkbook workbook = new HSSFWorkbook();
        HSSFSheet sheet = workbook.createSheet();
        int rownum = 1;

        HSSFRow headRow = sheet.createRow(rownum++);
        headRow.createCell(0).setCellValue("ISIN");
        headRow.createCell(1).setCellValue("Полное наименование");

        for (Object obj : securities) {
            JSONObject json = (JSONObject) obj;
            HSSFRow row = sheet.createRow(rownum++);
            row.createCell(0).setCellValue(json.optString("isin"));
            row.createCell(1).setCellValue(json.optString("name_full"));
        }

        return workbook;
    }
}