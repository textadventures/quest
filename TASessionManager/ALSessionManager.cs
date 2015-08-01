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

        public ALUser GetTAUser()
        {
            if (HttpContext.Current == null) return null;

            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get("al_session");
            if (cookie == null)
            {
                return null;
            }

            ALUser user = HttpContext.Current.Session["user"] as ALUser;
            string userSession = HttpContext.Current.Session["usersession"] as string;
            if (user != null && userSession != null && userSession == cookie.Value)
            {
                return user;
            }

            var userData = Api.GetData<ApiUser>("api/session/" + cookie.Value);
            if (userData == null) return null;

            user = new ALUser {UserId = userData.UserId};

            HttpContext.Current.Session["user"] = user;
            HttpContext.Current.Session["usersession"] = cookie.Value;

            return user;
        }
    }

    public class ALUser
    {
        public string UserId { get; set; }
    }
}