using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.CoreAudioApi;

namespace RealTimeClosedCaptioning2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string lastLine = string.Empty;

        SpeechRecognizer recognizer = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in
                     enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                Microphone.Items.Add(new ComboBoxItem() { Content = endpoint.FriendlyName, Tag = endpoint.ID });
            }
            
        }

        private void Recognizer_SpeechStartDetected(object sender, RecognitionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBlock.Text = $"Speech Start: {e.SessionId}";
            });
        }

        private void Recognizer_SpeechEndDetected(object sender, RecognitionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBlock.Text = $"Speech End: {e.SessionId}";
            });
        }

        private void Recognizer_SessionStopped(object sender, SessionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBlock.Text = $"Session Stopped: {e.SessionId}";
            });
        }

        private void Recognizer_SessionStarted(object sender, SessionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBlock.Text = $"Session Started: {e.SessionId}";
            });
        }

        private void Recognizer_Canceled(object sender, SpeechRecognitionCanceledEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBlock.Text = $"Canceled: {e.Result.Reason}";
            });
        }

        private void Recognizer_Recognizing(object sender, SpeechRecognitionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBlock.Text = $"{lastLine}\n{e.Result.Text.ToUpper()}";
            });
        }

        private void Recognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(e.Result.Text))
                {
                    lastLine = e.Result.Text.ToUpper();
                    MessageBlock.Text = $"{lastLine}";
                }
            });
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (recognizer != null)
                await recognizer.StopContinuousRecognitionAsync();
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            var config = SpeechConfig.FromSubscription(SpeechKey.Text, SpeechRegion.Text);
            //var exe = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //config.SetProperty(PropertyId.Speech_LogFilename, System.IO.Path.Combine(exe.DirectoryName, "log.txt"));
            recognizer = new SpeechRecognizer(config, AudioConfig.FromMicrophoneInput((Microphone.SelectedItem as ComboBoxItem).Tag.ToString()));

            recognizer.Recognizing += Recognizer_Recognizing;
            recognizer.Recognized += Recognizer_Recognized;
            //recognizer.Canceled += Recognizer_Canceled;
            //recognizer.SessionStarted += Recognizer_SessionStarted;
            //recognizer.SessionStopped += Recognizer_SessionStopped;
            //recognizer.SpeechEndDetected += Recognizer_SpeechEndDetected;
            //recognizer.SpeechStartDetected += Recognizer_SpeechStartDetected;

            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            Dispatcher.Invoke(() =>
            {
                MessageBlock.Text = "Ready!";
                Controls.Visibility = Visibility.Hidden;
            });
        }

        private void Hidden_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Controls.Visibility == Visibility.Visible)
                Controls.Visibility = Visibility.Hidden;
            else
                Controls.Visibility = Visibility.Visible;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
