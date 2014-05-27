namespace SignalRSelfHost
{
    using System;
    using System.Web.Http;

    public class HomeController : ApiController //PageResourcesController
    {
        //http://localhost:8080/home
        public HtmlActionResult GetValues()
        {
            return new HtmlActionResult("Home.cshtml", new { Numbers = new [] { 1, 2, 3 } });
        }

        //want to return just XML and not go via the view+razor+cshtml
        //Works ok for normal self-hosted webapi, but cant get to work for views+razor
        //http:localhost:8080/home/1
        public String Get2(int id)
        {
            return "Get2() returns a string";
        }

    }
}