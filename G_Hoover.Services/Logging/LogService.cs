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

        public void Prop(string name, object value)
        {
            object[] arguments = { name, value };

            GetStringAttributes(nameof(Prop), arguments, DateTime.Now);
        }

        public void Called(params object[] arguments)
        {
            GetStringAttributes(nameof(Called), arguments, DateTime.Now);
        }

        public void Ended(params object[] arguments)
        {
            GetStringAttributes(nameof(Ended), arguments, DateTime.Now);
        }

        public void Info(params object[] arguments)
        {
            GetStringAttributes(nameof(Info), arguments, DateTime.Now);
        }

        public void Error(params object[] arguments)
        {
            GetStringAttributes(nameof(Error), arguments, DateTime.Now);
        }

        private void GetStringAttributes(string eventType, object[] arguments, DateTime date)
        {
            string methodName = string.Empty;
            string className = string.Empty;
            ParameterInfo[] parameters = { };

           MethodBase callingMethod = new StackTrace().GetFrame(2).GetMethod();

            if (callingMethod.Name == "MoveNext" || callingMethod.Name == "Run")
            {
                callingMethod = GetRealMethodFromAsyncMethod(callingMethod);
            }
            if (callingMethod != null)
            {
                methodName = callingMethod.Name;
                className = callingMethod.ReflectedType.Name;
                parameters = callingMethod.GetParameters();
            }

            string type = GetEventType(eventType);
            string line = BuildLine(date, type, className, methodName, parameters, arguments);

            SaveLog(line);
        }

        public void SaveLog(string line)
        {
            _fileService.SaveLogAsync(line);
        }

        //not possible to get argument variables with reflection, best way is to use 'nameof': https://stackoverflow.com/a/2566177/11972985
        //not possible to get parameter values with reflection: https://stackoverflow.com/a/1867496/11972985

        private string BuildLine(DateTime date, string type, string className, string methodName, ParameterInfo[] parameters, object[] arguments)
        {
            bool areParams = parameters.Length > 0;
            bool combineParamsArgs = parameters.Length == arguments.Length;
            bool areArgs = arguments.Length > 0;
            bool noArgs = arguments.Length == 0;
            bool typeProps = type == "PROP";
            bool typeCalled = type == "CALLED";

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

            if (areParams && combineParamsArgs && typeCalled) //called
            {
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
            }
            else if (areArgs && typeProps) //properties
            {
                sb.Append(arguments[0].ToString());
                sb.Append("=");
                sb.Append(arguments[1].ToString());
            }
            else if (areParams && !combineParamsArgs && !typeCalled) //all other
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
            if (!typeCalled && !typeProps)
                sb.Append("|");

            if (areArgs && !combineParamsArgs && !typeProps)
            {
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
            }
            else if (noArgs && !typeCalled && !typeProps)
                sb.Append("none");

            return sb.ToString();
        }

        public string GetEventType(string eventType)
        {
            if (eventType == "Called")
            {
                return "CALLED";
            }
            else if (eventType == "Ended")
            {
                return "ENDED";
            }
            else if (eventType == "Info")
            {
                return "INFO";
            }
            else if (eventType == "Prop")
            {
                return "PROP";
            }
            else
            {
                return "ERROR";
            }
        }

        public static MethodBase GetRealMethodFromAsyncMethod(MethodBase asyncMethod)
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
                var foundMethod = matchingMethods.Single();
                return foundMethod;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
