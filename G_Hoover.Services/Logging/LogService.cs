using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace G_Hoover.Services.Logging
{
    public class LogModel
    {
        public string MethodName { get; set; }
        public object[] Arguments { get; set; }
        public DateTime Date { get; set; }
        public MethodBase Method { get; set; }
    }
    public interface IAsyncInitialization
    {
        Task TimerInitialization { get; }
    }

    public class LogService : ILogService
    {
        private Timer _savingTimer;
        AppDomain _currentDomain;
        private readonly string _logFileDefaults = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "log.txt");
        private readonly bool _debugOnlyDefaults = true;
        private string _logFile;
        private bool _debugOnly;
        private bool _executeOnDebugSettings;
        private bool _deleteLogs;

        public LogService()
        {
            _currentDomain = AppDomain.CurrentDomain;

            LogList = new List<LogModel>();

            RunConfig();

            if (_deleteLogs)
            {
                DeleteFile(_logFile);
            }

            if (_executeOnDebugSettings) // if on DEBUG
            {
                TimerInitialization = RunTimerAsync();

                UnhandledExceptionsHandler();
            }
        }

        public List<LogModel> LogList { get; set; }
        public Task TimerInitialization { get; private set; }
        public Task ConfigInitialization { get; private set; }

        private void RunConfig()
        {
            string path = GetLogConfigPath();

            if (string.IsNullOrEmpty(path)) //defaults
            {
                _logFile = _logFileDefaults;
                _debugOnly = _debugOnlyDefaults;
            }
            else
            {
                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
                configMap.ExeConfigFilename = path;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

                _logFile = GetLogPath(config);
                _debugOnly = GetDebugOnlyPath(config);
                _deleteLogs = GetDeleteLogs(config);
            }

            if (!_debugOnly || (_debugOnly && Debugger.IsAttached))
            {
                _executeOnDebugSettings = true;
            }
            else
            {
                _executeOnDebugSettings = false;
            }
        }

        private bool GetDeleteLogs(Configuration config)
        {
            bool result;

            string debugOnly = config.AppSettings.Settings["deleteLogs"].Value;
            bool isParsed = bool.TryParse(debugOnly, out result);

            return isParsed ? Convert.ToBoolean(debugOnly) : true;
        }

        private bool GetDebugOnlyPath(Configuration config)
        {
            bool result;

            string debugOnly = config.AppSettings.Settings["debugOnly"].Value;
            bool isParsed = bool.TryParse(debugOnly, out result);

            return isParsed ? Convert.ToBoolean(debugOnly) : true;
        }

        private string GetLogPath(Configuration config)
        {
            string logFile = config.AppSettings.Settings["logFile"].Value;

            return (!string.IsNullOrEmpty(logFile)) ? logFile : _logFileDefaults;
        }

        private string GetLogConfigPath()
        {
            bool ok = false;

            string newDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string[] files = Directory.GetFiles(newDirectory, "log.config", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                List<string> lines = ReadFileLines(file);

                if (lines.Any(l => l.Contains("logFile")) && lines.Any(l => l.Contains("debugOnly")))
                    ok = true;

                if (ok)
                {
                    return file;
                }
            }

            return string.Empty;
        }

        private List<string> ReadFileLines(string file)
        {
            using (var reader = File.OpenText(file))
            {
                var fileText = reader.ReadToEnd();

                var array = fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                return new List<string>(array);
            }
        }

        private void UnhandledExceptionsHandler()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledExceptionAsync);
        }

        async void OnUnhandledExceptionAsync(object sender, UnhandledExceptionEventArgs args)
        {
            Error(string.Format("Runtime terminating: {0}", args.IsTerminating));

            await ProcessLogList();
        }

        private async Task RunTimerAsync()
        {
            _savingTimer = new Timer();

            if ((!_savingTimer.Enabled || _savingTimer == null) && LogList.Count > 0)
            {
                await ProcessLogList();
            }
            _savingTimer = new Timer();
            _savingTimer.Elapsed += new ElapsedEventHandler(ResetTimerAsync);
            _savingTimer.Interval = 20;
            _savingTimer.Start();
        }

        private async void ResetTimerAsync(object sender, EventArgs e)
        {
            _savingTimer.Enabled = false;

            await RunTimerAsync();
        }

        private async Task ProcessLogList()
        {
            List<LogModel> iterateMe = LogList.ToList();
            LogList.Clear();

            foreach (LogModel item in iterateMe)
            {
                await GetStringAttributesAsync(item);
            }
        }

        public void Prop(object value, [CallerMemberName] string propertyName = null)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                object[] arguments = { propertyName, value };

                MethodBase callingMethod = new StackTrace().GetFrame(1).GetMethod();

                if (callingMethod != null && (callingMethod.Name == "MoveNext" || callingMethod.Name == "Run"))
                {
                    callingMethod = GetRealMethodFromAsyncMethod(callingMethod);
                }

                LogList.Add(new LogModel { MethodName = nameof(Prop), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        public void Called(params object[] arguments)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                var frames = new StackTrace().GetFrames();

                MethodBase callingMethod = new StackTrace().GetFrame(1).GetMethod();

                if (callingMethod != null && (callingMethod.Name == "MoveNext" || callingMethod.Name == "Run"))
                {
                    callingMethod = GetRealMethodFromAsyncMethod(callingMethod);
                }

                LogList.Add(new LogModel { MethodName = nameof(Called), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        public void Ended(params object[] arguments)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                var frames = new StackTrace().GetFrames();

                MethodBase callingMethod = new StackTrace().GetFrame(1).GetMethod();

                if (callingMethod != null && (callingMethod.Name == "MoveNext" || callingMethod.Name == "Run"))
                {
                    callingMethod = GetRealMethodFromAsyncMethod(callingMethod);
                }

                LogList.Add(new LogModel { MethodName = nameof(Ended), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        public void Info(string value)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                object[] arguments = { value };

                var frames = new StackTrace().GetFrames();

                MethodBase callingMethod = new StackTrace().GetFrame(1).GetMethod();

                LogList.Add(new LogModel { MethodName = nameof(Info), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        public void Error(string value)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                object[] arguments = { value };

                var frames = new StackTrace().GetFrames();

                MethodBase callingMethod = new StackTrace().GetFrame(1).GetMethod();

                if (callingMethod != null && (callingMethod.Name == "MoveNext" || callingMethod.Name == "Run"))
                {
                    callingMethod = GetRealMethodFromAsyncMethod(callingMethod);
                }

                LogList.Add(new LogModel { MethodName = nameof(Error), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        private async Task GetStringAttributesAsync(LogModel log)
        {
            string methodName = string.Empty;
            string className = string.Empty;
            ParameterInfo[] parameters = { };

            if (log.Method != null && log.Method.ReflectedType != null)
            {
                methodName = log.Method.Name;
                className = log.Method.ReflectedType.Name;
                parameters = log.Method.GetParameters();
            }

            log.MethodName = log.MethodName.ToUpper();

            string line = BuildLine(log.Date, log.MethodName, className, methodName, parameters, log.Arguments);

            await SaveLogAsync(line);

        }

        //NOTE: not possible to get argument variables with reflection, best way is to use 'nameof': https://stackoverflow.com/a/2566177/11972985
        //NOTE: not possible to get parameter values with reflection: https://stackoverflow.com/a/1867496/11972985

        private string BuildLine(DateTime date, string type, string className, string methodName, ParameterInfo[] parameters, object[] arguments)
        {
            bool areParams = parameters.Length > 0;
            bool combineParamsArgs = parameters.Length == arguments.Length;
            bool areArgs = arguments.Length > 0;
            bool noArgs = arguments.Length == 0;
            bool typeProps = type == "PROP";
            bool typeCalled = type == "CALLED";
            bool typeInfo = type == "INFO";
            bool typeError = type == "ERROR";
            bool typeOther = !combineParamsArgs && !typeProps && !typeCalled && !typeInfo;

            string begin = "";
            string param = "";
            string separator = "";
            string argum = "";

            begin = BuildBegin(date, type, className, methodName);

            if (typeCalled && areParams && combineParamsArgs) //CALLED
            {
                param = BuildCalled(parameters, arguments);
            }
            else if (typeCalled && !areParams) //CALLED
            {
                param = "()";
            }
            else if (typeProps) //PROP
            {
                param = BuildProp(arguments);
            }
            else if (typeInfo || typeError) //INFO & ERROR
            {
                param = BuildInfoError(arguments);
            }
            else if (areParams && !combineParamsArgs && !typeCalled) //all OTHER
            {
                param = BuildOther(arguments, parameters);
            }

            if (!typeCalled && !typeProps && !typeInfo && !typeError)
            {
                separator = "|";
            }

            if (areArgs && typeOther) //all OTHER
            {
                argum = BuildArguments(arguments);
            }
            else if (noArgs && typeOther) //none
            {
                argum = "none";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(begin);
            sb.Append(param);
            sb.Append(separator);
            sb.Append(argum);

            return sb.ToString();
        }

        private string BuildArguments(object[] arguments)
        {
            StringBuilder sb = new StringBuilder();

            for (var i = 0; i < arguments.Length; i++)
            {
                sb.Append("(");
                sb.Append(arguments[i].GetType().Name);
                sb.Append(")");
                sb.Append("=");
                sb.Append(arguments[i].ToString());
                if (i < arguments.Length - 1)
                    sb.Append("; ");
            }

            return sb.ToString();
        }

        private string BuildOther(object[] arguments, ParameterInfo[] parameters)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("(");
            for (var i = 0; i < parameters.Length; i++)
            {
                sb.Append(parameters[i].ParameterType.Name);
                sb.Append(" ");
                sb.Append(parameters[i].Name);
                if (i < parameters.Length - 1)
                    sb.Append(", ");
            }
            sb.Append(")");

            return sb.ToString();
        }

        private string BuildInfoError(object[] arguments)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            sb.Append("(");
            sb.Append(arguments[0]);
            sb.Append(")");
            sb.Append(")");

            return sb.ToString();
        }

        private string BuildProp(object[] arguments)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            sb.Append(arguments[1]);
            sb.Append(")");

            return sb.ToString();
        }

        private string BuildCalled(ParameterInfo[] parameters, object[] arguments)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            for (var i = 0; i < parameters.Length; i++)
            {
                sb.Append("(");
                sb.Append(parameters[i].ParameterType.Name);
                sb.Append(")");
                sb.Append(parameters[i].Name);
                if (!string.IsNullOrEmpty(arguments[i].ToString()))
                    sb.Append("=");
                sb.Append(arguments[i].ToString());
                if (i < parameters.Length - 1)
                    sb.Append(", ");
            }
            sb.Append(")");

            return sb.ToString();
        }

        private string BuildBegin(DateTime date, string type, string className, string methodName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(date.ToLongTimeString());
            sb.Append(".");
            sb.Append(date.Millisecond.ToString());
            sb.Append("|");
            sb.Append(type);
            sb.Append("|");
            sb.Append(className);
            sb.Append("|");
            sb.Append(methodName);

            return sb.ToString();
        }

        private static MethodBase GetRealMethodFromAsyncMethod(MethodBase asyncMethod)
        {
            try
            {
                var generatedType = asyncMethod.DeclaringType;
                var originalType = generatedType.DeclaringType;
                var matchingMethods =
                    from methodInfo in originalType.GetMethods()
                    let attr = methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>()
                    where attr != null && attr.StateMachineType == generatedType
                    select methodInfo;

                // If this throws, the async method scanning failed.
                MethodInfo foundMethod = matchingMethods.Single();
                return foundMethod;
            }
            catch
            {
                return null;
            }
        }

        private async Task SaveLogAsync(string line) //DO NOT LOG -> makes endless loop!
        {
            try
            {
                using (TextWriter LineBuilder = new StreamWriter(_logFile, true))
                {
                    await LineBuilder.WriteLineAsync(line);
                }
            }
            catch
            {
                await Task.Delay(100);
                await SaveLogAsync(line + " <--DELAYED");
            }
        }

        private void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
