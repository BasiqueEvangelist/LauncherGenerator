package me.basiqueevangelist.launchergenerator.authhelper;

public class YggdrasilException extends Exception {
    public YggdrasilException() {
    }

    public YggdrasilException(String message) {
        super(message);
    }

    public YggdrasilException(String message, Throwable cause) {
        super(message, cause);
    }

    public YggdrasilException(Throwable cause) {
        super(cause);
    }
}
