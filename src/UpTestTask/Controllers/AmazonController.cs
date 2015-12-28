using Microsoft.AspNet.Mvc;

namespace UpTestTask.Controllers
{
    public class AmazonController : Controller
    {
        
        public string Index(string searchString)
        {
            return "Here we will add some JSON in the future, searchString = " + searchString;
        }
    }
}
