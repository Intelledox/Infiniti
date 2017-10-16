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
    /// This sample demonstrates a fully featured 'Password' custom question. It is documented in detail on the
    /// knowledge base http://ixsupport.intelledox.com/kb
    /// </summary>
    public class AdvancedCustomQuestion : CustomQuestionExtension
    {
        private Guid _maxLengthGuid = new Guid("23C18C01-B617-4678-B355-A014B73E23C6");
        private Guid _defaultValueGuid = new Guid("37422F36-A896-4FF3-8CEA-6BA6F319C704");
        private Guid _manualChangeFlagGuid = new Guid("385F2B36-93C4-42A1-9D5C-2F6AEF2EC32A");
        private Guid _parentChangeFlagGuid = new Guid("23930E83-40D6-4147-8A6C-589EEC722992");
        private Guid _valueGuid = new Guid("EA232F27-1D3E-4EB4-AFEA-331A20BC701D");

        public override ExtensionIdentity ExtensionIdentity { get; protected set; }
            = new ExtensionIdentity()
            {
                Id = new Guid("63E9819F-5B09-4CF1-BE6F-416088484247"),
                Name = "Advanced Question"
            };

        public override List<AvailableInput> GetAvailableInputs()
        {
            return new List<AvailableInput>()
            {
                new AvailableInput()
                {
                    Id = _maxLengthGuid,
                    Name = "Max Length"
                },
                new AvailableInput()
                {
                    Id = _defaultValueGuid,
                    Name = "Default Value"
                }
            };
        }

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

        public override void InitialiseInputs(CustomQuestionProperties props)
        {
            props.UpdateAttribute(_manualChangeFlagGuid, false);
            props.UpdateAttribute(_parentChangeFlagGuid, false);
            foreach (CustomQuestionInput input in props.QuestionInputs)
            {
                if (input.InputTypeId == _maxLengthGuid)
                {
                    int maxLength = 0;
                    if (int.TryParse(input.Value, out maxLength))
                    {
                        props.UpdateAttribute(_maxLengthGuid, maxLength);
                    }
                }
                if (input.InputTypeId == _defaultValueGuid)
                {
                    props.UpdateAttribute(_valueGuid, input.Value.ToString());
                }
            }
        }

        public override void WriteHtml(string controlPrefix, CustomQuestionProperties props, TextWriter writer)
        {
            writer.Write("<input type=\"password\" name=\"");
            writer.Write(controlPrefix);
            writer.Write("\" id=\"");
            writer.Write(controlPrefix);
            writer.Write("\"");
            if (!string.IsNullOrEmpty(props.GetAttributeString(_maxLengthGuid)))
            {
                writer.Write(" maxlength=\"");
                writer.Write(WebUtility.HtmlEncode(props.GetAttributeString(_maxLengthGuid)));
                writer.Write("\"");
            }
            if (!string.IsNullOrEmpty(props.GetAttributeString(_valueGuid)))
            {
                writer.Write(" value=\"");
                writer.Write(WebUtility.HtmlEncode(props.GetAttributeString(_valueGuid)));
                writer.Write("\"");
            }
            if (props.Question.IsRealtimeParentQuestion)
            {
                writer.Write(" onblur=\"if ($('#" + controlPrefix + "_change').val() == '1') {" + TRIGGER_REFRESH + "}\"");
            }
            writer.Write(" />");

            if (props.Question.IsRealtimeParentQuestion)
            {
                writer.Write("<input type=\"hidden\" id=\"" + controlPrefix + "_change\" name=\"" + controlPrefix + "_change\" value=\"0\" />");
                writer.Write("<script type=\"text/javascript\">");
                writer.Write("$('#" + controlPrefix + "').on('change', function() { $('#" + controlPrefix + "_change').val('1'); });");
                writer.Write("$('#" + controlPrefix + "').on('keydown', function(e) ");
                writer.Write("{ ");
                writer.Write("    var code = e.keyCode || e.which;");
                writer.Write("    if (code != '9' && code != '16')");
                writer.Write("        $('#" + controlPrefix + "_change').val('1'); ");
                writer.Write("});");
                writer.Write("</script>");
            }
            props.UpdateAttribute(_parentChangeFlagGuid, false);
        }

        // Not needed, but here for demonstration purposes
        public override bool IsValid(CustomQuestionProperties props, ref string validationMessage)
        {
            return true;
        }

        public override void UpdateAttributes(string controlPrefix, NameValueCollection postedFormValues, CustomQuestionProperties props)
        {
            if (!(bool)props.GetAttributeBool(_parentChangeFlagGuid))
            {
                if (props.GetAttribute(_valueGuid) as string != postedFormValues[controlPrefix])
                {
                    props.UpdateAttribute(_manualChangeFlagGuid, true);
                }
                props.UpdateAttribute(_valueGuid, postedFormValues[controlPrefix]);
            }
        }

        public override void FillAnswerFileNode(System.Xml.Linq.XElement answerFileNode, CustomQuestionProperties props)
        {
            if (props.ContainsAttribute(_valueGuid))
            {
                answerFileNode.Add(new System.Xml.Linq.XAttribute(_valueGuid.ToString(), props.GetAttributeString(_valueGuid)));
            }
        }

        public override void ReadAnswerFileNode(System.Xml.Linq.XElement answerFileNode, CustomQuestionProperties props)
        {
            foreach (System.Xml.Linq.XAttribute xmlAttribute in answerFileNode.Attributes())
            {
                if (string.Equals(xmlAttribute.Name.ToString(), _valueGuid.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    string savedValue = xmlAttribute.Value;
                    if (savedValue != props.GetAttributeString(_valueGuid))
                    {
                        props.UpdateAttribute(_manualChangeFlagGuid, true);
                    }
                    props.UpdateAttribute(_valueGuid, savedValue);
                }
            }
        }

        public override bool HasBeenAnswered(CustomQuestionProperties props)
        {
            if (props.ContainsAttribute(_valueGuid))
            {
                return !string.IsNullOrEmpty(props.GetAttributeString(_valueGuid));
            }
            else
            {
                return false;
            }
        }

        public override void InputChanged(CustomQuestionInput input, object oldValue, CustomQuestionProperties props)
        {
            if (input.InputTypeId == _defaultValueGuid)
            {
                if (!(bool)props.GetAttributeBool(_manualChangeFlagGuid))
                {
                    props.UpdateAttribute(_valueGuid, input.Value);
                    props.UpdateAttribute(_parentChangeFlagGuid, true);
                }
            }
            else if (input.InputTypeId == _maxLengthGuid)
            {
                props.UpdateAttribute(_maxLengthGuid, input.Value);
            }
        }
    }
}
