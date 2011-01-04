﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    public class ResponseGroup<TFirstResponse, TSecondResponse>
        where TFirstResponse : Response, new()
        where TSecondResponse : Response, new()
    {
        public ResponseGroup(TFirstResponse firstResponse, TSecondResponse secondResponse)
        {
            if (firstResponse == null) throw new ArgumentNullException("firstResponse");
            if (secondResponse == null) throw new ArgumentNullException("secondResponse");
            Contract.EndContractBlock();

            First = firstResponse;
            Second = secondResponse;
        }

        public ResponseGroup(Response[] responses)
        {
            if (responses == null) throw new ArgumentNullException("responses");
            if (responses.Length < 2) throw new ArgumentException("responses lenght must be at least 2");
            Contract.EndContractBlock();

            if (!(responses[0] is TFirstResponse))
                throw new ColomboException(string.Format("responses[0] is of the wrong type: expected {0}, actual {1}", typeof(TFirstResponse), responses[0].GetType()));

            if (!(responses[1] is TSecondResponse))
                throw new ColomboException(string.Format("responses[1] is of the wrong type: expected {0}, actual {1}", typeof(TSecondResponse), responses[1].GetType()));

            First = (TFirstResponse)responses[0];
            Second = (TSecondResponse)responses[1];
        }

        public TFirstResponse First { get; private set; }

        public TSecondResponse Second { get; private set; }

        public virtual IEnumerable<Response> GetResponses()
        {
            yield return First;
            yield return Second;
        }
    }

    public class ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse> : ResponseGroup<TFirstResponse, TSecondResponse>
        where TFirstResponse : Response, new()
        where TSecondResponse : Response, new()
        where TThirdResponse : Response, new()
    {
        public ResponseGroup(TFirstResponse firstResponse, TSecondResponse secondResponse, TThirdResponse thirdResponse)
            : base(firstResponse, secondResponse)
        {
            if (firstResponse == null) throw new ArgumentNullException("firstResponse");
            if (secondResponse == null) throw new ArgumentNullException("secondResponse");
            if (thirdResponse == null) throw new ArgumentNullException("thirdResponse");
            Contract.EndContractBlock();

            Third = thirdResponse;
        }

        public ResponseGroup(Response[] responses)
            : base(responses)
        {
            if (responses == null) throw new ArgumentNullException("responses");
            if (responses.Length < 3) throw new ArgumentException("responses lenght must be at least 3");
            Contract.EndContractBlock();

            if (!(responses[2] is TThirdResponse))
                throw new ColomboException(string.Format("responses[2] is of the wrong type: expected {0}, actual {1}", typeof(TThirdResponse), responses[2].GetType()));

            Third = (TThirdResponse)responses[2];
        }

        public TThirdResponse Third { get; private set; }

        public override IEnumerable<Response> GetResponses()
        {
            yield return First;
            yield return Second;
            yield return Third;
        }
    }

    public class ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse> : ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse>
        where TFirstResponse : Response, new()
        where TSecondResponse : Response, new()
        where TThirdResponse : Response, new()
        where TFourthResponse : Response, new()
    {
        public ResponseGroup(TFirstResponse firstResponse, TSecondResponse secondResponse, TThirdResponse thirdResponse, TFourthResponse fourthResponse)
            : base(firstResponse, secondResponse, thirdResponse)
        {
            if (firstResponse == null) throw new ArgumentNullException("firstResponse");
            if (secondResponse == null) throw new ArgumentNullException("secondResponse");
            if (thirdResponse == null) throw new ArgumentNullException("thirdResponse");
            if (fourthResponse == null) throw new ArgumentNullException("fourthResponse");
            Contract.EndContractBlock();

            Fourth = fourthResponse;
        }

        public ResponseGroup(Response[] responses)
            : base(responses)
        {
            if (responses == null) throw new ArgumentNullException("responses");
            if (responses.Length < 4) throw new ArgumentException("responses lenght must be at least 4");
            Contract.EndContractBlock();

            if (!(responses[3] is TFourthResponse))
                throw new ColomboException(string.Format("responses[3] is of the wrong type: expected {0}, actual {1}", typeof(TFourthResponse), responses[3].GetType()));

            Fourth = (TFourthResponse)responses[3];
        }

        public TFourthResponse Fourth { get; private set; }

        public override IEnumerable<Response> GetResponses()
        {
            yield return First;
            yield return Second;
            yield return Third;
            yield return Fourth;
        }
    }
}