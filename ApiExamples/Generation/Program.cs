using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Generation
{
    class Program
    {
        private static string _serviceUrl = "http://server/produce/api/v1/client/";         // Infiniti Produce url
        private static string _projectGroupGuid = "f88d902f-3023-4586-b589-28356ec5925e";   // Publish id of the form. Retrieve from Manage
        private static string _demoUsername = "DemoUser";                                   // User that the generation will run as
        private static string _demoPassword = "<DemoPasswordHere>";

        static void Main(string[] args)
        {
            Task.Run(async () => await MainAsync(args)).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            HttpClient client = await GetAuthenticatedClient();
            await GenerateWithNoValues(client, _projectGroupGuid);

            /*
             * Other demonstration generation calls
             * 
             * await GenerateWithAnswerLabels(client, _projectGroupGuid);
             * await GenerateWithProvidedData(client, _projectGroupGuid);
             * await GenerateWithAnswerFile(client, _projectGroupGuid);
             * await GenerateWithDelayedSchedule(client, _projectGroupGuid);
             * await GenerateWithWorkflow(client, _projectGroupGuid);
             * await GenerateWithPollingAndDownload(client, _projectGroupGuid);
             */
        }

        static async Task<HttpClient> GetAuthenticatedClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_serviceUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            JObject login = new JObject()
            {
                { "UserName", _demoUsername },
                { "Password", _demoPassword }
            };

            HttpResponseMessage authResponse = await client.PostAsync("login/forms", new StringContent(login.ToString(), Encoding.UTF8, "application/json"));
            authResponse.EnsureSuccessStatusCode();
            JObject token = JObject.Parse(await authResponse.Content.ReadAsStringAsync());

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token["authorizationToken"].ToString());

            return client;
        }

        static async Task GenerateWithNoValues(HttpClient client, string projectGroupGuid)
        {
            // Generates a form passing no additional parameters. The form must be able to determine everything it needs to complete.
            HttpResponseMessage generateResponse = await client.PostAsync("generate/" + projectGroupGuid, new StringContent("{ }", Encoding.UTF8, "application/json"));
            generateResponse.EnsureSuccessStatusCode();
        }

        static async Task GenerateWithAnswerLabels(HttpClient client, string projectGroupGuid)
        {
            // Generates a form passing answer file labels.
            JObject generate = new JObject()
            {
                {
                    "values", new JObject
                    {
                        { "LabelName1", "Value1" },
                        { "LabelName2", "Value2" }
                    }
                }
            };

            HttpResponseMessage generateResponse = await client.PostAsync("generate/" + projectGroupGuid, new StringContent(generate.ToString(), Encoding.UTF8, "application/json"));
            generateResponse.EnsureSuccessStatusCode();
        }

        static async Task GenerateWithProvidedData(HttpClient client, string projectGroupGuid)
        {
            // Generates a form passing data source files.
            JObject generate = new JObject()
            {
                {
                    "data", new JArray
                    {
                        new JObject
                        {
                            { "dataServiceGuid", "8182A028-DE3B-473B-8436-914669871421" },
                            { "value", "CSVHeader1,CSVHeader2\r\nValue1,Value2" }
                        },
                        new JObject
                        {
                            { "dataServiceGuid", "FA9669BB-5091-4351-8852-29262592CA1E" },
                            { "value", "<xmldata>value1</xmldata>" }
                        }
                    }
                }
            };

            HttpResponseMessage generateResponse = await client.PostAsync("generate/" + projectGroupGuid, new StringContent(generate.ToString(), Encoding.UTF8, "application/json"));
            generateResponse.EnsureSuccessStatusCode();
        }

        static async Task GenerateWithAnswerFile(HttpClient client, string projectGroupGuid)
        {
            // Generates a form passing a pre-prepared answer file
            JObject generate = new JObject()
            {
                { "answerFile", "<AnswerFile Version=\"2\"><ps><p pid=\"eb63fa10-aa77-4f0e-aae4-3fa512fd89fa\"><qs /></p></ps></AnswerFile>" }
            };

            HttpResponseMessage generateResponse = await client.PostAsync("generate/" + projectGroupGuid, new StringContent(generate.ToString(), Encoding.UTF8, "application/json"));
            generateResponse.EnsureSuccessStatusCode();
        }

        static async Task GenerateWithDelayedSchedule(HttpClient client, string projectGroupGuid)
        {
            // Schedules a generation for a later time. Also useful for moving the generation load from the web server to the scheduler service.
            // The scheduler must installed and configured for the generation to occur.
            JObject generate = new JObject()
            {
                {
                    "schedule", new JObject
                    {
                        { "startDateTime", DateTime.UtcNow.AddHours(1) }
                    }
                }
            };

            HttpResponseMessage generateResponse = await client.PostAsync("generate/" + projectGroupGuid, new StringContent(generate.ToString(), Encoding.UTF8, "application/json"));
            generateResponse.EnsureSuccessStatusCode();
        }

        static async Task GenerateWithWorkflow(HttpClient client, string projectGroupGuid)
        {
            // Generates a workflow form passing assignment details for the next state.
            JObject generate = new JObject()
            {
                {
                    "workflow", new JObject
                    {
                        { "comment", "For approval" },
                        { "sendToGroupName", "Approvers" }
                    }
                }
            };

            HttpResponseMessage generateResponse = await client.PostAsync("generate/" + projectGroupGuid, new StringContent(generate.ToString(), Encoding.UTF8, "application/json"));
            generateResponse.EnsureSuccessStatusCode();
        }

        static async Task GenerateWithPollingAndDownload(HttpClient client, string projectGroupGuid)
        {
            Uri operationsUrl;

            // Start the form generation and retrieve the polling url
            using (HttpResponseMessage generateResponse = await client.PostAsync("generate/" + projectGroupGuid, new StringContent("{ }", Encoding.UTF8, "application/json")))
            {
                generateResponse.EnsureSuccessStatusCode();

                operationsUrl = generateResponse.Headers.Location;
            }

            string resourceUrl = null;
            int delayMilliseconds = 2000; // Set to the estimated time this form takes to complete.

            // Poll the operations url until the generation is complete
            while (true)
            {
                await Task.Delay(delayMilliseconds);

                using (HttpResponseMessage operationsResponse = await client.GetAsync(operationsUrl))
                {
                    operationsResponse.EnsureSuccessStatusCode();

                    if (operationsResponse.Headers.Contains("Retry-After"))
                    {
                        // Generation is still in-progress, try again after pausing
                        delayMilliseconds = (int)operationsResponse.Headers.RetryAfter.Delta.Value.TotalMilliseconds;
                    }
                    else
                    {
                        // Generation is complete
                        JObject operationsResult = JObject.Parse(await operationsResponse.Content.ReadAsStringAsync());

                        if ((string)operationsResult["status"] == "succeeded")
                        {
                            resourceUrl = (string)operationsResult["resourceLocation"];
                        }
                        else
                        {
                            Console.WriteLine((string)operationsResult["status"]);
                            return;
                        }

                        break;
                    }
                }
            }

            // Optionally retrieve the generated file list for download of the binaries
            while (true)
            {
                using (HttpResponseMessage resourceResponse = await client.GetAsync(resourceUrl))
                {
                    resourceResponse.EnsureSuccessStatusCode();
                    JObject resourceResult = JObject.Parse(await resourceResponse.Content.ReadAsStringAsync());

                    JArray files = (JArray)resourceResult["files"];

                    foreach (JObject file in files)
                    {
                        Console.Write(file["filename"]);
                        Console.Write(" : ");
                        Console.Write(file["resourceLocation"]);
                    }

                    if (resourceResult["@nextLink"] == null)
                    {
                        break;
                    }
                    else
                    {
                        resourceUrl = (string)resourceResult["@nextLink"];
                    }
                }
            }
        }
    }
}
