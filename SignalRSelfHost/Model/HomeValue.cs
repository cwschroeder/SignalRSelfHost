namespace SignalRSelfHost
{
    using WebApiContrib.Formatting.Html;

    [View("Home")]     //need this line to get the webApi views+razor to work
    public class HomeValue
    {
        public int[] Numbers { get; set; }
    }

    [View("Net")]
    public class NetValue
    {
        public string Name { get; set; }
    }
}