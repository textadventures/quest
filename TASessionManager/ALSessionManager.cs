using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using WebInterfaces;

namespace TASessionManager
{
    public class ALSessionManager
    {
        public class ApiUser
        {
            public string UserId { get; set; }
        }

        public User GetTAUser()
        {
            if (HttpContext.Current == null) return null;

            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get("al_session");
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

            user = new User {UserId = userData.UserId};

            HttpContext.Current.Session["user"] = user;
            HttpContext.Current.Session["usersession"] = cookie.Value;

            return user;
        }
    }
}