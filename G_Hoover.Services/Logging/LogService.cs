using System;
using System.Collections.Generic;
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

    public class LogService : ILogService
    {
        private Timer _savingTimer;
        AppDomain _currentDomain;
        private readonly string _logFile = "../../../../log.txt";

        public LogService()
        {
            _currentDomain = AppDomain.CurrentDomain;

            LogList = new List<LogModel>();

            if (Debugger.IsAttached) // only for DEBUG
            {
                Initialization = RunTimerAsync();

                UnhandledExceptionsHandlerAsync();
            }
        }

        private async void UnhandledExceptionsHandlerAsync()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

            await ProcessLogList();
        }

        void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Error(string.Format("Runtime terminating: {0}", args.IsTerminating));
        }

        private async Task RunTimerAsync()
        {
            _savingTimer = new Timer();

            if (!_savingTimer.Enabled || _savingTimer == null)
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

        public List<LogModel> LogList { get; set; }
        public Task Initialization { get; private set; } //for Asynchronous Initialization Pattern

        public void Prop(object value, [CallerMemberName] string propertyName = null)
        {
            if (Debugger.IsAttached) // only for DEBUG
            {
                DateTime date = DateTime.Now;

                object[] arguments = { propertyName, value };

                var frames = new StackTrace().GetFrames();

                MethodBase callingMethod = GetCallingMethod(frames);

                LogList.Add(new LogModel { MethodName = nameof(Prop), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        public void Called(params object[] arguments)
        {
            if (Debugger.IsAttached) // only for DEBUG
            {
                DateTime date = DateTime.Now;

                var frames = new StackTrace().GetFrames();

                MethodBase callingMethod = GetCallingMethod(frames);

                LogList.Add(new LogModel { MethodName = nameof(Called), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        public void Ended(params object[] arguments)
        {
            if (Debugger.IsAttached) // only for DEBUG
            {
                DateTime date = DateTime.Now;

                var frames = new StackTrace().GetFrames();

                MethodBase callingMethod = GetCallingMethod(frames);

                LogList.Add(new LogModel { MethodName = nameof(Ended), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        public void Info(string value)
        {
            if (Debugger.IsAttached) // only for DEBUG
            {
                DateTime date = DateTime.Now;

                object[] arguments = { value };

                var frames = new StackTrace().GetFrames();

                MethodBase callingMethod = GetCallingMethod(frames);

                LogList.Add(new LogModel { MethodName = nameof(Info), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        public void Error(string value)
        {
            if (Debugger.IsAttached) // only for DEBUG
            {
                DateTime date = DateTime.Now;

                object[] arguments = { value };

                var frames = new StackTrace().GetFrames();

                MethodBase callingMethod = GetCallingMethod(frames);

                LogList.Add(new LogModel { MethodName = nameof(Error), Arguments = arguments, Date = date, Method = callingMethod });
            }
        }

        private MethodBase GetCallingMethod(StackFrame[] frames)
        {
            MethodBase callingMethod;

            if (frames.Length > 1)
            {
                callingMethod = new StackTrace().GetFrame(1).GetMethod();
            }
            else
            {
                callingMethod = null;
            }

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
    }
}
