using System;
using System.Data.SqlTypes;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;

namespace ChyaAzureFunc
{
   public class AzureDurableFunc
   {
      //Client
      [FunctionName("AzureDurableFunc")]
      public IActionResult ChyaDurableFuncTest(
         [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Durable/{msg}")] HttpRequest req,
         string msg,
         [DurableClient] IDurableClient orchest,
         ILogger log
         )
      {
         log.LogInformation("You triggered the client function.");

         var instanceId =  orchest.StartNewAsync<string>("OrchestrationCall",msg);

         var result = orchest.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId.Result);

         return result.Result;
      }

      //Orchestrator
      [FunctionName("OrchestrationCall")]
      public async Task<string> OrchestrationCallAsync([OrchestrationTrigger] IDurableOrchestrationContext client, ILogger log)
      {
         log.LogInformation("You triggered the OrchestrationCall function.");

         var msg = client.GetInput<string>();

         var res1 =  await client.CallActivityAsync<string>("ActivityCallFirst", msg);
         var res2 =  await client.CallActivityAsync<string>("ActivityCallSecond", res1);

         return res2;
      }

      [FunctionName("ActivityCallFirst")]
      public string ActivityCallFirst(
         [ActivityTrigger] IDurableActivityContext activityContext,
         [Blob("blobtrigger/dural.dat", FileAccess.Write)] Stream blobFile, 
         ILogger log
         )
      {
         log.LogInformation("You triggered the ActivityCallFirst function.");

         var input = activityContext.GetInput<string>();

         var sw = new StreamWriter(blobFile);
         sw.WriteLine(input);
         sw.Flush();

         return $"Message saved to blob: {input}";
      }

      [FunctionName("ActivityCallSecond")]
      public string ActivityCallSecond(
         [ActivityTrigger] IDurableActivityContext activityContext,
         [Queue("durablefuncmsg")] out string queueMsg,
         ILogger log
      )
      {
         log.LogInformation("You triggered the ActivityCallSecond function.");

         var input = activityContext.GetInput<string>();
         queueMsg = "Triggered 2nd activity :" + input;

         return $"Message saved to queue: {queueMsg}";
      }
   }
}
