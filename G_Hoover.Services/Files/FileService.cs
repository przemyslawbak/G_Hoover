﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using G_Hoover.Services.Messages;
using System.Text;
using G_Hoover.Models;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using System.Text.RegularExpressions;

namespace G_Hoover.Services.Files
{
    public class FileService : IFileService
    {
        private readonly string _logFile = "../../../../log.txt";
        private readonly string _resultFile = "../../../../result.txt";
        private readonly string _audioFile = "../../../../dump.wav";

        public FileService()
        {

        }

        public async Task<List<string>> GetNewListFromFileAsync(string filePath)
        {
            List<string> list = new List<string>();

            //_logger.Info(MessagesInfo[callerName] + filePath); //log

            try
            {
                using (var reader = File.OpenText(filePath))
                {
                    var fileText = await reader.ReadToEndAsync();
                    string[] array = fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    foreach (string line in array)
                    {
                        list.Add(line);
                    }
                }

                //_logger.Info(MessagesResult[callerName] + list.Count); //log

                return list;
            }
            catch (Exception e)
            {
                //_logger.Error(MessagesError[callerName] + e.Message); //log

                list.Clear();

                return list;
            }
        }

        public void RemoveOldLogs()
        {
            //_logger.Info(MessagesInfo[callerName]); //log

            try
            {
                if (File.Exists(_logFile))
                {
                    File.Delete(_logFile);
                }

                //_logger.Info(MessagesResult[callerName]); //log
            }
            catch (Exception e)
            {
                //_logger.Error(MessagesError[callerName] + e.Message); //log
            }
        }

        public async Task SaveNewResultAsync(ResultObjectModel result, string phrase)
        {
            string stringResult = CombineStringResult(result, phrase);

            using (TextWriter LineBuilder = new StreamWriter(_resultFile, true))
            {
                await LineBuilder.WriteLineAsync(stringResult);
            }
        }

        public string CombineStringResult(ResultObjectModel result, string phrase)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(phrase);
            sb.Append("|");
            sb.Append(result.Header);
            sb.Append("|");
            sb.Append(result.Url);

            return sb.ToString();
        }

        public WaveWriter CreateNewWaveWriter(WasapiCapture capture)
        {
            if (File.Exists(_audioFile))
            {
                File.Delete(_audioFile);
            }

            WaveWriter writer = new WaveWriter(_audioFile, capture.WaveFormat);

            return writer;
        }

        public bool CheckAudioFile()
        {
            return (new FileInfo(_audioFile).Length > 10000) ? true : false;
        }

        public string GetAudioFilePath()
        {
            return _audioFile;
        }

        public string ProsessText(string audioResult)
        {
            return Regex.Replace(audioResult.ToLower(), @"[.?!,]", "");
        }

        public void DeleteResultsFile()
        {
            if (File.Exists(_resultFile))
            {
                File.Delete(_resultFile);
            }
        }

        public async Task SaveLogAsync(string line)
        {
            using (TextWriter LineBuilder = new StreamWriter(_logFile, true))
            {
                await LineBuilder.WriteLineAsync(line);
            }
        }
    }
}
