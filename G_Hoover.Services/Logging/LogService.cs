using G_Hoover.Services.Files;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace G_Hoover.Services.Logging
{
    public class LogService : ILogService
    {
        private readonly IFileService _fileService;

        public LogService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public void Called(params object[] values)
        {
            GetStringAttributes(nameof(Called), values, DateTime.Now);
        }

        public void Ended(params object[] values)
        {
            GetStringAttributes(nameof(Ended), values, DateTime.Now);
        }

        public void Info(params object[] values)
        {
            GetStringAttributes(nameof(Info), values, DateTime.Now);
        }

        public void Error(params object[] values)
        {
            GetStringAttributes(nameof(Error), values, DateTime.Now);
        }

        private void GetStringAttributes(string eventType, object[] values, DateTime date)
        {
            MethodBase callingMethod = new StackTrace().GetFrame(2).GetMethod();

            if (callingMethod.Name == "MoveNext" || callingMethod.Name == "Run")
            {
                callingMethod = GetRealMethodFromAsyncMethod(callingMethod);
            }

            string methodName = callingMethod.Name;

            var className = callingMethod.ReflectedType.Name;

            var parameters = callingMethod.GetParameters();

            string type = GetEventType(eventType);

            string line = BuildLine(date, type, className, methodName, parameters, values);

            SaveLog(line);
        }

        public void SaveLog(string line)
        {
            _fileService.SaveLogAsync(line);
        }

        //not possible to get argument variables with reflection, best way is to use 'nameof': https://stackoverflow.com/a/2566177/11972985
        //not possible to get parameter values with reflection: https://stackoverflow.com/a/1867496/11972985

        private string BuildLine(DateTime date, string type, string className, string methodName, ParameterInfo[] parameters, object[] values)
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
            sb.Append("(");
            if (parameters.Length > 0)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    sb.Append(parameters[i].ParameterType.Name);
                    sb.Append(" ");
                    sb.Append(parameters[i].Name);
                    if (i < parameters.Length - 1)
                        sb.Append(", ");
                }
            }
            sb.Append(")");
            sb.Append("|");
            if (values.Length == 0)
                sb.Append("none");
            else
            {
                for (var i = 0; i < values.Length; i++)
                {
                    sb.Append(values[i].GetType().Name);
                    sb.Append("=");
                    sb.Append(values[i].ToString());
                    sb.Append("; ");
                }
            }

            return sb.ToString();
        }

        public string GetEventType(string eventType)
        {
            if (eventType == "Call")
            {
                return "CALLED";
            }
            else if (eventType == "End")
            {
                return "ENDED";
            }
            else if (eventType == "Info")
            {
                return "INFO";
            }
            else
            {
                return "ERROR";
            }
        }

        public static MethodBase GetRealMethodFromAsyncMethod(MethodBase asyncMethod)
        {
            var generatedType = asyncMethod.DeclaringType;
            var originalType = generatedType.DeclaringType;
            var matchingMethods =
                from methodInfo in originalType.GetMethods()
                let attr = methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>()
                where attr != null && attr.StateMachineType == generatedType
                select methodInfo;

            // If this throws, the async method scanning failed.
            var foundMethod = matchingMethods.Single();
            return foundMethod;
        }
    }
}
