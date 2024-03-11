using SimpleRpc.Sample.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRpc.Sample.Client
{
    public class DemoClient: IDisposable
    {
        private readonly IFooService service;

        public DemoClient(IFooService service)
        {
            this.service = service;
        }

        public async Task TestMain()
        {
            service.Plus(1, 5);

            Console.WriteLine("Calling Concat Method: " + service.Concat("Foo", "Bar"));

            await service.WriteFooAsync("TaskFoo", "TaskBar");

            Stream stream = await service.ReturnStream();
            string streamData =  Encoding.UTF8.GetString((stream as MemoryStream).ToArray());
            Console.WriteLine($"ReturnStream: {streamData}");
        }

        public async Task TestConcatAsync(int iterations = 1000)
        {
            var startTime = DateTime.Now;
            
            Console.WriteLine($"Start ConcatAsync Iterations: {iterations}");

            for (int i = 0; i < iterations; ++i)
            {
                string res1 = await service.ConcatAsync("sadasd", "asdsd");
            }

            var diff = DateTime.Now - startTime;
            Console.WriteLine($"End ConcatAsync: Time {diff}, Performance: {(iterations / diff.TotalMilliseconds) * 1000} msg/sec");
        }

        public async Task TestReturnGenericType(int iterations = 25000)
        {
            List<TestDto> list = new List<TestDto>();
            for (int i = 0; i < 10; ++i)
            {
                list.Add(new TestDto()
                {
                    Id= Guid.NewGuid(),
                    Name= $"Test Name lorem ipsum abra cadabra qwertyuiopasdfghjklzxcvbnm {i}",
                    Date= DateTime.Now,
                });
            }

            //Console.WriteLine($"ReturnGenericType Items Count: {list?.Count} First Id: {list.First().Id} Date: {list.First().Date.ToString("o")}");

            var res2 = await service.ReturnGenericType(list);

            Console.WriteLine($"ReturnGenericType Items Count: {res2?.Count} First Id: {res2.First().Id} Date: {res2.First().Date.ToString("o")}");

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 8
            };

            IEnumerable<int> ints = Enumerable.Range(0, iterations);

            var startTime = DateTime.Now;
            Console.WriteLine($"Start ReturnGenericType Iterations: {iterations}");

            await Parallel.ForEachAsync(ints, parallelOptions, async (id, _) =>
            {
                var res = await service.ReturnGenericType(list);
            });

        
            var diff = DateTime.Now - startTime;
            Console.WriteLine($"End ReturnGenericType: Time {diff}, Performance: {(iterations / diff.TotalMilliseconds) * 1000} msg/sec");
        }

        public async Task TestExceptions()
        {
            try
            {
                await service.ThrowException<object>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Client Dispose");
        }
    }
}
