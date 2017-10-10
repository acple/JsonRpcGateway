using System;

namespace JsonRpcGateway
{
    [AttributeUsage(AttributeTargets.Method)]
    public class JsonRpcMethodAttribute : Attribute
    {
        public string Name { get; }

        public JsonRpcMethodAttribute()
        { }

        public JsonRpcMethodAttribute(string name)
        {
            this.Name = name;
        }
    }
}
