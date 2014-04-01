namespace SignalRSelfHost
{
    using System;
    using System.Web.Http;

    public class HomeController : ApiController //PageResourcesController
    {
        //http://localhost:8080/api/home
        public HomeValue GetValues()
        {
            // Option via return type: return new View("Home", null);
            return new HomeValue() { Numbers = new int[] { 1, 2, 3 } };
        }

        //want to return just XML and not go via the view+razor+cshtml
        //Works ok for normal self-hosted webapi, but cant get to work for views+razor
        //http:localhost:8080/api/home/1
        public String Get2(int id)
        {
            return "Get2() returns a string";
        }

    }
}