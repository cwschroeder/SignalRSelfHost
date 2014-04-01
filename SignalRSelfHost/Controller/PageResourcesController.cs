namespace SignalRSelfHost
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Resources;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Http;

    // not really needed thanks to: appBuilder.UseStaticFiles();
    public class PageResourcesController : ApiController
    {
        //
        // An HTML page will have references to css, javascript and image files
        // This method supplies these file to the browser
        // These files are saved in the Visual Studio project as linked resources
        // Make sure the resources are names correctly (and correct case) i.e.:
        // <fileName> = <resourceName>.<fileExtension>
        // http://localhost:8080/api/PageResources/<fileName>
        // The fileExtension is used to determine how to extract & present the resource
        // (Note, <filename> is the reference in the HTML page 
        // - it needed be the same as the name of the actual file.)
        //

        public HttpResponseMessage Get(string filename)
        {
            String projectName = "Owin_Test1";

            //Obtain the resource name and file extension
            var matches = Regex.Matches(filename, @"^\s*(.+?)\.([^.]+)\s*$");
            String resourceName = matches[0].Groups[1].ToString();
            String fileExtension = matches[0].Groups[2].ToString().ToLower();
            Debug.WriteLine("Resource: {0} {1}",
                resourceName,
                fileExtension);

            //Get the resource
            var rm = new ResourceManager(
                projectName + ".Properties.Resources",
                typeof(Properties.Resources).Assembly);
            Object resource = rm.GetObject(resourceName);

            var imageConverter = new ImageConverter();
            byte[] resourceByteArray;
            String contentType;

            //Generate a byteArray and contentType for each type of resource
            switch (fileExtension)
            {
                case "jpg":
                case "jpeg":
                    resourceByteArray = (byte[])imageConverter.ConvertTo(resource, typeof(byte[]));
                    contentType = "image/jpeg";
                    break;

                case "png":
                    resourceByteArray = (byte[])imageConverter.ConvertTo(resource, typeof(byte[]));
                    contentType = "image/png";
                    break;

                case "css":
                    resourceByteArray = Encoding.UTF8.GetBytes((String)resource);
                    contentType = "text/css";
                    break;

                case "js":
                    resourceByteArray = Encoding.UTF8.GetBytes((String)resource);
                    contentType = "application/javascript";
                    break;

                case "html":
                default:
                    resourceByteArray = Encoding.UTF8.GetBytes((String)resource);
                    contentType = "text/html";
                    break;
            }

            //Convert resource to a stream, package up and send on to the browser
            using (var dataStream = new MemoryStream(resourceByteArray))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(dataStream) };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                return response;
            }
        }
    }
}