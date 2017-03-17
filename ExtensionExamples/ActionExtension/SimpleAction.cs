using Intelledox.Extension.Action;
using Intelledox.QAWizard;
using System;
using System.Threading.Tasks;

namespace SampleActionExtensions
{
    /// <summary>
    /// This sample demonstrates the basic shell of an action extension.
    /// </summary>
    public class SimpleAction : ActionConnector
    {
        // The identity is used for registering the action within Infiniti. The id needs
        // to be unique and the name is displayed to the user in design.
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("94773256-9A6C-4281-B2C6-6F854DA076AB"),
                Name = "Infiniti Simple Action Extension"
            };


        // Main entry point into the action where it will perform its custom operations.
        // This will be called after the form has been submitted or after each document
        // generation for a repeating template.
        public override Task<ActionResult> RunAsync(ActionProperties properties)
        {
            ActionResult results = new ActionResult()
            {
                Result = Intelledox.QAWizard.Design.ActionResultType.Success
            };


            // Implement custom action details here. Call services, save documents, etc


            return Task.FromResult(results);
        }

        // Whether the action supports the RunAsync method. Typically true.
        public override bool SupportsRun() => true;

        // Whether the action displays a custom UI after submission. Typically false.
        public override bool SupportsUI() => false;
    }
}
