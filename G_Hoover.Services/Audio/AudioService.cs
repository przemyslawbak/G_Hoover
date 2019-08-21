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

        public AudioService(IFileService fileService)
        {
            _fileService = fileService;
            _config = new AppConfig();
            _recordFileTimer = new Timer();
        }

        public async Task RecordAudioSampleAsync()
        {
            _capture = new WasapiLoopbackCapture();

            _recordFileTimer.Elapsed += new ElapsedEventHandler(OnFinishedRecordingEvent);
            _recordFileTimer.Interval = 8000; //recording period

            try
            {
                _capture.Initialize();
                _writer = _fileService.CreateNewWaveWriter(_capture); //need to have _capture initialized
                _capture.DataAvailable += (s, e) =>
                {
                    _writer.Write(e.Data, e.Offset, e.ByteCount); //saving recorded audio sample
                };

                _capture.Start(); //start recording

                _recordFileTimer.Enabled = true;

                while (_recordFileTimer.Enabled)
                {
                    await Task.Delay(100);
                }
            }
            catch
            {
                //log
            }
        }

        private void OnFinishedRecordingEvent(object sender, ElapsedEventArgs e)
        {
            _capture.Stop(); //stop recording

            _recordFileTimer.Enabled = false; //timer disabled

            _writer.Dispose();
            _capture.Dispose();
        }

        public async Task<string> ProcessAudioSampleAsync()
        {
            string key = _config.AudioApiKey;
            string region = _config.AudioApiRegion;
            SpeechConfig configRecognizer = SpeechConfig.FromSubscription(key, region);
            string processedAudio = "";
            string audioFile = _fileService.GetAudioFilePath();

            bool isRecorded = _fileService.CheckAudioFile();

            if (!isRecorded)
                return processedAudio; //if file empty

            using (AudioConfig audioInput = AudioConfig.FromWavFileInput(audioFile))
            using (IntentRecognizer recognizer = new IntentRecognizer(configRecognizer, audioInput))
            {
                TaskCompletionSource<int> stopRecognition = new TaskCompletionSource<int>();

                recognizer.Recognized += (s, e) =>
                {

                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        processedAudio = e.Result.Text;
                    }
                };

                recognizer.Canceled += (s, e) => {
                    if (e.Reason == CancellationReason.Error)
                    {
                        //log
                    }
                    stopRecognition.TrySetResult(0);
                };

                recognizer.SessionStarted += (s, e) => {

                    //log
                };

                recognizer.SessionStopped += (s, e) => {

                    //log

                    stopRecognition.TrySetResult(0);
                };

                await recognizer.StartContinuousRecognitionAsync();

                Task.WaitAny(new[] { stopRecognition.Task });

                await recognizer.StopContinuousRecognitionAsync();
            }

            //log

            return processedAudio;
        }
    }
}
