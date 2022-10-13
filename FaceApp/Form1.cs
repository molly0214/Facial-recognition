using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace FaceApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnDetect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("請輸入圖片的網址");
            }
            else
            {
                HttpClient Client = new HttpClient();
                using (HttpResponseMessage Response=await Client.GetAsync(textBox1.Text))
                {
                    Response.EnsureSuccessStatusCode();
                    using (Stream inputStream = await Response.Content.ReadAsStreamAsync())
                    {
                        pictureBox1.Image = Image.FromStream(inputStream);
                    }
                }
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "3176efd4c2014aa7903981e7d0c143b6");

                // Request parameters
                queryString["returnFaceId"] = "true";
                queryString["returnFaceLandmarks"] = "false";
                queryString["returnFaceAttributes"] = "age,gender,emotion";
                queryString["recognitionModel"] = "recognition_04";
                queryString["returnRecognitionModel"] = "false";
                queryString["detectionModel"] = "detection_01";
                queryString["faceIdTimeToLive"] = "86400";
                var uri = "https://msit143000face.cognitiveservices.azure.com/face/v1.0/detect?" + queryString;

                JObject data = new JObject { ["url"] = textBox1.Text };
                string json = JsonConvert.SerializeObject(data);
                StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage JsonResponse =await Client.PostAsync(uri, stringContent);
                JsonResponse.EnsureSuccessStatusCode();
                string result = await JsonResponse.Content.ReadAsStringAsync();
                MessageBox.Show(result);
                dynamic faces = JsonConvert.DeserializeObject(result);
                foreach(var item in faces)
                {
                    JObject face = item as JObject;
                    int age = Convert.ToInt32(face["faceAttributes"]["age"]);
                    string gender = Convert.ToString(face["faceAttributes"]["gender"]);
                    JObject emotion = JObject.Parse(Convert.ToString(face["faceAttributes"]["emotion"]));
                    float MaxEmotion = 0;
                    string MaxEmotionName = "";
                    foreach(JProperty prop in emotion.Properties())
                    {
                        float PropertyValue = float.Parse(Convert.ToString(emotion[prop.Name]));
                        if(PropertyValue> MaxEmotion)
                        {
                            MaxEmotion = PropertyValue;
                            MaxEmotionName = prop.Name;
                        }
                    }
                    MessageBox.Show($"性別:{gender},年齡:{age},情緒:{MaxEmotionName},信心指數:{MaxEmotion * 100:n2}%");
                }


            }
        }



    }
}
