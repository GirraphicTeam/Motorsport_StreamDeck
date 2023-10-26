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
    [PluginActionId("com.hardenduro.streamdeck.bikeautoset")]
    public class BikeAutoSet : PluginBase
    {
        private class Rider
        {
            public int BikeNum;
            public string Position;
            public string Name;
            public string Country;
        }

        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings()
                {
                    ItemIndex = 0,
                };

                return instance;
            }

            [JsonProperty(PropertyName = "ItemIndex")]
            public int ItemIndex { get; set; }
        }

        #region Private members
        private PluginSettings settings;
        private string country = "";
        private Rider riderSelected = null;
        private bool clicked = false;
        //private bool keyPressed = false;
        //private DateTime keyPressStart;

        #endregion

        #region PluginBase Methods
        public BikeAutoSet(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            if (riderSelected != null)
            {
                if (clicked == false)
                {
                    UdpClient udpClient = new UdpClient();

                    Byte[] sendBytes = Encoding.UTF8.GetBytes("NAMESUPER|" + riderSelected.BikeNum);
                    try
                    {
                        udpClient.Send(sendBytes, sendBytes.Length, "127.0.0.1", 50511);
                        udpClient.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    Image IMG = Image.FromFile(@"Images/Clear.png");
                    SetNewImage(IMG);

                    clicked = true;
                }
                else
                {

                    UdpClient udpClient = new UdpClient();

                    Byte[] sendBytes = Encoding.UTF8.GetBytes("OUT");
                    try
                    {
                        udpClient.Send(sendBytes, sendBytes.Length, "127.0.0.1", 50511);
                        udpClient.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    Image IMG = Image.FromFile(@"Images/Flags/" + riderSelected.Country + ".jpg");
                    SetNewImage(IMG);

                    clicked = false;
                }
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
        }

        public override void KeyReleased(KeyPayload payload)
        {
            //keyPressed = false;
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Released");
        }

        public async void SetNewImage(Image img)
        {

            await Connection.SetImageAsync(img);
        }

        public async override void OnTick()
        {
            using (StreamReader file = File.OpenText(@"bikedata.json"))
            {
                string jsonData = file.ReadToEnd();
                List<Rider> riders = JsonConvert.DeserializeObject<List<Rider>>(jsonData);
                if (riders.Count > settings.ItemIndex)
                {
                    Rider r = riders[settings.ItemIndex];
                    riderSelected = r;
                    await Connection.SetTitleAsync($"{r.Name + "\n" + r.BikeNum + "\n\n" + r.Position}");

                    if (country != r.Country)
                    {
                        country = r.Country;
                        Image IMG = Image.FromFile(@"Images/Flags/" + r.Country + ".jpg");
                        await Connection.SetImageAsync(IMG);
                    }
                }
                else
                {
                    await Connection.SetTitleAsync($"");
                    Image IMG = Image.FromFile(@"Images/Back.png");
                    await Connection.SetImageAsync(IMG);
                }
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
