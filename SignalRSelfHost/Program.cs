namespace SignalRSelfHost
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Formatting;
    using System.Web;
    using System.Web.Http;

    using Microsoft.Owin.Cors;
    using Microsoft.Owin.Hosting;

    using Newtonsoft.Json;

    using Owin;

    using RazorEngine.Templating;

    using SignalRSelfHost.Infrastructure;
    using SignalRSelfHost.Infrastructure.Config;

    using WebApiContrib.Formatting.Razor;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        #region Public Methods and Operators

        public static void Main(string[] args)
        {
            JsonConfig.Init();

            var fo = new UploadObserver();
            fo.Run();

            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.
            string webApiUrl = string.Format(
                "{0}:{1}",
                JsonConfig.AppSettings.Web.BaseAddress,
                JsonConfig.AppSettings.Web.WebApiPort);
            string signalRUrl = string.Format(
                "{0}:{1}",
                JsonConfig.AppSettings.Web.BaseAddress,
                JsonConfig.AppSettings.Web.SignalrPort);

            using (WebApp.Start<OWINWebAPIConfig>(webApiUrl))
            using (WebApp.Start<OWINSignalRConfig>(signalRUrl))
            {
                Console.WriteLine("Web API running on {0}", webApiUrl);
                Console.WriteLine("SignalR running on {0}", signalRUrl);
                Console.ReadLine();
            }
        }

        #endregion
    }

    /// <summary>
    /// The owin web api config.
    /// </summary>
    public class OWINWebAPIConfig
    {
        #region Public Methods and Operators

        public void Configuration(IAppBuilder appBuilder)
        {
            // appBuilder.UseStaticFiles("/web");
            appBuilder.UseStaticFiles();

            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });

            // Razor - so WebApi can use views+razor
            config.Formatters.Add(
                new RazorViewFormatter(
                    null,
                    new RazorViewLocator(),
                    new RazorViewParser(baseTemplateType: typeof(HtmlTemplateBase<>))));

            // Limit formatters to json only
            JsonMediaTypeFormatter json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            appBuilder.UseWebApi(config);
        }

        #endregion
    }

    /// <summary>
    /// The owin signal r config.
    /// </summary>
    public class OWINSignalRConfig
    {
        #region Public Methods and Operators

        /// <summary>
        /// The configuration.
        /// </summary>
        /// <param name="appBuilder">
        /// The app builder.
        /// </param>
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.MapSignalR();
        }

        #endregion
    }
}