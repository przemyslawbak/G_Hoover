using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace G_Hoover.Services.Logging
{
    public class LogService : ILogService
    {
        private readonly string _logFile = "../../../../log.txt";

        public string Line { get; set; }

        public void Prop(object value, [CallerMemberName] string propertyName = null)
        {
            object[] arguments = { propertyName, value };

            GetStringAttributesAsync(nameof(Prop), arguments, DateTime.Now).Wait();
        }

        public void Called(params object[] arguments)
        {
            GetStringAttributesAsync(nameof(Called), arguments, DateTime.Now).Wait();
        }

        public void Ended(params object[] arguments)
        {
            GetStringAttributesAsync(nameof(Ended), arguments, DateTime.Now).Wait();
        }

        public void Info(string value)
        {
            object[] arguments = { value };

            GetStringAttributesAsync(nameof(Info), arguments, DateTime.Now).Wait();
        }

        public void Error(string value)
        {
            object[] arguments = { value };

            GetStringAttributesAsync(nameof(Error), arguments, DateTime.Now).Wait();
        }

        private async Task GetStringAttributesAsync(string eventType, object[] arguments, DateTime date)
        {
            string methodName = string.Empty;
            string className = string.Empty;
            ParameterInfo[] parameters = { };

            MethodBase callingMethod = new StackTrace().GetFrame(4).GetMethod();

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

            eventType = eventType.ToUpper();

            Line = BuildLine(date, eventType, className, methodName, parameters, arguments);

            await SaveLogAsync(Line);
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
            else if (typeProps && areArgs) //PROP
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
                separator = BuildSeparator();
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

        private string BuildSeparator()
        {
            return "|";
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
                var foundMethod = matchingMethods.Single();
                return foundMethod;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task SaveLogAsync(string line) //DO NOT LOG -> makes endless loop!
        {
            try
            {
                if (Debugger.IsAttached) //save only for DEBUG
                {
                    using (TextWriter LineBuilder = new StreamWriter(_logFile, true))
                    {
                        await LineBuilder.WriteLineAsync(line);
                    }
                }
            }
            catch
            {
                Thread.Sleep(10);
                await SaveLogAsync(line + " <--DELAYED");
            }
        }
    }
}
