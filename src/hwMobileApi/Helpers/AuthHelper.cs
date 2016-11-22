using System;
using System.Linq;
using HydrantWiki.Library.Constants;
using HydrantWiki.Library.Managers;
using HydrantWiki.Library.Objects;
using HydrantWiki.Mobile.Api.ResponseObjects;
using Nancy;
using TreeGecko.Library.Net.Objects;
using TreeGecko.Library.Common.Enums;
using Nancy.Extensions;
using Newtonsoft.Json;

namespace HydrantWiki.Mobile.Api.Helpers
{
    public static class AuthHelper
    {
        public static bool IsAuthorized(Request _request, out User _user)
        {
            HydrantWikiManager hwManager = new HydrantWikiManager();

            string username = _request.Headers["Username"].First();
            string authToken = _request.Headers["AuthorizationToken"].First();

            User user = hwManager.GetUser(UserSources.HydrantWiki, username);
            if (user != null)
            {
                TGUserAuthorization userAuth = hwManager.GetUserAuthorization(user.Guid, authToken);

                if (userAuth != null
                    && !userAuth.IsExpired())
                {
                    _user = user;

                    return true;
                }
            }

            _user = null;
            return false;
        }

        public static BaseResponse Authorize(Objects.AuthObject _auth, out User _user)
        {
            AuthorizationResponse authResponse = new AuthorizationResponse {Success = false};

            if (_auth == null)
            {
                _user = null;
                authResponse.Message = "Bad Request";
                return authResponse;
            }

            HydrantWikiManager hwManager = new HydrantWikiManager();
            _user = hwManager.GetUserByEmail(UserSources.HydrantWiki, _auth.Email);

            if (_user != null)
            {
                if (_user.IsVerified)
                {
                    if (_user.Active)
                    {
                        DateTime now = DateTime.UtcNow;
                        DateTime tenMinAgo = now.AddMinutes(-10);
                        int count10Min = hwManager.GetAuthenticationFailureCount(_user.Guid, tenMinAgo);
                        DateTime oneDayAgo = now.AddHours(-24);
                        int count1Day = hwManager.GetAuthenticationFailureCount(_user.Guid, oneDayAgo);


                        if (hwManager.ValidateUser(_user, _auth.Password))
                        {
                            TGUserAuthorization authorization =
                                TGUserAuthorization.GetNew(_user.Guid, "unknown");
                            hwManager.Persist(authorization);

                            var user = new HydrantWiki.Mobile.Api.Objects.User();
                            user.AuthorizationToken = authorization.AuthorizationToken;
                            user.DisplayName = _user.DisplayName;
                            user.Username = _user.Username;
                            user.UserType = Enum.GetName(typeof(UserTypes), _user.UserType);

                            authResponse.Success = true;
                            authResponse.User = user;
                            authResponse.Message = "";

                            hwManager.LogUserToInstall(_auth.InstallId, user.Username);

                            hwManager.LogInfo(_user.Guid, "User Logged In");

                            return authResponse;
                        }

                        //Record failure to test if this is an attack. 
                        hwManager.RecordAuthenticationFailure(_user.Guid);

                        //Bad password or username
                        hwManager.LogWarning(_user.Guid, "Bad user or password");
                        authResponse.Message = "Bad user or password";

                        return authResponse;
                    }

                    //user not active
                    //Todo - Log Something
                    hwManager.LogWarning(_user.Guid, "User Not Active");
                    authResponse.Message = "User not active";
                    return authResponse;
                }

                //User not verified
                //Todo - Log Something
                hwManager.LogWarning(_user.Guid, "User not verified");
                authResponse.Message = "User not verified";
                return authResponse;
            }

            //User not found
            hwManager.LogWarning(Guid.Empty, "User not found");
            authResponse.Message = "User not found";
            return authResponse;
        }

        public static BaseResponse Authorize(Request _request, out User _user)
        {
            string body = _request.Body.AsString();
            Objects.AuthObject auth = null;

            if (body != null)
            {
                auth = JsonConvert.DeserializeObject<Objects.AuthObject>(body);
            }

            return Authorize(auth, out _user);
        }
    }
}