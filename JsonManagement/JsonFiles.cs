using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Motorsport_StreamDeck.JsonManagement
{
    public class JsonFiles
    {
        public static void ReadJSON(string fileName, ref string jsonContent)
        {
            jsonContent = "";
            int tries = 0;
            while (tries < 10)
            {
                try
                {
                    tries++;
                    if (File.Exists(fileName))
                    {
                        using (StreamReader reader = new StreamReader(fileName))
                        {
                            jsonContent = reader.ReadToEnd();
                        }
                    }
                    break;
                }
                catch (Exception)
                {
                }
            }
        }

        public static void WriteToJSON(string fileName, string jsonContent)
        {
            if (!File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            int tries = 0;
            while (tries < 10)
            {
                try
                {
                    tries++;
                    using (FileStream fs = File.Create(fileName))
                    {
                        byte[] info = new UTF8Encoding(true).GetBytes(jsonContent);
                        fs.Write(info, 0, info.Length);
                    }
                    break;
                }
                catch (Exception)
                {

                }
            }
        }

        public static List<int> LoadJSONList(string name)
        {
            string path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).CodeBase).Replace("file:\\", "");
            string serialized = "";
            string JSONFile;
            JSONFile = path + "\\" + name + ".json";

            if (File.Exists(JSONFile))
            {
                ReadJSON(JSONFile, ref serialized);
                var infoJSON = (JArray)JsonConvert.DeserializeObject(serialized);
                List<int> entryList = new List<int>();
                foreach (var v in infoJSON)
                {
                    int i = Convert.ToInt32(v.Value<string>());
                    entryList.Add(i);
                }

                return entryList;
            }

            return new List<int>();
        }

        public static void SaveJSONList(string name, List<int> elem)
        {
            string path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).CodeBase).Replace("file:\\", "");
            string serialized = "";
            string JSONFile = path + "\\" + name + ".json";
            serialized = JsonConvert.SerializeObject(elem);
            WriteToJSON(JSONFile, serialized);
        }
    }
}
