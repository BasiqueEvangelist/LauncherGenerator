package me.basiqueevangelist.launchergenerator.authhelper.datacontracts;

import java.util.List;

public class AuthenticateResponse {
    public String accessToken;

    public String clientToken;

    public List<YggdrasilProfile> availableProfiles;

    public YggdrasilProfile selectedProfile;

    public YggdrasilUser user;
}
