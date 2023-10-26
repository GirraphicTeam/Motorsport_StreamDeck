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

namespace Motorsport_StreamDeck
{
    [PluginActionId("com.hardenduro.streamdeck.car")]
    public class Car : PluginBase
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
        public Car(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            // Used for long press
            //keyPressStart = DateTime.Now;
            //keyPressed = true;

            List<Rider> eList;

            using (StreamReader file = File.OpenText(@"EntryList.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                eList = (List<Rider>)serializer.Deserialize(file, typeof(List<Rider>));

                SelectedCars = JsonFiles.LoadJSONList("Selected");
                string value = "";
                int carNum = 0;
                if (eList != null && eList.Count > 0 && settings.CarPosition > 0 && settings.CarPosition <= eList.Count)
                {
                    value = eList[settings.CarPosition - 1].BikeNum.ToString();
                }

                UdpClient udpClient = new UdpClient();

                Byte[] sendBytes = Encoding.UTF8.GetBytes("NAMESUPER|" + value);
                try
                {
                    udpClient.Send(sendBytes, sendBytes.Length, "127.0.0.1", 50511);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Int32.TryParse(value, out carNum);

                if (carNum > 0)
                {
                    bool isSel = false;
                    foreach (var l in SelectedCars)
                    {
                        if (l == carNum)
                        {
                            isSel = true;
                            break;
                        }
                    }

                    if (!isSel)
                    {
                        SelectedCars.Add(carNum);
                    }
                    else
                    {
                        SelectedCars.Remove(carNum);
                    }

                    JsonFiles.SaveJSONList("Selected", SelectedCars);
                }
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
            List<Rider> eList;

            using (StreamReader file = File.OpenText(@"EntryList.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                eList = (List<Rider>)serializer.Deserialize(file, typeof(List<Rider>));

                SelectedCars = JsonFiles.LoadJSONList("Selected");
                string value = String.Empty;
                if (eList != null && eList.Count > 0 && settings.CarPosition > 0 && settings.CarPosition <= eList.Count)
                {
                    value = eList[settings.CarPosition - 1].BikeNum.ToString();
                    value += "\n" + eList[settings.CarPosition - 1].Name;
                }
                int carNum = 0;
                Int32.TryParse(value, out carNum);

                await Connection.SetTitleAsync($"{value}");

                if (carNum > 0)
                {
                    bool isSel = false;
                    foreach (var l in SelectedCars)
                    {
                        if (l == carNum)
                        {
                            isSel = true;
                            break;
                        }
                    }
                    if (isSel)
                    {
                        Image newImg = Image.FromFile("Images\\BackSel.png");
                        await Connection.SetImageAsync(newImg, null, true);
                    }
                    else
                    {
                        Image newImg = Image.FromFile("Images\\Back.png");
                        await Connection.SetImageAsync(newImg, null, true);
                    }
                }
            }
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        #endregion
    }
}
