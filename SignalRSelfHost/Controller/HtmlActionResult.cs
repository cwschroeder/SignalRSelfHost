using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SignalRSelfHost
{
    public class HtmlActionResult : IHttpActionResult
    {
        private static readonly string ViewBaseDir =
            Path.Combine(new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName, "Views");

        private readonly string viewName;
        private readonly object model;

        public HtmlActionResult(string viewFileName, object model)
        {
            this.viewName = viewFileName;
            this.model = model;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var view = File.ReadAllText(Path.Combine(ViewBaseDir, this.viewName));
            var content = RazorEngine.Razor.Parse(view, this.model);

            // create http
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "text/html"),
                StatusCode = HttpStatusCode.OK
            };

            return Task.FromResult(response);
        }
    }
}