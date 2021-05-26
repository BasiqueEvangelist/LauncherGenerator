package me.basiqueevangelist.launchergenerator.authhelper;

import com.google.gson.JsonElement;
import com.google.gson.stream.JsonReader;
import com.google.gson.stream.JsonWriter;
import me.basiqueevangelist.launchergenerator.authhelper.datacontracts.YggdrasilError;

import java.io.*;
import java.net.HttpURLConnection;
import java.net.URL;

public final class RequestUtil {
    private RequestUtil() {

    }


    public static <O, I> O post(String url, I obj, Class<I> iClass, Class<O> oClass) throws YggdrasilException {
        try {
            HttpURLConnection conn = (HttpURLConnection) new URL(url).openConnection();
            conn.setRequestMethod("POST");
            conn.setRequestProperty("Content-Type", "application/json");
            conn.setRequestProperty("Accept", "application/json");
            conn.setDoOutput(true);

            try (OutputStream os = conn.getOutputStream();
                 BufferedOutputStream bos = new BufferedOutputStream(os);
                 OutputStreamWriter osw = new OutputStreamWriter(bos);
                 JsonWriter jw = new JsonWriter(osw)) {
                Yggdrasil.GSON.toJson(obj, iClass, jw);
            }

            conn.connect();

            if (conn.getResponseCode() > 299) {
                try (InputStream is = conn.getErrorStream();
                     BufferedInputStream bis = new BufferedInputStream(is);
                     InputStreamReader isr = new InputStreamReader(bis);
                     JsonReader jr = new JsonReader(isr)) {
                    YggdrasilError err = Yggdrasil.GSON.fromJson(jr, YggdrasilError.class);
                    throw new YggdrasilException(err.error + ": " + err.errorMessage);
                }
            }

            try (InputStream is = conn.getInputStream();
                 BufferedInputStream bis = new BufferedInputStream(is);
                 InputStreamReader isr = new InputStreamReader(bis);
                 JsonReader jr = new JsonReader(isr)) {
                return Yggdrasil.GSON.fromJson(jr, oClass);
            }

        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }
}
