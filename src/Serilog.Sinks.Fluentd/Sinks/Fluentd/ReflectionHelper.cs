using System;
using System.Collections.Generic;

namespace Serilog.Sinks.Fluentd
{
    public static class ReflectionHelper
    {
        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(sbyte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float)
        };

        public static bool IsNumericType(this Type type) => NumericTypes.Contains(type);
    }
}