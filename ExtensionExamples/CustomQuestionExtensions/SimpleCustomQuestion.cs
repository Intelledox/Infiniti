using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using Intelledox.Extension.CustomQuestion;
using Intelledox.QAWizard;
using Intelledox.Model;

namespace SampleCustomQuestionExtensions
{
    /// <summary>
    /// This sample demonstrates a basic 'Hello World' level custom question extension
    /// </summary>
    public class SimpleCustomQuestion : CustomQuestionExtension
    {
        private Guid _valueGuid = new Guid("97FF19FF-CDB1-4109-A223-D6514B11B6ED");

        // The identity is used for registering the custom question within Infiniti. The id needs
        // to be unique and the name is displayed to the user in Design.
        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("E7213603-044E-419B-B00E-92205B378E74"),
                Name = "Infiniti Simple Custom Question Extension"
            };

        public override List<AvailableOutput> GetAvailableOutputs()
        {
            return new List<AvailableOutput>()
            {
                new AvailableOutput()
                {
                    Id = _valueGuid,
                    Name = "Value",
                    OutputType = CustomQuestionOutputType.Text
                }
            };
        }

        public override byte[] Icon16x16Png => IconHelper.GetResourceBytes("SampleCustomQuestionExtensions.Question16.png");

        public override byte[] Icon48x48Png => IconHelper.GetResourceBytes("SampleCustomQuestionExtensions.Question48.png");

        public override void UpdateAttributes(string controlPrefix, NameValueCollection postedFormValues, CustomQuestionProperties props)
        {
            // Updates the question attribute value from the form

            props.UpdateAttribute(_valueGuid, postedFormValues[controlPrefix]);
        }

        public override void WriteHtml(string controlPrefix, CustomQuestionProperties props, TextWriter writer)
        {
            // Writes the HTML UI of the current state of the question. Non-HTML must be appropriately encoded.

            writer.Write("<input name=\"");
            writer.Write(controlPrefix);
            writer.Write("\" value=\"");
            writer.Write(WebUtility.HtmlEncode(props.GetAttributeString(_valueGuid) ?? ""));
            writer.Write("\" />");
        }
    }
}
