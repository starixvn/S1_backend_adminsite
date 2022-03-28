using System;

namespace BackendSite.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class ExportIgnoreAttribute : Attribute
    {
    }
}