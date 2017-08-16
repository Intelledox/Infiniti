using Intelledox.Extension.IdentityExtension;
using System;
using System.Threading.Tasks;

namespace SampleIdentityExtension
{
    public class SimpleFormsAuthentication : FormsIdentityExtension
    {
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity
            {
                Id = new Guid("DECD315D-03B0-4DB7-A536-148267C89882"),
                Name = "Infiniti Simple Forms Identity Extension"
            };

        public override Task<bool> ValidateUserAsync(string username, string password)
        {
            var result = false;

            // Implement custom extension details here. Compare username/password to external sources, etc
            if (username == "Simple" && password == "Forms")
            {
                result = true;
            }

            return Task.FromResult(result);
        }
    }
}
