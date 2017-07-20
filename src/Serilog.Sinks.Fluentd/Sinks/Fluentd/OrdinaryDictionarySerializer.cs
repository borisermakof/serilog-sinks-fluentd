using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MsgPack;
using MsgPack.Serialization;

namespace Serilog.Sinks.Fluentd
{
    internal class OrdinaryDictionarySerializer : MessagePackSerializer<IDictionary<string, object>>
    {
        public OrdinaryDictionarySerializer() : base(SerializationContext.Default)
        {

        }

        protected override void PackToCore(Packer packer, IDictionary<string, object> objectTree)
        {
            packer.PackMapHeader(objectTree);
            foreach (var pair in objectTree)
            {
                packer.PackString(pair.Key);
                var serializationContext = new SerializationContext(packer.CompatibilityOptions);
                serializationContext.Serializers.Register(this);
                packer.Pack(pair.Value, serializationContext);
            }
        }

        protected void UnpackTo(Unpacker unpacker, IDictionary<string, object> dict, long mapLength)
        {
            for (long i = 0; i < mapLength; i++)
            {
                string key;
                MessagePackObject value;
                if (!unpacker.ReadString(out key))
                    throw new InvalidMessagePackStreamException("string expected for a map key");
                if (!unpacker.ReadObject(out value))
                    throw new InvalidMessagePackStreamException("unexpected EOF");
                if (unpacker.IsMapHeader)
                {
                    var innerMapLength = value.AsInt64();
                    var innerDict = new Dictionary<string, object>();
                    UnpackTo(unpacker, innerDict, innerMapLength);
                    dict.Add(key, innerDict);
                }
                else if (unpacker.IsArrayHeader)
                {
                    var innerArrayLength = value.AsInt64();
                    var innerArray = new List<object>();
                    UnpackTo(unpacker, innerArray, innerArrayLength);
                    dict.Add(key, innerArray);
                }
                else
                {
                    dict.Add(key, value.ToObject());
                }
            }
        }

        protected void UnpackTo(Unpacker unpacker, IList<object> array, long arrayLength)
        {
            for (long i = 0; i < arrayLength; i++)
            {
                MessagePackObject value;
                if (!unpacker.ReadObject(out value))
                    throw new InvalidMessagePackStreamException("unexpected EOF");
                if (unpacker.IsMapHeader)
                {
                    var innerMapLength = value.AsInt64();
                    var innerDict = new Dictionary<string, object>();
                    UnpackTo(unpacker, innerDict, innerMapLength);
                    array.Add(innerDict);
                }
                else if (unpacker.IsArrayHeader)
                {
                    var innerArrayLength = value.AsInt64();
                    var innerArray = new List<object>();
                    UnpackTo(unpacker, innerArray, innerArrayLength);
                    array.Add(innerArray);
                }
                else
                {
                    array.Add(value.ToObject());
                }
            }
        }

        public new void UnpackTo(Unpacker unpacker, IDictionary<string, object> collection)
        {
            long mapLength;
            if (!unpacker.ReadMapLength(out mapLength))
                throw new InvalidMessagePackStreamException("map header expected");
            UnpackTo(unpacker, collection, mapLength);
        }

        protected override IDictionary<string, object> UnpackFromCore(Unpacker unpacker)
        {
            var retval = new Dictionary<string, object>();
            UnpackTo(unpacker, retval);
            return retval;
        }

        public new void UnpackTo(Unpacker unpacker, object collection)
        {
            var _collection = collection as IDictionary<string, object>;
            if (_collection == null)
                throw new NotSupportedException();
            UnpackTo(unpacker, _collection);
        }
    }
}
