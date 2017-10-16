using Intelledox.Extension.Action;
using Intelledox.QAWizard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleActionExtensions
{
    /// <summary>
    /// This sample demonstrates registering available action inputs and outputs with Infiniti
    /// </summary>
    public class InputOutputAction : ActionConnector
    {
        private readonly Guid _basicInput = new Guid("9B1B2C85-841E-4F85-841F-93244B2D4C29");
        private readonly Guid _multiValueInput = new Guid("8FE0739D-ABB1-4600-B6C9-E149A05E418F");
        private readonly Guid _keyValueInput = new Guid("A71C3613-2F4A-4114-B71A-1A068D6D92D2");
        private readonly Guid _output = new Guid("3CD44B70-C70E-48FA-91A6-EF5B4A955550");

        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("FF4A4128-3277-4459-92CA-DEA1F00CD21F"),
                Name = "Infiniti Input Output Action Extension"
            };

        public override List<AvailableInput> GetAvailableInputs()
        {
            // Inputs provide a way for an action to identify what information it needs.
            // These might be configuration options to change how the action behaves or
            // could be setup as references to pull selected information from the form.

            return new List<AvailableInput>()
            {
                // Basic input that can be added once to a given action. Simply takes
                // a value from the form into the action.
                new AvailableInput()
                {
                    Id = _basicInput,
                    Name = "Basic"
                },

                // Configures the input for an unlimited number of uses for a given action.
                // Multi-valued input.
                new AvailableInput()
                {
                    Id = _multiValueInput,
                    Name = "Multi",
                    InstanceLimit = 0
                },

                // Key value input that will display a name and a value in Design. Useful
                // for configurable input names or matching key/value parameters in external
                // services.
                new AvailableInput()
                {
                    Id = _keyValueInput,
                    Name = "KeyValue",
                    InstanceLimit = 0,
                    IsKeyValue = true
                }
            };
        }

        public override List<AvailableOutput> GetAvailableOutputs()
        {
            // Outputs are information that this action has created that it may want to pass on
            // to other actions. For example a reference number or database record id.

            return new List<AvailableOutput>()
            {
                new AvailableOutput()
                {
                    Id = _output,
                    Name = "Custom Output"
                }
            };
        }

        public override Task<ActionResult> RunAsync(ActionProperties properties)
        {
            // Get a single input value as a boolean and fallback to 'false' if the input hasn't be configured.
            bool basicInputValue = properties.GetInputValue(_basicInput, false);

            // Get a multi-valued input as a list converting the values into integers. An empty list
            // is returned if the input hasn't been configured.
            IList<int> multiInputValue = properties.GetInputValueList<int>(_basicInput);

            // Get a key/value input as a dictionary. An empty dictionary is returned if the input hasn't been configured.
            IDictionary<string, string> keyValueInputValue = properties.GetInputKeyValues<string>(_basicInput);


            // Implement custom action details here. Call services, save documents, etc


            var result = new ActionResult()
            {
                Result = Intelledox.QAWizard.Design.ActionResultType.Success,

                Outputs = new List<Intelledox.Model.ActionOutput> {
                    // Return an output value as part of the results.
                    new Intelledox.Model.ActionOutput()
                    {
                        ID = _output,
                        Name = "Custom Output",
                        Value = "myOutputValue"
                    }
                }
            };

            return Task.FromResult(result);
        }

        public override bool SupportsRun() => true;

        public override bool SupportsUI() => false;
    }
}
