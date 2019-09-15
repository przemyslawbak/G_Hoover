using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using G_Hoover.Services.Config;
using G_Hoover.Services.Files;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;
using Params_Logger;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace G_Hoover.Services.Audio
{
    public class AudioService : IAudioService
    {
        private readonly IFilingService _fileService;
        private readonly IAppConfig _config;
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();


        private WasapiCapture _capture;
        private WaveWriter _writer;
        private readonly Timer _recordFileTimer;

        public AudioService(IFilingService fileService, IAppConfig config)
        {
            _fileService = fileService;
            _config = config;

            _recordFileTimer = new Timer();
        }

        public async Task RecordAudioSampleAsync()
        {
            _log.Called();

            using (_capture = new WasapiLoopbackCapture())
            {
                _recordFileTimer.Elapsed += new ElapsedEventHandler(OnFinishedRecordingEvent);
                _recordFileTimer.Interval = 8000; //recording period

                try
                {
                    _capture.Initialize();
                    _writer = CreateNewWaveWriter(_capture); //need to have _capture initialized

                    if (_writer != null)
                    {
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
                    else
                    {
                        throw new Exception("Could not create audio writer.");
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message);
                }
            }
        }

        private void OnFinishedRecordingEvent(object sender, ElapsedEventArgs e)
        {
            _log.Called(sender.ToString(), string.Empty);

            try
            {
                _capture.Stop(); //stop recording

                _recordFileTimer.Enabled = false; //timer disabled

                _writer.Dispose();
            }
            catch
            {
                _log.Error("Problems when finishing recording.");
            }
        }

        public async Task<string> ProcessAudioSampleAsync()
        {
            _log.Called();

            string key = _config.AudioApiKey;
            string region = _config.AudioApiRegion;
            SpeechConfig configRecognizer = SpeechConfig.FromSubscription(key, region);
            string processedAudio = "";
            string audioFile = _fileService.GetAudioFilePath();
            bool isRecorded = _fileService.CheckAudioFile();

            if (!isRecorded)
                return processedAudio; //if file empty

            try
            {
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

                        _log.Info("Start recording.");
                    };

                    recognizer.SessionStopped += (s, e) => {

                        _log.Info("Stop recording.");

                        stopRecognition.TrySetResult(0);
                    };

                    await recognizer.StartContinuousRecognitionAsync();

                    Task.WaitAny(new[] { stopRecognition.Task });

                    await recognizer.StopContinuousRecognitionAsync();
                }

                return processedAudio;
            }
            catch (Exception e)
            {
                _log.Error(e.Message);

                return string.Empty;
            }
        }

        public WaveWriter CreateNewWaveWriter(WasapiCapture capture)
        {
            _log.Called(string.Empty);

            try
            {
                string audioFile = _fileService.GetAudioFilePath();

                _fileService.DeleteOldAudio();

                WaveWriter writer = new WaveWriter(audioFile, capture.WaveFormat);

                return writer;
            }
            catch (Exception e)
            {
                _log.Error(e.Message);

                return null;
            }
        }
    }
}
