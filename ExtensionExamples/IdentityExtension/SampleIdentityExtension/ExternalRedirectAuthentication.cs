using Intelledox.Extension.IdentityExtension;
using Intelledox.Model;
using System;
using System.Threading.Tasks;

namespace SampleIdentityExtension
{
    public class ExternalRedirectAuthentication : IdentityExtension
    {
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity
            {
                Id = new Guid("54D2C379-4BE8-4BAE-A614-8E2C24A4CBB4"),
                Name = "Infiniti External Redirect Identity Extension"
            };

        public override Task<ResponseDetails> ProcessUnauthenticatedResponseAsync()
        {
            // A response is coming back from Infiniti saying the user is unauthenticated.
            // Redirect the request to an IDP site

            var result = new ResponseDetails();

            result.ResponseAction = ResponseAction.Redirect;
            result.RedirectUrl = "https://my-external-idp-site.com/?callback=" + 
                UrlEncode("account/externalcallback/sample?returnurl=" + GetRequestUrl());

            return Task.FromResult(result);
        }

        public override Task<RequestDetails> ProcessRequestAsync()
        {
            // A request is coming into Infiniti. Check if it's a IDP callback.
            var result = new RequestDetails();
            var requestUri = GetRequestUrl().PathAndQuery;

            if (requestUri.IndexOf("account/externalcallback/sample", StringComparison.OrdinalIgnoreCase) > -1)
            {
                // Implement custom extension details here. Confirm posted details from the IDP and that
                // it actually came from the IDP

                string username = "username from idp";
                User checkUser = Intelledox.Controller.UserController.FindUserByUsername(username);

                if (checkUser == null)
                {
                    // New user not previously known to Infiniti

                    // Create address
                    Intelledox.Model.AddressBookItem ixUserAddress = new AddressBookItem();
                    Intelledox.Controller.AddressController.Update(ixUserAddress);

                    // Create user
                    bool exceedsLicenseLimit = false;
                    Intelledox.DataObjects.User ixUser = new Intelledox.DataObjects.User();
                    ixUser.Username = username;
                    ixUser.BusinessUnitGuid = Intelledox.Controller.BusinessUnitController.GetBusinessUnitsList()[0].BusinessUnitGuid;
                    ixUser.AddressId = ixUserAddress.AddressId;
                    ixUser.Update(ref exceedsLicenseLimit);

                    if (exceedsLicenseLimit || ixUser.ID <= 0)
                    {
                        // Fail fast
                        throw new Exception(exceedsLicenseLimit ? "License Exceeded" : "User Creation Error");
                    }

                    checkUser = Intelledox.Controller.UserController.FindUserByUsername(username);
                }

                if (checkUser.IsDisabled)
                {
                    // Deny access
                    result.RequestAction = RequestAction.Unauthorized;
                }
                else
                {
                    // Successful login, return the identity
                    var ident = Intelledox.Auth.SessionProperties.GetClaimsIdentity(checkUser.UserId, null, allowLogout: true);

                    result.LoginIdentity = ident;
                }
            }

            return Task.FromResult(result);
        }
    }
}
