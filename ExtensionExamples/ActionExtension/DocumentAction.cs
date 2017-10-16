using Intelledox.Extension.Action;
using Intelledox.QAWizard;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SampleActionExtensions
{
    /// <summary>
    /// This sample demonstrates accessing and working with generated document binaries within an action.
    /// The project’s administrator configures which document(s) should generated at the end of each workflow step and the final submission of the form.
    /// </summary>
    public class DocumentAction : ActionConnector
    {
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("D2FC0284-BF5E-41E8-9B05-340378D61710"),
                Name = "Infiniti Document Action Extension"
            };

        public async override Task<ActionResult> RunAsync(ActionProperties properties)
        {
            ActionResult results = new ActionResult()
            {
                Result = Intelledox.QAWizard.Design.ActionResultType.Success
            };

            try
            {
                foreach (var doc in properties.Documents)
                {
                    string fileName = properties.JobGuid.ToString() + " - " +
                        doc.DisplayName + doc.Extension;

                    using (var docStream = await properties.GetDocumentStreamAsync(doc))
                    // Write or send the document to a file, external service, etc
                    using (FileStream file = new FileStream(Path.Combine("C:\\temp\\", fileName), FileMode.Create, FileAccess.Write))
                    {
                        await docStream.CopyToAsync(file);
                    }                    
                }
            }
            catch (Exception ex)
            {
                properties.AddMessage(ex.Message, "Document Action Error");
                results.Result = Intelledox.QAWizard.Design.ActionResultType.Fail;
            }

            return results;
        }
        public override bool SupportsRun() => true;

        public override bool SupportsUI() => false;
    }
}
