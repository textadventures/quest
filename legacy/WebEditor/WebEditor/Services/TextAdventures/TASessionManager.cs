using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace TASessionManager
{
    public class TASessionManager
    {
        public class ApiUser
        {
            public string UserId { get; set; }
        }

        public User GetTAUser()
        {
            var debugUser = ConfigurationManager.AppSettings["DebugUserId"];
            if (debugUser != null)
            {
                return new User { UserId = debugUser };
            }

            if (HttpContext.Current == null) return null;

            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(ConfigurationManager.AppSettings["CookieName"]);
            if (cookie == null)
            {
                return null;
            }

            User user = HttpContext.Current.Session["user"] as User;
            string userSession = HttpContext.Current.Session["usersession"] as string;
            if (user != null && userSession != null && userSession == cookie.Value)
            {
                return user;
            }

            var userData = Api.GetData<ApiUser>("api/session/" + cookie.Value);
            if (userData == null) return null;

            user = new User { UserId = userData.UserId };

            HttpContext.Current.Session["user"] = user;
            HttpContext.Current.Session["usersession"] = cookie.Value;

            return user;
        }
    }
}