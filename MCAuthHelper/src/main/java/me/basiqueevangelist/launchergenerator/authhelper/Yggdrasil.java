package me.basiqueevangelist.launchergenerator.authhelper;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import me.basiqueevangelist.launchergenerator.authhelper.datacontracts.AuthenticateRequest;
import me.basiqueevangelist.launchergenerator.authhelper.datacontracts.AuthenticateResponse;
import me.basiqueevangelist.launchergenerator.authhelper.datacontracts.RefreshRequest;
import me.basiqueevangelist.launchergenerator.authhelper.datacontracts.RefreshResponse;

import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.UUID;

public final class Yggdrasil {
    private Yggdrasil() {

    }

    public static final Gson GSON = new GsonBuilder().registerTypeAdapter(UUID.class, new UUIDParser()).create();

    public static boolean check() {
        try {
            HttpURLConnection urlConnection = (HttpURLConnection) new URL("https://authserver.mojang.com").openConnection();
            urlConnection.setRequestMethod("HEAD");
            urlConnection.connect();
            try (InputStream is = urlConnection.getInputStream()) {
                is.read(new byte[1]);
            }
        } catch (MalformedURLException e) {
            throw new RuntimeException(e);
        } catch (IOException unused) {
            return false;
        }

        return true;
    }

    public static RefreshResponse refresh(RefreshRequest req) throws YggdrasilException {
        return RequestUtil.post("https://authserver.mojang.com/refresh", req, RefreshRequest.class, RefreshResponse.class);
    }

    public static AuthenticateResponse authenticate(AuthenticateRequest req) throws YggdrasilException {
        return RequestUtil.post("https://authserver.mojang.com/authenticate", req, AuthenticateRequest.class, AuthenticateResponse.class);
    }
}
