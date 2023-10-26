using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Drawing;
using HotkeyCommands;
using HardEnduro_StreamDeck.JsonManagement;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Reflection;
using HardEnduro_StreamDeck;

namespace Motorsport_StreamDeck
{
    [PluginActionId("com.hardenduro.streamdeck.bikenumber")]
    public class BikeNumber : PluginBase
    {
        private class Rider
        {
            public int BikeNum;
            public string Name;
        }

        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings()
                {
                    TitleParam = String.Empty,
                    CarPosition = 0,
                };

                return instance;
            }

            [JsonProperty(PropertyName = "CarPosition")]
            public int CarPosition { get; set; }

            [JsonProperty(PropertyName = "TitleParam")]
            public string TitleParam { get; set; }
        }

        #region Private members
        private PluginSettings settings;
        //private bool keyPressed = false;
        //private DateTime keyPressStart;

        #endregion

        #region PluginBase Methods
        public BikeNumber(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings));
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            // New in StreamDeck-Tools v2.0:
            Tools.AutoPopulateSettings(settings, payload.Settings);
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        { }

        public List<int> SelectedCars = new List<int>();

        public override void KeyPressed(KeyPayload payload)
        {
            string path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).CodeBase).Replace("file:\\", "");
            string filename;
            filename = path + "\\CurrentNumber.txt";

            if (!File.Exists(filename))
            {
                File.Delete(filename);
            }

            using (FileStream fs = File.Create(filename))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("");
                fs.Write(info, 0, info.Length);
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
        }

        public override void KeyReleased(KeyPayload payload)
        {
            //keyPressed = false;
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Released");
        }

        public async override void OnTick()
        {
            using (StreamReader file = File.OpenText(@"CurrentNumber.txt"))
            {
                string contents = file.ReadToEnd();
                await Connection.SetTitleAsync($"{"Input:\n" + contents}");
                file.Close();
            }
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        #endregion
    }
}
