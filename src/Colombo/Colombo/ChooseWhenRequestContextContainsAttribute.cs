using System;

namespace Colombo
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class ChooseWhenRequestContextContainsAttribute : Attribute
    {
        private readonly string key;
        private readonly string value;

        public ChooseWhenRequestContextContainsAttribute(string key)
            : this(key, null)
        {
        }

        public ChooseWhenRequestContextContainsAttribute(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

            this.key = key;
            this.value = value;
        }

        public bool IsChoosen(BaseRequest request)
        {
            if (value == null)
                return request.Context.ContainsKey(key);
            
            return request.Context.ContainsKey(key) && request.Context[key].Equals(value, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
