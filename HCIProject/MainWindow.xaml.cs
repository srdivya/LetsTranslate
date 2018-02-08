using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using Microsoft.Translator.Samples;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace HCIProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string TEXT_TRANSLATION_API_SUBSCRIPTION_KEY = "1dc6a1b7b4494fb9af8e73466096795d";
        private AzureAuthToken tokenProvider;
        private string[] friendlyName = { " " };
        private Dictionary<string, string> languageCodesAndTitles = new Dictionary<string, string>();
        private static int count = 1;
        public MainWindow()
        {
            InitializeComponent();
            tokenProvider = new AzureAuthToken(TEXT_TRANSLATION_API_SUBSCRIPTION_KEY);
            GetLanguagesForTranslate();
            GetLanguageNamesMethod(tokenProvider.GetAccessToken(), friendlyName);
            addLanguages();
        }
        private void addLanguages()
        {
            //run a loop to load the combobox from the dictionary
            var count = languageCodesAndTitles.Count;

            for (int i = 0; i < count; i++)
            {
                user1combo.Items.Add(languageCodesAndTitles.ElementAt(i).Key);
                user2combo.Items.Add(languageCodesAndTitles.ElementAt(i).Key);
            }
        }

        private void GetLanguagesForTranslate()
        {

            string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguagesForTranslate";
            WebRequest WebRequest = WebRequest.Create(uri);
            WebRequest.Headers.Add("Authorization", tokenProvider.GetAccessToken());

            WebResponse response = null;

            try
            {
                response = WebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {

                    System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(typeof(List<string>));
                    List<string> languagesForTranslate = (List<string>)dcs.ReadObject(stream);
                    friendlyName = languagesForTranslate.ToArray(); //put the list of language codes into an array to pass to the method to get the friendly name.

                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        private void GetLanguageNamesMethod(string authToken, string[] languageCodes)
        {
            string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguageNames?locale=en";
            // create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("Authorization", tokenProvider.GetAccessToken());
            request.ContentType = "text/xml";
            request.Method = "POST";
            DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String[]"));
            using (System.IO.Stream stream = request.GetRequestStream())
            {
                dcs.WriteObject(stream, languageCodes);
            }
            WebResponse response = null;
            try
            {
                response = request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    string[] languageNames = (string[])dcs.ReadObject(stream);

                    for (int i = 0; i < languageNames.Length; i++)
                    {

                        languageCodesAndTitles.Add(languageNames[i], languageCodes[i]); //load the dictionary for the combo box

                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (user1combo.Text == "" || user2combo.Text == "")
                MessageBox.Show("Please select language for both users!");
            else
            {
                String inputText = user1Input.Text;
                //String user1Text = user1textBox.Text;
                //String user2Text = user2TextBox.Text;
                string languageCode;
                languageCodesAndTitles.TryGetValue(user2combo.Text, out languageCode);
                string uri = string.Format("http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + System.Web.HttpUtility.UrlEncode(inputText) + "&to={0}", languageCode);

                WebRequest translationWebRequest = WebRequest.Create(uri);
                translationWebRequest.Headers.Add("Authorization", tokenProvider.GetAccessToken()); //header value is the "Bearer plus the token from ADM
                WebResponse response = null;
                response = translationWebRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");
                StreamReader translatedStream = new StreamReader(stream, encode);
                System.Xml.XmlDocument xTranslation = new System.Xml.XmlDocument();

                xTranslation.LoadXml(translatedStream.ReadToEnd());
                user1textBox.Items.Add("User1: " + user1Input.Text);
                user2TextBox.Items.Add("User1: " + xTranslation.InnerText);

                String logdata = DateTime.Now.ToString() + " User1: " + user1combo.Text + ": " + user1Input.Text + " " + user2combo.Text + ": " + xTranslation.InnerText;
                LogThis(logdata);
            }
        }

        private void LogThis(String logdata)
        {
            try
            {
                if (!Directory.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, "Logs")))
                    Directory.CreateDirectory(System.IO.Path.Combine(Environment.CurrentDirectory, "Logs"));
            }
            catch { }
            Boolean bFileExist = File.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, "Logs") + @"\log.txt");
            
            using (StreamWriter writer = new StreamWriter(System.IO.Path.Combine(Environment.CurrentDirectory, "Logs") + @"\log.txt", bFileExist))
            {      
                writer.WriteLine(logdata);
            }
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            if (user1combo.Text == "" || user2combo.Text == "")
                MessageBox.Show("Please select language for both users!");
            else
            {
                String languageCode;
                String inputText = user2Input.Text;
                languageCodesAndTitles.TryGetValue(user1combo.Text, out languageCode);
                //String user1Text = user1textBox.Text;
                //String user2Text = user2TextBox.Text;
                string uri = string.Format("http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + System.Web.HttpUtility.UrlEncode(inputText) + "&to={0}", languageCode);

                WebRequest translationWebRequest = WebRequest.Create(uri);
                translationWebRequest.Headers.Add("Authorization", tokenProvider.GetAccessToken()); //header value is the "Bearer plus the token from ADM
                WebResponse response = null;
                response = translationWebRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");
                StreamReader translatedStream = new StreamReader(stream, encode);
                System.Xml.XmlDocument xTranslation = new System.Xml.XmlDocument();

                xTranslation.LoadXml(translatedStream.ReadToEnd());

                user2TextBox.Items.Add("User2: " + user2Input.Text);
                user1textBox.Items.Add("User2: " + xTranslation.InnerText);
                String logdata = DateTime.Now.ToString() + " User2: " + user2combo.Text + ": " + user2Input.Text + " " + user1combo.Text + ": " + xTranslation.InnerText;
                LogThis(logdata);
            }
        }

        private void Error_Click(object sender, RoutedEventArgs e)
        {
            String logdata = DateTime.Now.ToString() + " User found error!";
            LogThis(logdata);
            MessageBox.Show("Sorry you found an error! :( We apologize for the inconvenience. \n Error number " +  count.ToString());
            count++;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://ufl.qualtrics.com/jfe/form/SV_6tx46lVC03gjgXj");
            System.Windows.Application.Current.Shutdown();
        }
    }
}
