using System;
using System.IO;
using Intelledox.Extension.State;
using Intelledox.QAWizard;

namespace SampleStateExtensions
{
    /// <summary>
    /// This sample demonstrates the basic shell of a form state extension
    /// </summary>
    public class SimpleState : StateExtension
    {
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("5F4775F3-3ED1-4ECD-BD97-7488D156B264"),
                Name = "Infiniti Simple State Extension"
            };

        public override void AnswerFileSaving(StateProperties properties)
        {
            // Called when the user is saving an answer file in the form
            // properties.GetAnswerFile();
        }

        public override void ChangingPage(StateProperties properties, PageChangeArguments direction)
        {
            // Called each time the user navigates to a different page in the form. Either
            // forwards or backwards
            File.WriteAllText(@"c:\Temp\" + properties.Context.Wizard.WizardSession.Variables.RunId + ".xml", properties.GetAnswerFile());
        }

        public override bool UseStandardSave(StateProperties properties)
        {
            // Whether Infiniti should carry out its regular save routine when the user clicks save
            return true;
        }

        public override void WriteHtml(StateProperties properties, TextWriter writer)
        {
            // Allows a custom UI such as a button to be displayed in the form.
        }
    }
}
