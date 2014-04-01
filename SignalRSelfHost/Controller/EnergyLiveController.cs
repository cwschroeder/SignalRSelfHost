namespace SignalRSelfHost
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;
    

    using WebApiContrib.Formatting.Html;

    public class EnergyLiveController : ApiController
    {
        public IHttpActionResult Get()
        {
            return new ViewResult(Request, "EnergyLive", null);
        }
    }

    public class LoadLiveController : ApiController
    {
        public IHttpActionResult Get()
        {
            return new ViewResult(Request, "LoadLive", null);
        }
    }

    public class EnergySimulationController : ApiController
    {
        public IHttpActionResult Get()
        {
            return new ViewResult(Request, "EnergySimulation", null);
        }
    }

    public class LoadSimulationController : ApiController
    {
        public IHttpActionResult Get()
        {
            return new ViewResult(Request, "LoadSimulation", null);
        }
    }
}