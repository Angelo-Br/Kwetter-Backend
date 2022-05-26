using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace AzureFunctionsLibs
{
    public static class Function1
    {
        [FunctionName("HttpExample")]
        public static async Task Run([TimerTrigger("*/15 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // This entire azure function will have the purpose of updating the timeline of the site to the most trending topics
            // for now for testing purpose it will simply change 1 value every 15 minutes
            var str = Environment.GetEnvironmentVariable("kwetter_db_string").Replace("DATABASE_NAME", "timelineservice");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var text = "UPDATE TimelineService.dbo.Timeline " +
                        "SET [Updated] = GetDate() WHERE Updated < GetDate();";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");
                }
            }
        }
    }
}
