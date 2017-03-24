using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Web.Http;

namespace Webhook.Controllers
{
    /// <summary>
    /// This sample demonstrates an Infiniti Webhook endpoint that can be called from the Webhook action.
    /// Set the minimum Action inputs in Design:
    ///     "Webhook URL endpoint" = https://server/site/api/samplewebhook
    ///     "Secret Key" = DemoValue
    /// </summary>
    public class SampleWebhookController : ApiController
    {
        public IHttpActionResult Post([FromBody] JObject data)
        {
            // Optional "API key" validation
            if ((string)data["secretKey"] != ConfigurationManager.AppSettings["ApiSecret"])
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }


            // Process posted custom values configured in Design for this action
            var values = (JObject)data["values"];

            if (values != null)
            {
                foreach (JProperty valueKvp in values.Properties())
                {
                    Debug.WriteLine(valueKvp.Name + " : " + valueKvp.Value);
                }
            }


            // Process posted documents
            var documents = (JArray)data["documents"];

            if (documents != null)
            {
                foreach (JObject doc in documents)
                {
                    Debug.WriteLine(doc["name"] + " : " + Convert.FromBase64String((string)doc["binary"]).Length.ToString());
                }
            }

            return Ok();
        }
    }
}
