using System;
using Newtonsoft.Json;

namespace JsonRpcGateway
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonRpcException : Exception
    {
        [JsonProperty("code")]
        public ErrorCode ErrorCode { get; }

        [JsonProperty("message")]
        public override string Message => base.Message;

        /// <summary>Create json-rpc error object from server-defined errorcode number.</summary>
        public JsonRpcException(int code, string message) : this(code, message, exception: null)
        { }

        /// <summary>Create json-rpc error object from server-defined errorcode number.</summary>
        public JsonRpcException(int code, Exception exception) : this(code, exception.Message, exception)
        { }

        /// <summary>Create json-rpc error object from server-defined errorcode number.</summary>
        public JsonRpcException(int code, string message, Exception exception) : base(message, exception)
        {
            if (code < 0 || 99 < code)
                throw new ArgumentOutOfRangeException(nameof(code), code, "Server-defined errorcode must be between 0 and 99.");

            const int _ServerError = -32000;
            this.ErrorCode = (ErrorCode)(_ServerError - code);
        }

        public JsonRpcException(ErrorCode code, string message) : this(code, message, exception: null)
        { }

        public JsonRpcException(ErrorCode code, Exception exception) : this(code, exception.Message, exception)
        { }

        public JsonRpcException(ErrorCode code, string message, Exception exception) : base(message, exception)
        {
            this.ErrorCode = code;
        }
    }
}
