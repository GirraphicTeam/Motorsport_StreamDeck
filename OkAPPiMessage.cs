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
    [PluginActionId("com.hardenduro.streamdeck.okappimessage")]
    public class OkAPPiMessage : PluginBase
    {

        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings()
                {
                    TitleParam = String.Empty,
                    UDPMessage = String.Empty
                };

                return instance;
            }

            [JsonProperty(PropertyName = "UDPMessage")]
            public string UDPMessage { get; set; }

            [JsonProperty(PropertyName = "TitleParam")]
            public string TitleParam { get; set; }
        }

        #region Private members
        private PluginSettings settings;
        private bool active = false;
        //private bool keyPressed = false;
        //private DateTime keyPressStart;

        #endregion

        #region PluginBase Methods
        public OkAPPiMessage(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            UdpClient udpClient = new UdpClient();

            Byte[] sendBytes = Encoding.UTF8.GetBytes(settings.UDPMessage);
            try
            {
                udpClient.Send(sendBytes, sendBytes.Length, "127.0.0.1", 50511);
                udpClient.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
        }

        public async override void OnTick()
        {
            //Do Nothing
        }

        public async void SetNewTitle(string titleString)
        {
            await Connection.SetTitleAsync($"{titleString}");
        }

        public override void KeyReleased(KeyPayload payload)
        {
            //keyPressed = false;
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Released");
        }
        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        #endregion
    }
}
