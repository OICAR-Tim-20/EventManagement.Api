using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers
{
    public class UserController : Controller
    {
        //TODO: napraviti metodu za dohvat user-a za prikaz profila(get)
        public IActionResult Index()
        {
            return View();
        }
    }
}
