using System.Collections.Generic;
using System.Linq;
using ChainingAssertion;
using JsonRpcGateway;
using Xunit;

namespace UnitTest.JsonRpcGateway
{
    public class DataType1
    {
        public int A { get; set; }
        public int B { get; set; }
        public string C { get; set; }
        public List<int> D { get; set; }
        public Dictionary<string, string> E { get; set; }
    }

    public class Nested
    {
        public A A { get; set; }
        public B B { get; set; }
    }

    public class A
    {
        public string AValue { get; set; }
        public B B { get; set; }
    }

    public class B
    {
        public string BValue { get; set; }
        public IEnumerable<C> CArray { get; set; }
    }

    public class C
    {
        public string CValue { get; set; }
    }

    internal class TypeTestApi
    {
        public string StructedDataType(DataType1 data)
        {
            var ab = data.A + data.B;
            var c = data.C;
            var d = data.D.Sum();
            var e = data.E["testkey"];
            return $"ab: {ab}, c: {c}, d: {d}, e: {e}";
        }

        public string NestedType(Nested nested)
        {
            var a = nested.A.AValue;
            var b = nested.B.BValue;
            var ab = nested.A.B.BValue;
            var bc = nested.B.CArray.First().CValue;
            var abc = nested.A.B.CArray.First().CValue;
            return $"a: {a}, b: {b}, ab: {ab}, bc: {bc}, abc: {abc}";
        }
    }

    public class TypeTests
    {
        private readonly JsonRpc<TypeTestApi> _jsonrpc = new JsonRpc<TypeTestApi>(new TypeTestApi());

        const string _DataType1String = @"{""A"":111,""B"":222,""C"":""abcde"",""D"":[11,22,33],""E"":{""mykey"":""abcd"",""testkey"":""EFGH""}}";

        [Fact]
        public void StructedDataType()
        {
            var request = $@"{{""jsonrpc"":""2.0"",""method"":""StructedDataType"",""params"":{_DataType1String}}}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<SuccessResponse>();
            response.Result.Is("ab: 333, c: abcde, d: 66, e: EFGH");
        }

        [Fact]
        public void NestedType()
        {
            var param = @"{""A"":{""AValue"":""aaa"",""B"":{""BValue"":""bbb"",""CArray"":[{""CValue"":""ccc""},{""CValue"":""dummy""}]}},""B"":{""CArray"":[{""CValue"":""CCC""}],""BValue"":""BBB""}}";
            var request = $@"{{""jsonrpc"":""2.0"",""method"":""NestedType"",""params"":{param},""id"":111}}";

            var response = this._jsonrpc.Run(request).IsInstanceOf<SuccessResponse>();
            response.Result.Is("a: aaa, b: BBB, ab: bbb, bc: CCC, abc: ccc");
        }
    }
}
