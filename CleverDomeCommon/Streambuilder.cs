
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CleverDomeCommon
{
    public static class StreamBuilder
    {
        private const string separator = "-----------------------------41184676334";

        public static Stream Build(Stream data, string fileName, Dictionary<string, string> parameters)
        {
            BinaryReader reader = new BinaryReader(data);
            byte[] tmp = tmp = reader.ReadBytes((int)data.Length);
            return Build(tmp, fileName, parameters);
        }

        public static Stream Build(byte[] data, string fileName, Dictionary<string, string> parameters)
        {
            Encoding encoding = Encoding.UTF8;
            MemoryStream stream = new MemoryStream();
            stream.WriteLine(separator);
            if (data != null)
            {
                stream.Write("Content-Disposition: form-data; name=\"file\"; filename=\"");
                stream.Write(fileName);
                stream.Write("\"\r\nContent-Type: application/force-download \r\n\r\n");
                stream.Write(data, 0, data.Length);
                stream.WriteLine();
                stream.WriteLine(separator);
            }

            foreach (var pair in parameters)
            {
                string str = string.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", pair.Key, pair.Value);
                stream.WriteLine(str);
                stream.WriteLine(separator);
            }

            stream.Position = 0;
            return stream;
        }

        public static Stream Build(Dictionary<string, string> parameters)
        {
            return Build((byte[])null, null, parameters);
        }

        private static void Write(this Stream stream, string str)
        {
            byte[] array = Encoding.UTF8.GetBytes(str);
            stream.Write(array, 0, array.Length);
        }

        private static void WriteLine(this Stream stream, string str = "")
        {
            stream.Write(str + "\r\n");
        }
    }
}
