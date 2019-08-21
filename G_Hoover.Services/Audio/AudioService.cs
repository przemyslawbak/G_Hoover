using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using G_Hoover.Services.Config;
using G_Hoover.Services.Files;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace G_Hoover.Services.Audio
{
    public class AudioService : IAudioService
    {
        private WasapiCapture _capture;
        private WaveWriter _writer;
        private readonly IFileService _fileService;
        private readonly AppConfig _config;
        private readonly Timer _recordFileTimer;
        private readonly string _audioFile = "../../../../dump.wav";

        public AudioService(IFileService fileService)
        {
            _fileService = fileService;
            _config = new AppConfig();
            _recordFileTimer = new Timer();
        }

        public async Task RecordAudioSample()
        {
            _capture = new WasapiLoopbackCapture();
            _writer = _fileService.CreateNewWaveWriter(_capture);

            _recordFileTimer.Elapsed += new ElapsedEventHandler(OnFinishedRecordingEventAsync);
            _recordFileTimer.Interval = 8000; //recording period

            try
            {
                _capture.Initialize();
                _capture.DataAvailable += (s, e) =>
                {
                   _writer.Write(e.Data, e.Offset, e.ByteCount); //saving recorded audio sample
                };

                _capture.Start(); //start recording

                _recordFileTimer.Enabled = true;
            }
            catch
            {
                //log
            }
        }

        private async void OnFinishedRecordingEventAsync(object sender, ElapsedEventArgs e)
        {
            _capture.Stop(); //stop recording

            _recordFileTimer.Enabled = false; //timer disabled

            _writer.Dispose();
            _capture.Dispose();

            await ProcessAudioAsync();
        }

        public async Task ProcessAudioAsync()
        {
            string key = _config.AudioApiKey;
            string region = _config.AudioApiRegion;

            //klucz
            var config = SpeechConfig.FromSubscription(key, region);
            string processedAudio = "";

            bool isRecorded = _fileService.CheckAudioFile();

            if (isRecorded)
            {
                using (var audioInput = AudioConfig.FromWavFileInput("dump.wav"))
                {
                    using (var recognizer = new IntentRecognizer(config, audioInput))
                    {
                        // nie korzystam
                        var stopRecognition = new TaskCompletionSource<int>();

                        // nie korzystam
                        var model = LanguageUnderstandingModel.FromAppId("YourLanguageUnderstandingAppId");
                        recognizer.AddIntent(model, "YourLanguageUnderstandingIntentName1", "id1");
                        recognizer.AddIntent(model, "YourLanguageUnderstandingIntentName2", "id2");
                        recognizer.AddIntent(model, "YourLanguageUnderstandingIntentName3", "any-IntentId-here");

                        recognizer.Recognizing += (s, e) => {
                            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                        };

                        recognizer.Recognized += (s, e) => {
                            if (e.Result.Reason == ResultReason.RecognizedIntent) //nie korzystam
                            {
                                processedAudio = e.Result.Text;
                                if (processedAudio != "")
                                {
                                    WyslijWynikAsync(processedAudio, poprawkaMyszY);
                                }
                                else
                                {
                                    //nie korzystam
                                }
                            }
                            else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                processedAudio = e.Result.Text;
                                if (processedAudio != "")
                                {
                                    WyslijWynikAsync(processedAudio, poprawkaMyszY);
                                }
                                else
                                {
                                    WybierzNoweAudioAsync(poprawkaMyszY);
                                }
                            }
                            else if (e.Result.Reason == ResultReason.NoMatch)
                            {
                                if (processedAudio == "")
                                {
                                    WybierzNoweAudioAsync(poprawkaMyszY);
                                }
                            }
                        };

                        recognizer.Canceled += (s, e) => {
                            if (e.Reason == CancellationReason.Error)
                            {
                                //zostawić
                            }
                            stopRecognition.TrySetResult(0);
                        };

                        recognizer.SessionStarted += (s, e) => {

                        };

                        recognizer.SessionStopped += (s, e) => {

                            stopRecognition.TrySetResult(0);
                        };

                        await recognizer.StartContinuousRecognitionAsync();

                        Task.WaitAny(new[] { stopRecognition.Task });

                        await recognizer.StopContinuousRecognitionAsync();
                    }
                }
            }
            else
            {
                DodajZdarzenieLog("audio się nie nagrało");
                if (searchViaTor)
                {
                    DodajZdarzenieLog("zmienię IP");
                    await PoraZmienicIpAsync();
                }
                else
                {
                    DodajZdarzenieLog("przejdę na sieć Tor");
                    await ConfigureBrowserTorAsync();
                    KontynuacjaSkrapowania();
                }
            }
        }
    }
}
