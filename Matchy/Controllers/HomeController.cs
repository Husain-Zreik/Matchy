using System.Web.Mvc;

namespace Matchy.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home/Index (Play)
        public ActionResult Index()
        {
            return View();
        }

        // GET: Home/ScoreBoard
        public ActionResult ScoreBoard()
        {
            return View();
        }

        // GET: Home/Settings
        public ActionResult Settings()
        {
            return View();
        }
    }
}
