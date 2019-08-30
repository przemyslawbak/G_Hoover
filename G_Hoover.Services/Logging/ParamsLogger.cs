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
        Task Initialization { get; }
    }

    public class ParamsLogger : IParamsLogger, IAsyncInitialization
    {
        private Timer _savingTimer;
        AppDomain _currentDomain;

        //config variables
        private string _logFile;
        private bool _debugOnly;
        private bool _executeOnDebugSettings;
        private bool _deleteLogs;

        //config defaults
        private readonly string _logFileDefaults = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "log.txt");
        private readonly bool _debugOnlyDefaults = true;
        private readonly bool _deleteLogsDefaults = true;

        public ParamsLogger()
        {
            LogList = new List<LogModel>();

            Initialization = RunConfigAsync();
        }

        public List<LogModel> LogList { get; set; } //list of added logs
        public Task Initialization { get; set; } //async init Task

        /// <summary>
        /// configuration method, loading variables from log.config
        /// if file not found, setting up defaults
        /// </summary>
        private async Task RunConfigAsync()
        {
            _currentDomain = AppDomain.CurrentDomain;
            string path = await GetLogConfigPathAsync();

            if (string.IsNullOrEmpty(path))
            {
                _logFile = _logFileDefaults;
                _debugOnly = _debugOnlyDefaults;
                _deleteLogs = _deleteLogsDefaults;
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

            if (_deleteLogs)
            {
                DeleteFile(_logFile);
            }

            if (_executeOnDebugSettings) // if on DEBUG
            {
                await RunTimerAsync();

                UnhandledExceptionsHandler();
            }
        }

        /// <summary>
        /// gets setting for deleteLogs from log.config or returns default
        /// </summary>
        /// <param name="config">app config</param>
        /// <returns>bool value</returns>
        private bool GetDeleteLogs(Configuration config)
        {
            bool result;

            string deleteLogs = config.AppSettings.Settings["deleteLogs"].Value;
            bool isParsed = bool.TryParse(deleteLogs, out result);

            return isParsed ? Convert.ToBoolean(deleteLogs) : _deleteLogsDefaults;
        }

        /// <summary>
        /// gets setting for debugOnly from log.config or returns default
        /// </summary>
        /// <param name="config">app config</param>
        /// <returns>bool value</returns>
        private bool GetDebugOnlyPath(Configuration config)
        {
            bool result;

            string debugOnly = config.AppSettings.Settings["debugOnly"].Value;
            bool isParsed = bool.TryParse(debugOnly, out result);

            return isParsed ? Convert.ToBoolean(debugOnly) : _debugOnlyDefaults;
        }

        /// <summary>
        /// gets setting for logFile from log.config or returns default
        /// </summary>
        /// <param name="config">app config</param>
        /// <returns>string path value</returns>
        private string GetLogPath(Configuration config)
        {
            string logFile = config.AppSettings.Settings["logFile"].Value;

            return (!string.IsNullOrEmpty(logFile)) ? logFile : _logFileDefaults;
        }

        /// <summary>
        /// looking for log.config file, in the project folders
        /// file have to contain phrases: logFile, debugOnly, deleteLogs
        /// </summary>
        /// <returns>file path</returns>
        private async Task<string> GetLogConfigPathAsync()
        {
            bool ok = false;

            string newDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string[] files = Directory.GetFiles(newDirectory, "log.config", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                List<string> lines = await ReadFileLinesAsync(file);

                if (lines.Any(l => l.Contains("logFile")) && lines.Any(l => l.Contains("debugOnly")) && lines.Any(l => l.Contains("deleteLogs")))
                    ok = true;

                if (ok)
                {
                    return file;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// file reader
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<List<string>> ReadFileLinesAsync(string file)
        {
            using (var reader = File.OpenText(file))
            {
                var fileText = await reader.ReadToEndAsync();

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

            if ((!_savingTimer.Enabled || _savingTimer == null) && LogList.Count > 0 && !string.IsNullOrEmpty(_logFile))
            {
                await ProcessLogList();
            }
            _savingTimer = new Timer();
            _savingTimer.Elapsed += new ElapsedEventHandler(ResetTimerAsync);
            _savingTimer.Interval = 10;
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

        /// <summary>
        /// interface implementation
        /// can be called from property setter
        /// </summary>
        /// <param name="value">property value</param>
        /// <param name="propertyName">property name</param>
        public void Prop(object value, [CallerMemberName] string propertyName = null)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                object[] arguments = { propertyName, value };

                MethodBase callingMethod = GetMethod(new StackTrace().GetFrame(1));

                LogList.Add(new LogModel { MethodName = nameof(Prop), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        /// <summary>
        /// interface implementation
        /// can be called from methods
        /// </summary>
        /// <param name="arguments">array of passed arguments</param>
        public void Called(params object[] arguments)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                MethodBase callingMethod = GetMethod(new StackTrace().GetFrame(1));

                LogList.Add(new LogModel { MethodName = nameof(Called), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        /// <summary>
        /// interface implementation
        /// can be called from methods
        /// </summary>
        /// <param name="arguments">array of passed arguments</param>
        public void Ended(params object[] arguments)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                MethodBase callingMethod = GetMethod(new StackTrace().GetFrame(1));

                LogList.Add(new LogModel { MethodName = nameof(Ended), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        /// <summary>
        /// interface implementation
        /// can be called to pass information about some app event
        /// </summary>
        /// <param name="arguments">string info</param>
        public void Info(string value)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                object[] arguments = { value };

                MethodBase callingMethod = GetMethod(new StackTrace().GetFrame(1));

                LogList.Add(new LogModel { MethodName = nameof(Info), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        /// <summary>
        /// interface implementation
        /// can be called from catch to pass information about exception
        /// </summary>
        /// <param name="arguments">string info</param>
        public void Error(string value)
        {
            if (_executeOnDebugSettings) // if on DEBUG
            {
                DateTime date = DateTime.Now;

                object[] arguments = { value };

                MethodBase callingMethod = GetMethod(new StackTrace().GetFrame(1));

                LogList.Add(new LogModel { MethodName = nameof(Error), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        private MethodBase GetMethod(StackFrame frame)
        {
            MethodBase callingMethod = frame.GetMethod();

            if (callingMethod != null && (callingMethod.Name == "MoveNext" || callingMethod.Name == "Run"))
            {
                callingMethod = GetRealMethodFromAsyncMethod(callingMethod);
            }

            return callingMethod;
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
