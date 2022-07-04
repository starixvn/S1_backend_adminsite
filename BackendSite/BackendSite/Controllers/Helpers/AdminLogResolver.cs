using System.Linq;
using System.Reflection;
using BackendSite.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BackendSite.Controllers.Helpers
{
    public class AdminLogResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member.GetCustomAttributes(typeof(LogIgnoreAttribute)).Any())
            {
                property.ShouldSerialize = _ => false;
            }

            var nameAttr = (LogPropertyNameAttribute[])member
                .GetCustomAttributes(typeof(LogPropertyNameAttribute), false);
            if (nameAttr.Any())
            {
                property.PropertyName = nameAttr[0].Name;
            }
            return property;
        }
    }
}