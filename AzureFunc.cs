using System;
using System.Data.SqlTypes;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChyaAzureFunc
{
   public static class AzureFunc
   {
      [FunctionName("HttpMsgToQueue")]
      public static IActionResult HttpMsgToQueue(
         [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
         [Queue("azurefuncmsg")] out string queueMsg,
         ILogger log)
      {
         log.LogInformation("C# HTTP trigger function processed a request.");

         ClaimsPrincipal identities = req.HttpContext.User;
         string username = identities.Identity?.Name;

         string msg = req.Query["msg"];

         string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
         dynamic data = JsonConvert.DeserializeObject(requestBody);
         msg ??= data?.msg;

         string responseMessage = string.IsNullOrEmpty(msg)
            ? "Pass a message with parameter msg to save to queue"
            : $"Hello {username}, Your message {msg} has been saved.";

         log.LogInformation("C# HTTP trigger function to save message: " + msg);
         queueMsg = msg;
         return new OkObjectResult(responseMessage);
      }

      [FunctionName("AzureMonitorMsgToQueue")]
      public static string AzureMonitorMsgToQueue(
         [HttpTrigger(AuthorizationLevel.Function)] HttpRequest req,
         [Queue("azureappmonitor")] out string msg,
         ILogger logger
         )
      {
         msg = "Alert triggered from App Service monitor to Azure Func";
         return msg;
      }

      [FunctionName("HttpMsgToQueueMsgRouted")]
      public static IActionResult HttpMsgToQueueMsgRouted(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "message/{msg:alpha}")] HttpRequest req,
         string msg,
         [Queue("azurefuncmsg")] out string queueMsg,
         ILogger log)
      {
         log.LogInformation("C# HTTP trigger function processed a request.");

         ClaimsPrincipal identities = req.HttpContext.User;
         string username = identities.Identity?.Name;

         string responseMessage = $"Hello {username}, Your message {msg} has been saved.";

         log.LogInformation("C# HTTP trigger function to save message: " + msg);
         queueMsg = msg;
         return new OkObjectResult(responseMessage);
      }

      [FunctionName("HttpMsgToQueueNumberRouted")]
      public static ObjectResult HttpMsgToQueueNumberRouted(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "message/{msg:double}")] HttpRequest req,
         string msg,
         [Queue("azurefuncmsg")] out string queueMsg,
         ILogger log)
      {
         log.LogInformation("C# HTTP trigger function processed a request.");

         ClaimsPrincipal identities = req.HttpContext.User;
         string username = identities.Identity?.Name;

         string responseMessage = $"Hello {username}, Your message number {msg} has been saved.";

         log.LogInformation("C# HTTP trigger function to save message: " + msg);
         queueMsg = msg;
         return new OkObjectResult(responseMessage);
      }

      [FunctionName("BlobTriggerFileNameToQueue")]
      public static void BlobTriggerFileNameToQueue(
          [BlobTrigger("blobtrigger/{filename}")] Stream blobStream, string filename, //file name binding expression
          [Queue("azurefuncmsg")] out string queueMsg,
          ILogger log)
      {
         log.LogInformation($"Function BlobTriggerFileNameToQueue triggered!");

         queueMsg = null;
         if (blobStream != null)
         {
            queueMsg = $"Got blob file {filename} with size: " + blobStream.Length;
         }

      }

       [FunctionName("TimerTriggerToQueue")]
       public static void TimerTriggerQueueInsert(
          [TimerTrigger("0 0 10 * * *", RunOnStartup = true, UseMonitor = true)] TimerInfo timer,
          [Queue("azurefuncmsg")] out string msg, 
          ILogger log)
       {
          log.LogInformation("TimerTriggerQueueInsert triggered"); 
          msg = "TimerTriggerQueueInsert triggered at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ", next trigger is at: " + timer.FormatNextOccurrences(1);
       }

       [FunctionName("EventGridHandler")]
       public static void EventGridTriggerToQueue([EventGridTrigger()] EventGridEvent ev,
                                                  [Queue("azurefuncmsg")] out string queueMsg)
       {
          var sb = new StringBuilder();
          sb.Append($"Event grid event: {ev.Subject} - {ev.Topic}, data: {ev.Data.ToString()}");
          sb.Append(ev.Subject);
          sb.Append(ev.Topic);
          sb.Append(ev.EventType);
          queueMsg = sb.ToString();
       }
    }
}
