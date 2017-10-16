using System;
using System.Threading.Tasks;
using Intelledox.Extension.Escalation;
using Intelledox.QAWizard;

namespace SampleEscalationExtension
{
    /// <summary>
    /// This sample demonstrates the basic shell of a escalation extension
    /// </summary>
    public class SimpleEscalation : EscalationExtension
    {
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("E1B9C3DA-7427-4B53-A235-40A50CE69C73"),
                Name = "Infiniti Simple Escalation Extension"
            };

        // Main entry point in the escalation where it will perform its custom operations
        public override async Task RunAsync(EscalationProperties properties)
        {
            // Implement custom escalation details here. Send notifications, update services, etc
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\temp\HelloWorld.txt", true))
            {
                await file.WriteLineAsync(DateTime.Now.ToString());
            }
        }

        // Defines whether or not this escalation is allowed run multiple times for a particular project
        public override bool SupportsRecurring()
        {
            return true;
        }
    }
}
