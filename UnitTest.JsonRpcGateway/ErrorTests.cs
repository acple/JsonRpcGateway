using System;
using ChainingAssertion;
using JsonRpcGateway;
using Xunit;

namespace UnitTest.JsonRpcGateway
{
    internal class ErrorTestApi
    {
        public void ExceptionThrown(int a)
            => throw new Exception("this is error");

        public int ThrowJsonRpcExceptionByUser(int a)
            => throw new JsonRpcException(12, "this is user-defined error");

        public string StandardMethod(int a, bool b)
            => a.ToString() + b.ToString();
    }

    public class ErrorTests
    {
        private readonly JsonRpc<ErrorTestApi> _jsonrpc = new JsonRpc<ErrorTestApi>(new ErrorTestApi());

        [Fact]
        public void NoVersion()
        {
            var request = @"{""method"":""StandardMethod"",""params"":{""a"":1,""b"":true}}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();
            response.Error.ErrorCode.Is(ErrorCode.InvalidRequest);
        }

        [Fact]
        public void NoMethod()
        {
            var request = @"{""jsonrpc"":""2.0"",""params"":123,""id"":""xyz""}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();
            response.Id.Is("xyz");
            response.Error.ErrorCode.Is(ErrorCode.InvalidRequest);
        }

        [Fact]
        public void ExceptionThrown()
        {
            var request = @"{""jsonrpc"":""2.0"",""method"":""ExceptionThrown"",""params"":123}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();

            response.Error.ErrorCode.Is(ErrorCode.InternalError);
            response.Error.Message.Is("this is error");
        }

        [Fact]
        public void ThrowJsonRpcExceptionByUser()
        {
            var request = @"{""jsonrpc"":""2.0"",""method"":""ThrowJsonRpcExceptionByUser"",""params"":123}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();

            response.Error.ErrorCode.Is((ErrorCode)(-32012));
            response.Error.Message.Is("this is user-defined error");
        }

        [Fact]
        public void MethodNotFound()
        {
            var request = @"{""jsonrpc"":""2.0"",""method"":""NonExistentMethod"",""params"":[123, false]}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();

            response.Error.ErrorCode.Is(ErrorCode.MethodNotFound);
        }

        [Fact]
        public void ParameterTypeMismatch()
        {
            var request = @"{""jsonrpc"":""2.0"",""method"":""StandardMethod"",""params"":[123,""abc""],""id"":12345}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();
            response.Id.Is(12345L);
            response.Error.ErrorCode.Is(ErrorCode.InvalidParameters);
            response.Error.Message.Is("Invalid method parameters.");
        }

        [Fact]
        public void ParameterCountMismatch()
        {
            var request = @"{""jsonrpc"":""2.0"",""method"":""StandardMethod"",""params"":[111],""id"":12345}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();

            response.Error.ErrorCode.Is(ErrorCode.InvalidParameters);
            response.Error.Message.Is("Parameter length mismatch.");
        }
    }
}
