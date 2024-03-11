using SimpleRpc.Sample.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRPC.Sample.Server
{
    public class FooServiceImpl : IFooService
    {
        private readonly long _instanceId;

        public FooServiceImpl(long instanceId)
        {
            _instanceId = instanceId;
            // Console.WriteLine($"Instance: {_instanceId}");
        }

        public void Plus(int a, int b)
        {
            Console.WriteLine($"Plus: {a + b}");
        }

        public string Concat(string a, string b)
        {
            return a + b;
        }

        public async Task WriteFooAsync(string a, string b)
        {
            await Task.Delay(10);
            Console.WriteLine($"WriteFooAsync a: {a} and b: {b}");
        }

        public async Task<string> ConcatAsync(string a, string b)
        {
            await Task.CompletedTask;
            return a + b;
        }

        public async Task<ICollection<T>> ReturnGenericType<T>(ICollection<T> collection)
        {
            await Task.CompletedTask;
            return collection;
        }

        public async Task<Stream> ReturnStream()
        {
            await Task.Yield();

            MemoryStream stream = new MemoryStream();
            byte[] buffer = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;
            return stream;
        }

        public Task<T> ThrowException<T>()
        {
            throw new ArgumentException($" THIS IS EXPECTED EXCEPTION MESSAGE!!! MethodName: {nameof(ThrowException)}");
        }
    }
}
