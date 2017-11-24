using MQTTnet;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using System;
using System.Text;
using Xamarin.Forms;

namespace HmhengXamMQTT
{
    public partial class MainPage : ContentPage
    {
        IMqttClient mqttClient;
        public MainPage()
        {
            InitializeComponent();
            btnConnect.Clicked += BtnConnect_Clicked;
            btnSend.Clicked += BtnSend_Clicked;

            // Create a new MQTT client.
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            mqttClient.Connected += MqttClient_Connected;
            mqttClient.Disconnected += async (s, e) =>
            {

                btnConnect.Text = "Connect";
                btnConnect.IsEnabled = true;
                Console.WriteLine("### DISCONNECTED WITH SERVER ###");
            };
            mqttClient.ApplicationMessageReceived += MqttClient_ApplicationMessageReceived;
        }

        
        private async void MqttClient_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => {
                btnConnect.Text = "Connected";
                btnConnect.IsEnabled = false;
            });

            Console.WriteLine("### CONNECTED WITH SERVER ###");
            // Subscribe to a topic
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("Apple").Build());

            Console.WriteLine("### SUBSCRIBED ###");
        }
        
        private async void BtnSend_Clicked(object sender, EventArgs e)
        {
            var message = new MqttApplicationMessageBuilder()
                               .WithTopic(txtTopic.Text)
                               .WithPayload(editMessage.Text)
                               .WithExactlyOnceQoS()
                               .WithRetainFlag()
                               .Build();

            await mqttClient.PublishAsync(message);
        }
        
        private async void BtnConnect_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Use TCP/WebSocket connection.
                var options = new MqttClientOptionsBuilder()
                    //.WithWebSocketServer("localhost:55888/mqtt")
                    .WithTcpServer(txtHost.Text, Int32.Parse(txtPort.Text))
                    .Build();

                await mqttClient.ConnectAsync(options);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        
        private void MqttClient_ApplicationMessageReceived(object sender, MQTTnet.Core.MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage.Topic == "response")
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine();

                editMessage.Text = "";
                editMessage.Text += "### RECEIVED APPLICATION MESSAGE ###";
                editMessage.Text += "$ Topic = " + e.ApplicationMessage.Topic;
                editMessage.Text += "$ Payload = " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                editMessage.Text += "$ QoS = " + e.ApplicationMessage.QualityOfServiceLevel;
                editMessage.Text += "$ Retain = " + e.ApplicationMessage.Retain;
                //Console.WriteLine();
            }
        }

                
    }
}
