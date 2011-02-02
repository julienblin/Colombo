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
namespace Colombo
{
    /// <summary>
    /// Holds constants for key values that various components can use to insert meta-properties into the <see cref="BaseRequest.Context"/>
    /// </summary>
    /// <see cref="IMetaContextKeysManager"/>
    public static class MetaContextKeys
    {
        /// <summary>
        /// Prefix used by all the keys.
        /// </summary>
        public const string MetaPrefix = @"_";

        /// <summary>
        /// <see cref="System.Environment.MachineName"/> of the sender.
        /// </summary>
        public const string SenderMachineName = MetaPrefix + @"senderMachineName";

        /// <summary>
        /// <see cref="System.Environment.MachineName"/> of the handler. Could be the same as <see cref="SenderMachineName"/> if processed locally.
        /// </summary>
        public const string HandlerMachineName = MetaPrefix + @"handlerMachineName";

        /// <summary>
        /// When processed remotely, contains the address of the endpoint that received the requests.
        /// </summary>
        public const string EndpointAddressUri = MetaPrefix + @"endpointAddressUri";

        /// <summary>
        /// Indication of where the caller code used a Send method - could be a partial call stack or javascript/HTTP-REFERRER for example.
        /// </summary>
        public const string CodeOrigin = MetaPrefix + @"codeOrigin";
    }
}
