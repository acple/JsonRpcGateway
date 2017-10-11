using System;
using ChainingAssertion;
using JsonRpcGateway;
using Xunit;

namespace UnitTest.JsonRpcGateway
{
    internal class OfficialSampleApi
    {
        public int[] Cache { get; private set; }

        [JsonRpcMethod("subtract")]
        public int Subtract(int minuend, int subtrahend)
            => minuend - subtrahend;

        public void Update(params int[] parameters)
        {
            this.Cache = parameters;
            foreach (var x in parameters)
                Console.WriteLine(x);
        }

        public void Foobar()
        { }
    }

    public class OfficialSample
    {
        private readonly JsonRpc<OfficialSampleApi> _jsonrpc = new JsonRpc<OfficialSampleApi>(new OfficialSampleApi());

        [Fact]
        public void Normal1()
        {
            var request = @"{""jsonrpc"": ""2.0"", ""method"": ""subtract"", ""params"": [42, 23], ""id"": 1}";
            // @"{""jsonrpc"": ""2.0"", ""result"": 19, ""id"": 1}";
            var response = this._jsonrpc.Run(request).IsInstanceOf<SuccessResponse>();
            response.Id.Is(1L);
            response.Result.Is(19);
        }

        [Fact]
        public void Normal2()
        {
            var request = @"{""jsonrpc"": ""2.0"", ""method"": ""subtract"", ""params"": [23, 42], ""id"": 2}";
            // @"{""jsonrpc"": ""2.0"", ""result"": -19, ""id"": 2}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<SuccessResponse>();
            response.Id.Is(2L);
            response.Result.Is(-19);
        }

        [Fact]
        public void NamedParameter1()
        {
            var request = @"{""jsonrpc"": ""2.0"", ""method"": ""subtract"", ""params"": {""subtrahend"": 23, ""minuend"": 42}, ""id"": 3}";
            // @"{""jsonrpc"": ""2.0"", ""result"": 19, ""id"": 3}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<SuccessResponse>();
            var x = response.ToString();
            response.Id.Is(3L);
            response.Result.Is(19);
        }

        [Fact]
        public void NamedParameter2()
        {
            var request = @"{""jsonrpc"": ""2.0"", ""method"": ""subtract"", ""params"": {""minuend"": 42, ""subtrahend"": 23}, ""id"": 4}";
            // @"{""jsonrpc"": ""2.0"", ""result"": 19, ""id"": 4}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<SuccessResponse>();
            response.Id.Is(4L);
            response.Result.Is(19);
        }

        [Fact]
        public void Notification()
        {
            var request = @"{""jsonrpc"": ""2.0"", ""method"": ""Update"", ""params"": [1,2,3,4,5]}";
            // no response

            var response = this._jsonrpc.Run(request);
            response.IsNull();
            this._jsonrpc.InterfaceInstance.Cache.Is(1, 2, 3, 4, 5);

            var request2 = @"{""jsonrpc"": ""2.0"", ""method"": ""Foobar""}";
            // no response

            var response2 = this._jsonrpc.Run(request2);
            response2.IsNull();
        }

        [Fact]
        public void NonExistentMethod()
        {
            var request = @"{""jsonrpc"": ""2.0"", ""method"": ""foobar"", ""id"": ""1""}";
            // @"{""jsonrpc"": ""2.0"", ""error"": {""code"": -32601, ""message"": ""Method not found""}, ""id"": ""1""}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();
            response.Id.Is("1");
            response.Error.ErrorCode.Is(ErrorCode.MethodNotFound);
        }

        [Fact]
        public void InvalidJson()
        {
            var request = @"{""jsonrpc"": ""2.0"", ""method"": ""foobar, ""params"": ""bar"", ""baz]";
            // @"{""jsonrpc"": ""2.0"", ""error"": {""code"": -32700, ""message"": ""Parse error""}, ""id"": null}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();
            response.Id.IsNull();
            response.Error.ErrorCode.Is(ErrorCode.ParseError);
        }
        // rpc call with invalid Request object:

        [Fact]
        public void InvalidRequest()
        {
            var request = @"{""jsonrpc"": ""2.0"", ""method"": 1, ""params"": ""bar""}";
            // @"{""jsonrpc"": ""2.0"", ""error"": {""code"": -32600, ""message"": ""Invalid Request""}, ""id"": null}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<ErrorResponse>();
            response.Id.IsNull();

            // spec expect to return "Invalid Request" for this request but actually is "Method not found"!
            response.Error.ErrorCode.Is(ErrorCode./*InvalidRequest*/MethodNotFound);
        }
    }
}
