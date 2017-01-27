using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using Google.Apis.Gmail.v1.Data;
using System.Threading.Tasks;

namespace Purloin.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Analyze()
        {
            string[] scopes = { GmailService.Scope.MailGoogleCom };
            UserCredential credential;

            using (var stream = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath(@"~/Content/json/client_secret.json"), FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);

                credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-validation.json");

                if (Directory.Exists(credPath))
                {
                    foreach (var file in Directory.EnumerateFiles(credPath))
                    {
                        System.IO.File.Delete(file);
                    }
                }

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });

            List<string> messageIds = new List<string>();
            var request = service.Users.Messages.List("me");

            do
            {
                ListMessagesResponse response = request.Execute();
                messageIds.AddRange(response.Messages.Select(m => m.Id));
                request.PageToken = response.NextPageToken;
            } while (!String.IsNullOrEmpty(request.PageToken));

            List<Message> messages = new List<Message>();
            Parallel.ForEach(messageIds, messageId =>
            {
                messages.Add(service.Users.Messages.Get("me", messageId).Execute());
            });
           
            return View(messages);
        }
    }
}