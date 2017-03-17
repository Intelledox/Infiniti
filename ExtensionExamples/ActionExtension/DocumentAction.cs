using Intelledox.Extension.Action;
using Intelledox.QAWizard;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SampleActionExtensions
{
    /// <summary>
    /// This sample demonstrates dealing within generated documents that have been sent into the action.
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
                    using (var docStream = await properties.GetDocumentStreamAsync(doc))
                    using (var destStream = new MemoryStream())
                    {
                        // Write or send the document to a file, external service, etc
                        await docStream.CopyToAsync(destStream);
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
