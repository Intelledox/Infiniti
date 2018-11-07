using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AdminApi
{
    class Program
    {
        private static string _serviceUrl = "http://server/Manage/api/v1/admin/";              // Infiniti Manage url
        private static string _demoUsername = "DemoUser";                                      // User that the adminApi will run as
        private static string _demoPassword = "<DemoPassword>";

        static void Main(string[] args)
        {
            Task.Run(async () => await MainAsync(args)).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            HttpClient client = null;

            //Login
            client = await GetFormsAuthenticatedClient();
            //client = await GetWindowsAuthenticatedClient();

            //User
            await UserPostPatchAndDel(client);

            //Project Sync
            //await ProjectSyncExportImport(client);
        }

        static async Task<HttpClient> GetFormsAuthenticatedClient()
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

        static async Task<HttpClient> GetWindowsAuthenticatedClient()
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
            client.BaseAddress = new Uri(_serviceUrl);

            HttpResponseMessage authResponse = await client.PostAsync("login/account", new StringContent(string.Empty));
            authResponse.EnsureSuccessStatusCode();
            JObject token = JObject.Parse(await authResponse.Content.ReadAsStringAsync());

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token["authorizationToken"].ToString());

            return client;
        }

        private async static Task UserPostPatchAndDel(HttpClient client)
        {
            JObject user = new JObject()
            {
                { "username" , "PostUser" },
                { "password", "InsertPassword#1234" },
                { "firstName" , "" },
                { "lastName" , "" },
                { "disabled",  false },
                { "changePasswordAtNextLogin",  false },
                { "passwordNeverExpires",  false },
                { "lockedOut",  false },
                { "twoFactorAuth",  false },
                { "email",  "" },
                { "culture",  "en-AU" },
                { "language",  "en" },
                { "timezone",  "AUS Eastern Standard Time" },
                { "prefix",  "" },
                { "jobTitle",  "" },
                { "organization",  "" },
                { "phone",  "" },
                { "fax",  "" },
                { "streetAddress", new JObject()
                    {
                        { "line1",  "" },
                        { "line2",  "" },
                        { "city",  "" },
                        { "state",  "" },
                        { "zipcode",  "" },
                        { "country",  "" }
                    }
                },
                { "postalAddress", new JObject()
                    {
                        { "line1",  "" },
                        { "line2",  "" },
                        { "city",  "" },
                        { "state",  "" },
                        { "zipcode",  "" },
                        { "country",  "" }
                    }
                },
                { "groups", new JArray()
                    {
                        { "Infiniti Users" }
                    }
                }
            };

            //Post (Create) user
            HttpResponseMessage postUserResponse = await client.PostAsync("users", new StringContent(user.ToString(), Encoding.UTF8, "application/json"));
            postUserResponse.EnsureSuccessStatusCode();

            //Get user
            Uri userUrl = postUserResponse.Headers.Location;
            HttpResponseMessage getInfinitiUserResponse = await client.GetAsync(userUrl);
            getInfinitiUserResponse.EnsureSuccessStatusCode();

            //Patch (update) user
            JObject infinitiUser = JObject.Parse(await getInfinitiUserResponse.Content.ReadAsStringAsync());
            infinitiUser["firstName"] = "PatchFirstName";
            infinitiUser["lastName"] = "PatchLastName";

            HttpRequestMessage patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), userUrl)
            {
                Content = new StringContent(infinitiUser.ToString(), Encoding.UTF8, "application/json")
            };
            HttpResponseMessage patchInfinitiUserResponse = await client.SendAsync(patchRequest);
            patchInfinitiUserResponse.EnsureSuccessStatusCode();

            //Delete the user
            HttpResponseMessage deleteUserResponse = await client.DeleteAsync(userUrl);
            deleteUserResponse.EnsureSuccessStatusCode();
        }
        static async Task ProjectSyncExportImport(HttpClient client)
        {
            var exportRequest = new JObject()
            {
                { "projects", new JArray()
                    {
                        new JObject() {
                            { "id", "353b3680-98d4-4528-8d9e-dca94c2a381c" }
                        }
                    }
                },
                { "options", new JObject()
                    {
                        { "projects",  true },
                        { "projectFragments",  true },
                        { "dataSources",  true },
                        { "connectionStrings",  true },
                        { "defaultData",  true },
                        { "publishFolders",  false },
                        { "groups",  false },
                        { "users",  false },
                        { "customFields", false },
                        { "connectorSettings", false },
                        { "roles",  false }
                    }
                },
            };

            HttpResponseMessage postSyncResponse = await client.PostAsync("projects/sync/export", new StringContent(exportRequest.ToString(), Encoding.UTF8, "application/json"));
            postSyncResponse.EnsureSuccessStatusCode();

            JObject infinitiSyncPack = JObject.Parse(await postSyncResponse.Content.ReadAsStringAsync());

            //Make Changes to the sync pack, for example
            if (infinitiSyncPack["dataSources"].HasValues)
            {
                foreach (var dataSource in infinitiSyncPack["dataSources"])
                {
                    if (dataSource["connectionString"] != null)
                    {
                        dataSource["connectionString"] = ((string)dataSource["connectionString"]).Replace("dev", "prod");
                    }
                }
            }

            // Create a client for another environment
            // Post sync pack to "projects/sync/import"
        }
    }
}
