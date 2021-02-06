using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
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

          string msg = req.Query["msg"];

          string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
          dynamic data = JsonConvert.DeserializeObject(requestBody);
          msg ??= data?.msg;

          string responseMessage = string.IsNullOrEmpty(msg)
             ? "Pass a message with parameter msg to save to queue"
             : $"Hello, Your message {msg} has been saved.";

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
          if(blobStream != null)
          {
             queueMsg = $"Got blob file {filename} with size: " + blobStream.Length;
          }

       }
   }
}
