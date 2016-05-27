using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Face;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HappinessIndexUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static string OxfordAPIKey = "YOUR PROJECT OXFORD KEY";
        string OxfordFaceAPIKey = "YOUR PROJECT OXFORD KEY";

        MediaCapture MC;
        DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(3) };
        EmotionServiceClient Oxford = new EmotionServiceClient(OxfordAPIKey);
        string faceAtrributes = "";
        string EmoAttributes = "";
        EmoCollection MyEmo = new EmoCollection();

        FaceServiceClient OxFaceRecognizer;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Init();
            dt.Tick += GetEmotions;
            dt.Start();
        }

        private async Task Init()
        {
            MC = new MediaCapture();
            var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var camera = cameras.First();
            var settings = new MediaCaptureInitializationSettings() { VideoDeviceId = camera.Id };
            await MC.InitializeAsync(settings);
            ViewFinder.Source = MC;
            await MC.StartPreviewAsync();

            OxFaceRecognizer = new FaceServiceClient(OxfordFaceAPIKey);
        }


        async void GetEmotions(object sender, object e)
        {
            try
            {
                var ms = new MemoryStream();
                var msFace = new MemoryStream();
                await MC.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), ms.AsRandomAccessStream());
                await MC.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), msFace.AsRandomAccessStream());

                msFace.Position = 0L;
                ms.Position = 0L;

                if (String.IsNullOrEmpty(faceAtrributes))
                {
                    faceAtrributes = await OxFaceRecognizer.DetectAllAsync(msFace, true, true, true, true);
                }
                else if (faceAtrributes != null && faceAtrributes.Length > 0)
                {
                    try
                    {
                        EmoAttributes = await Oxford.RecognizeReturnAsync(ms);
                        if (EmoAttributes != null && EmoAttributes.Length > 0)
                        {                        
                            //Remove special characters and provide a valid JSON 
                            string txtjsonResult = JSONMergeAndClean();

                            AgeAndGender.Text = txtjsonResult;

                            //function to be called
                            EventHubHandler x = new EventHubHandler();
                            x.ehpush(txtjsonResult);
                        }
                    }
                    catch { }
                }
            }
            catch (ObjectDisposedException ex)
            {
                var a = ex;
            }
        }


        public string JSONMergeAndClean()
        {
            string s = EmoAttributes.Remove(EmoAttributes.Length - 1, 1)
                                    .Remove(0, EmoAttributes.IndexOf("\"anger")) 
                                    + "," + 
                                    faceAtrributes.Remove(0, faceAtrributes.IndexOf("\"gender") - 1);

            StringBuilder sb = new StringBuilder(s);

            sb.Replace("}", "");
            sb.Replace("{", "");
            sb.Replace("[", "");
            sb.Replace("]", "");
            sb.Replace("\\\"", "\"");
            sb.Replace("\"\"faceRectangle\":", "{");
            sb.Replace("\"faceAttributes\":", "");
            sb.Replace("\"facialHair\":", "");
            sb.Replace("\"scores\":", "");
            sb.Replace("\",\"faceRectangle\":", ",");
            sb.Replace("E-", "");
          
            return "{" + sb.Remove(sb.Length - 1, 1) + "}".ToString();
        }

    }
}
