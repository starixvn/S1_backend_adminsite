using System;

namespace BackendSite.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LogPropertyNameAttribute : Attribute
    {
        public LogPropertyNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}