package ru.nsd.example;

import com.sun.jersey.api.client.Client;
import com.sun.jersey.api.client.WebResource;
import com.sun.jersey.api.client.config.DefaultClientConfig;
import org.apache.poi.hssf.usermodel.HSSFRow;
import org.apache.poi.hssf.usermodel.HSSFSheet;
import org.apache.poi.hssf.usermodel.HSSFWorkbook;
import org.json.JSONArray;
import org.json.JSONObject;

import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.UriBuilder;
import java.io.FileOutputStream;
import java.io.IOException;

// v0
public class RestClientExample {

    private static final String NSD_DATA_HOST = "http://0.0.0.0:5000";
    private static final String METHOD_NAME = "securities";
    private static final String RESULT_EXCEL_PATH = "securities100.xls";

    public static void main(String[] args) {
        saveExcel(getFirstHundredSecurities());
    }

    private static JSONArray getFirstHundredSecurities() {
        Client client = Client.create(new DefaultClientConfig());
        WebResource webResource = client.resource(UriBuilder.fromUri(NSD_DATA_HOST).build());
        String jsonData = webResource.path(METHOD_NAME).queryParam("limit", "100")
                .accept(MediaType.APPLICATION_JSON_TYPE).get(String.class);
        return new JSONArray(jsonData);
    }

    private static void saveExcel(JSONArray securities) {
        try {
            FileOutputStream stream = new FileOutputStream(RESULT_EXCEL_PATH);
            HSSFWorkbook workbook = generateExcel(securities);
            workbook.write(stream);
            stream.close();
        } catch (IOException ex) {
            System.out.println(ex);
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