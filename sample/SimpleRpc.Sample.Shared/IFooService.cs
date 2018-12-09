﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleRpc.Sample.Shared
{
    public interface IFooService
    {
        void Plus(int a, int b);

        string Concat(string a, string b);

        Task WriteFooAsync(string a, string b);

        Task<string> ConcatAsync(string a, string b);

        Task<ICollection<T>> ReturnGenericType<T>(ICollection<T> collection);

        Task<T> ThrowException<T>();
    }
}
