using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClientApi.Helper
{
    public static class AnswerFileHelper
    {
        public static string GetAnswerFile(string formId, Dictionary<Guid, string> providedData)
        {
            XDocument xDocument = new System.Xml.Linq.XDocument();

            XElement answerFile = new XElement("AnswerFile");
            XElement headerInfo = new XElement("HeaderInfo");
            XElement templateInfo = new XElement("TemplateInfo");

            templateInfo.Add(new XAttribute("TemplateGroupId", formId.ToString()));
            templateInfo.Add(new XAttribute("RunId", Guid.NewGuid().ToString().ToString()));
            templateInfo.Add(new XAttribute("FirstLaunchTimeUtc", DateTime.UtcNow));
            headerInfo.Add(templateInfo);

            answerFile.Add(headerInfo);
            xDocument.Add(answerFile);

            if (providedData.Count > 0)
            {
                XElement providedDataElm = new XElement("ProvidedData");

                foreach (Guid key in providedData.Keys)
                {
                    XElement dataElm = new XElement("Data");
                    dataElm.Add(new XAttribute("Id", key.ToString()));
                    dataElm.Add(new XCData(providedData[key]));
                    providedDataElm.Add(dataElm);
                }

                answerFile.Add(providedDataElm);
            }

            return answerFile.ToString();
        }
    }
}
