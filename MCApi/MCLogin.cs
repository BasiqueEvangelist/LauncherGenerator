using System.Net.Http;

namespace MCApi
{
    public static class MCLogin
    {
        public const string AUTHSERVER_URL = "https://authserver.mojang.com";

        public static async Task<bool> CheckConnection()
        {
            try
            {
                await MCHttpHelper.Head(AUTHSERVER_URL);
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public static async Task<MCSession> Login(string email, string password)
        {
            if (!await CheckConnection())
                throw new MCLoginException();
            AuthenticateRequest req = new AuthenticateRequest
            {
                Agent = new AuthenticateRequest.AgentData
                {
                    Name = "Minecraft",
                    Version = 1
                },
                Username = email,
                Password = password,
                ClientToken = Guid.NewGuid().ToString(),
                RequestUser = true
            };
            AuthenticateResponse r = await MCHttpHelper.PostYggdrasil<AuthenticateResponse, AuthenticateRequest>(AUTHSERVER_URL + "/authenticate", req);
            return new MCSession(r, email);
        }

        public static async Task Signout(string email, string password)
        {
            if (!await CheckConnection())
                throw new MCLoginException();
            SignoutRequest req = new SignoutRequest
            {
                Username = email,
                Password = password
            };
            await MCHttpHelper.PostYggdrasil(AUTHSERVER_URL + "/signout", req);
        }
    }
    public class MCSession
    {
        private string accessToken;
        private string clientToken;
        private MCProfile[] profiles;
        private int selected;
        private string userId;
        private Dictionary<string, string> userProperties;
        private bool isOffline;
        private string email;

        public string AccessToken => accessToken;
        public MCProfile[] Profiles => profiles;
        public MCProfile SelectedProfile => profiles[selected];
        public string UserID => UserID;
        public bool IsOffline => isOffline;

        internal MCSession(AuthenticateResponse resp, string email)
        {
            accessToken = resp.AccessToken;
            clientToken = resp.ClientToken;
            profiles = resp.AvailableProfiles;
            selected = Array.IndexOf(resp.AvailableProfiles, resp.SelectedProfile);
            userId = resp.User.ID;
            userProperties = resp.User.Properties == null ? new Dictionary<string, string>() : resp.User.Properties.ToDictionary(x => x.Name, x => x.Value);
            this.email = email;
            isOffline = false;
        }

        internal MCSession(SavedSession ss)
        {
            accessToken = ss.AccessToken;
            clientToken = ss.ClientToken;
            profiles = ss.AvailableProfiles;
            selected = Array.IndexOf(ss.AvailableProfiles, ss.SelectedProfile);
            userId = ss.User.ID;
            userProperties = ss.User.Properties == null ? new Dictionary<string, string>() : ss.User.Properties.ToDictionary(x => x.Name, x => x.Value);
            email = ss.Email;
            isOffline = ss.IsOffline;
        }

        internal MCSession(string username, string email)
        {
            accessToken = "00000000-0000-0000-0000-000000000000";
            clientToken = Guid.NewGuid().ToString();
            profiles = new MCProfile[]
            {
                new MCProfile()
                {
                PlayerID = GuidUtility.Create(Encoding.UTF8.GetBytes("OfflinePlayer:" + username)),
                PlayerName = username,
                IsLegacy = false
                }
            };
            selected = 0;
            userId = GuidUtility.Create(Encoding.UTF8.GetBytes("OfflinePlayer:" + username)).ToString();
            userProperties = new Dictionary<string, string>();
            this.email = email;
            isOffline = true;
        }

        public static MCSession PackFrom(SavedSession ss) => new MCSession(ss);

        public SavedSession PackInto()
        {
            return new SavedSession
            {
                AccessToken = accessToken,
                ClientToken = clientToken,
                AvailableProfiles = profiles,
                SelectedProfile = profiles[selected],
                User = new AuthenticateResponse.MCUser()
                {
                    ID = userId,
                    Properties = userProperties.Select(x => new AuthenticateResponse.UserProperty() { Name = x.Key, Value = x.Value }).ToArray()
                },
                Email = email,
                IsOffline = isOffline
            };
        }

        public async Task Refresh()
        {
            if (isOffline) throw new MCLoginException("Unable to refresh offline session");
            if (!await MCLogin.CheckConnection())
                throw new MCLoginException();
            RefreshRequest req = new RefreshRequest
            {
                AccessToken = accessToken,
                ClientToken = clientToken,
                RequestUser = true
            };
            RefreshResponse res = await MCHttpHelper.PostYggdrasil<RefreshResponse, RefreshRequest>(MCLogin.AUTHSERVER_URL + "/refresh", req);
            accessToken = res.AccessToken;
            selected = Array.IndexOf(profiles, res.SelectedProfile);
            userId = res.User.ID;
            userProperties = res.User.Properties.ToDictionary(x => x.Name, x => x.Value);
        }

        public async Task<bool> Validate()
        {
            if (isOffline) throw new MCLoginException("Unable to validate offline session");
            if (!await MCLogin.CheckConnection())
                throw new MCLoginException();
            ValidateRequest req = new ValidateRequest
            {
                AccessToken = accessToken,
                ClientToken = clientToken
            };
            try
            {
                await MCHttpHelper.PostYggdrasil(MCLogin.AUTHSERVER_URL + "/validate", req);
                return true;
            }
            catch (MCLoginException)
            {
                return false;
            }
        }
        public async Task Invalidate()
        {
            if (isOffline) throw new MCLoginException("Unable to invalidate offline session");
            if (!await MCLogin.CheckConnection())
                throw new MCLoginException();
            InvalidateRequest req = new InvalidateRequest
            {
                AccessToken = accessToken,
                ClientToken = clientToken
            };
            await MCHttpHelper.PostYggdrasil(MCLogin.AUTHSERVER_URL + "/invalidate", req);
        }
    }
}
