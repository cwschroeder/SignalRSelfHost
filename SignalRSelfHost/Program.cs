using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;

namespace SignalRSelfHost
{
    using System.Web.Http;

    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Cors;
    using Microsoft.Owin.Hosting;
    using Microsoft.Owin.StaticFiles;

    using Owin;

    using WebApiContrib.Formatting.Html;
    using WebApiContrib.Formatting.Html.Formatting;
    using WebApiContrib.Formatting.Razor;

    public class Program
    {
        public static void Main(string[] args)
        {
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.
            string webApiUrl = "http://localhost:8080";
            string signalRUrl = "http://localhost:9000";

            using(WebApp.Start<OWINWebAPIConfig>(url: webApiUrl))
            using (WebApp.Start<OWINSignalRConfig>(url: signalRUrl))
            {
                Console.WriteLine("Web API running on {0}", webApiUrl);
                Console.WriteLine("SignalR running on {0}", signalRUrl);
                Console.ReadLine();
            }
        }
    }

    public class OWINWebAPIConfig
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            //appBuilder.UseStaticFiles("/web");
            appBuilder.UseStaticFiles();
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Razor - so WebApi can use views+razor
            config.Formatters.Add(new RazorViewFormatter(null, new RazorViewLocator(), new RazorViewParser(baseTemplateType: typeof(HtmlTemplateBase<>))));

            appBuilder.UseWebApi(config);
        } 
    }

    public class OWINSignalRConfig
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.MapSignalR();
        }
    }
}
