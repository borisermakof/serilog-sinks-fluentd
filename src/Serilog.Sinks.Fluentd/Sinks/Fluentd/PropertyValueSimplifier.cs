using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.Fluentd
{
    static class PropertyValueSimplifier
    {
        // Code taken from: https://github.com/saleem-mirza/serilog-sinks-azure-analytics
        public static object Simplify(LogEventPropertyValue data)
        {
            if (data is ScalarValue value)
                return value.Value;

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (data is DictionaryValue dictValue)
            {
                var expObject = new ExpandoObject() as IDictionary<string, object>;
                foreach (var item in dictValue.Elements)
                {
                    if (item.Key.Value is string key)
                        expObject.Add(key, Simplify(item.Value));
                }

                return expObject;
            }

            if (data is SequenceValue seq)
                return seq.Elements.Select(Simplify).ToArray();

            if (!(data is StructureValue str))
                return null;

            {
                try
                {
                    if (str.TypeTag == null)
                        return str.Properties.ToDictionary(p => p.Name, p => Simplify(p.Value));

                    if (!str.TypeTag.StartsWith("DictionaryEntry") && !str.TypeTag.StartsWith("KeyValuePair"))
                        return str.Properties.ToDictionary(p => p.Name, p => Simplify(p.Value));

                    var key = Simplify(str.Properties[0].Value);

                    if (key == null)
                        return null;

                    var expObject = new ExpandoObject() as IDictionary<string, object>;
                    expObject.Add(key.ToString(), Simplify(str.Properties[1].Value));

                    return expObject;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return null;
        }
    }
}
