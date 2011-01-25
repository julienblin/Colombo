#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System.Diagnostics.Contracts;
using Colombo.Contracts;

namespace Colombo
{
    /// <summary>
    /// A stateful message bus is a <see cref="IMessageBus"/> that tracks its state and complement the base <see cref="IMessageBus"/>
    /// with additional functionality such as the ability to automatically batch sends and to limit the number of sends it allows.
    /// </summary>
    [ContractClass(typeof(StatefulMessageBusContract))]
    public interface IStatefulMessageBus : IMessageBus
    {
        /// <summary>
        /// Return a promise of response - that is a proxy that when accessed the first time, it will send.
        /// This allow the batching of several FutureSend together.
        /// </summary>
        TResponse FutureSend<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// The number of time this <see cref="IStatefulMessageBus"/> has already sent.
        /// </summary>
        int NumberOfSend { get; }

        /// <summary>
        /// The maximum allowed number of send that this <see cref="IStatefulMessageBus"/> will allow.
        /// After this quota, every attempt to send will result in a <see cref="ColomboException"/>.
        /// </summary>
        int MaxAllowedNumberOfSend { get; set; }
    }
}
