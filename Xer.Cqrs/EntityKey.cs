//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Jey.Cqrs
//{
//    public class EntityKey : ValueObject, IEquatable<EntityKey>
//    {
//        public Guid KeyValue { get; private set; }

//        public EntityKey(Guid keyValue)
//        {
//            KeyValue = keyValue;
//        }

//        protected override bool ValueEquals(ValueObject other)
//        {
//            var entityKey = other as EntityKey;
//            if(entityKey == null)
//            {
//                return false;
//            }

//            return Equals(other);
//        }

//        public bool Equals(EntityKey other)
//        {
//            if(other == null)
//            {
//                return false;
//            }

//            return KeyValue == other.KeyValue;
//        }
//    }
//}
