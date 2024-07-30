using System.Web.Mvc;

namespace WebPlayer.Controllers
{
    public class ResumeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(string data)
        {
            Session["LoadData"] = data;
            return Json(new object());
        }
    }
}