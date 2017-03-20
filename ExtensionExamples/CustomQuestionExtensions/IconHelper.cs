using System;
using System.IO;

namespace SampleCustomQuestionExtensions
{
    internal class IconHelper
    {
        public static Byte[] GetResourceBytes(string resourceName)
        {
            byte[] buffer = null;

            using (Stream resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                buffer = new byte[(int)resourceStream.Length];
                resourceStream.Read(buffer, 0, (int)resourceStream.Length);
            }

            return buffer;
        }
    }
}
