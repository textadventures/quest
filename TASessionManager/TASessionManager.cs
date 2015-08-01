using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using WebInterfaces;

namespace TASessionManager
{
    public class TASessionManager
    {
        public class ApiUser
        {
            public string UserId { get; set; }
        }

        public TAUser GetTAUser()
        {
            if (HttpContext.Current == null) return null;

            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get("ta_session2");
            if (cookie == null)
            {
                return null;
            }

            TAUser user = HttpContext.Current.Session["user"] as TAUser;
            string userSession = HttpContext.Current.Session["usersession"] as string;
            if (user != null && userSession != null && userSession == cookie.Value)
            {
                return user;
            }

            var userData = Api.GetData<ApiUser>("api/session/" + cookie.Value);
            if (userData == null) return null;

            user = new TAUser { UserId = userData.UserId };

            HttpContext.Current.Session["user"] = user;
            HttpContext.Current.Session["usersession"] = cookie.Value;

            return user;
        }
    }

    public class TAUser
    {
        public string UserId { get; set; }
    }
}