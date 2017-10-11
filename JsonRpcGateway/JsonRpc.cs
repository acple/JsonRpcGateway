using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Linq.Expressions.Expression;

namespace JsonRpcGateway
{
    public class JsonRpc<T>
    {
        private readonly IReadOnlyDictionary<string, Method> _methods;

        public T InterfaceInstance { get; }

        private struct Method
        {
            public Type ReturnType { get; }

            public ParameterInfo[] Parameters { get; }

            public Func<object[], object> Function { get; }

            public Method(MethodInfo method, T target)
            {
                this.ReturnType = method.ReturnType;
                this.Parameters = method.GetParameters();
                this.Function = CreateDelegate(method, target);
            }

            private static Func<object[], object> CreateDelegate(MethodInfo method, T target)
            {
                var args = Parameter(typeof(object[]));
                var parameters = method.GetParameters()
                    .Select((parameter, i) => Convert(ArrayIndex(args, Constant(i)), parameter.ParameterType));

                var call = (method.IsStatic)
                    ? Call(method, parameters)
                    : Call(Constant(target, typeof(T)), method, parameters);
                var body = (method.ReturnType == typeof(void))
                    ? Block(call, Constant(null))
                    : Convert(call, typeof(object)) as Expression;

                var expression = Lambda<Func<object[], object>>(body, args);
                return expression.Compile();
            }
        }

        public JsonRpc() : this(default)
        { }

        public JsonRpc(T target)
        {
            this.InterfaceInstance = target;
            this._methods = typeof(T).GetTypeInfo().DeclaredMethods
                .Where(method => method.IsPublic && !method.IsSpecialName && !method.IsGenericMethod)
                .Where(method => target != null || method.IsStatic) // for static template
                .ToDictionary(
                    method => method.GetCustomAttribute<JsonRpcMethodAttribute>()?.Name ?? method.Name,
                    method => new Method(method, target));
        }

        public JsonRpcResponse Run(string json)
            => (TryParseRequest(json, out var request))
                ? this.RunRequest(request)
                : new ErrorResponse(null, new JsonRpcException(ErrorCode.ParseError, "Parse error, not well formed."));

        private static bool TryParseRequest(string json, out JsonRpcRequest request)
        {
            try
            {
                request = JsonConvert.DeserializeObject<JsonRpcRequest>(json);
                return true;
            }
            catch (JsonReaderException)
            {
                request = null;
                return false;
            }
        }

        public JsonRpcResponse RunRequest(JsonRpcRequest request)
        {
            try
            {
                ValidateRequest(request);

                var method = this.GetMethod(request);
                var parameters = GetParameters(request, method);

                var result = this.InvokeMethod(request, method, parameters);

                return (method.ReturnType != typeof(void)) ? new SuccessResponse(request.Id, result) : null;
            }
            catch (JsonRpcException exception)
            {
                return new ErrorResponse(request.Id, exception);
            }
        }

        private static void ValidateRequest(JsonRpcRequest request)
        {
            if (request.JsonRpc != "2.0" || request.Method == null)
                throw new JsonRpcException(ErrorCode.InvalidRequest, "Invalid Request The JSON sent is not a valid Request object.");
        }

        private Method GetMethod(JsonRpcRequest request)
        {
            if (!this._methods.ContainsKey(request.Method))
                throw new JsonRpcException(ErrorCode.MethodNotFound, $"Requested method \"{request.Method}\" is not found.");

            return this._methods[request.Method];
        }

        private static object[] GetParameters(JsonRpcRequest request, Method method)
        {
            var parameters = method.Parameters;
            if (parameters.Length == 0)
                return new object[0];

            try
            {
                return (parameters.Length == 1)
                    ? new[] { request.Parameters.ToObject(parameters[0].ParameterType) }
                    : (request.Parameters is JObject jObject) // named parameters
                        ? parameters
                            .Select(parameter => jObject[parameter.Name].ToObject(parameter.ParameterType))
                            .ToArray()
                        : parameters
                            .Select(parameter => parameter.ParameterType)
                            .Zip(request.Parameters, (type, json) => json.ToObject(type))
                            .ToArray();
            }
            catch (Exception exception)
            {
                throw new JsonRpcException(ErrorCode.InvalidParameters, "Invalid method parameters.", exception);
            }
        }

        private object InvokeMethod(JsonRpcRequest request, Method method, object[] parameters)
        {
            if (parameters.Length != method.Parameters.Length)
                throw new JsonRpcException(ErrorCode.InvalidParameters, "Parameter length mismatch.");

            try
            {
                return method.Function.Invoke(parameters);
            }
            catch (JsonRpcException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new JsonRpcException(ErrorCode.InternalError, exception);
            }
        }
    }
}
