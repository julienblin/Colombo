using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Colombo
{
    /// <summary>
    /// Base class for responses.
    /// </summary>
    [DataContract]
    public abstract class Response : BaseMessage
    {
        
    }
}
