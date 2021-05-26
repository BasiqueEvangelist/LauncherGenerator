package me.basiqueevangelist.launchergenerator.authhelper;

import com.google.gson.TypeAdapter;
import com.google.gson.stream.JsonReader;
import com.google.gson.stream.JsonWriter;

import java.io.IOException;
import java.util.UUID;

public class UUIDParser extends TypeAdapter<UUID> {
    @Override
    public void write(JsonWriter out, UUID value) throws IOException {
        out.value(value.toString().replace("-", ""));
    }

    @Override
    public UUID read(JsonReader in) throws IOException {
        return UUID.fromString(in.nextString().replaceAll("([a-z0-9]{8})([a-z0-9]{4})([a-z0-9]{4})([a-z0-9]{4})([a-z0-9]{12})", "$1-$2-$3-$4-$5"));
    }
}
