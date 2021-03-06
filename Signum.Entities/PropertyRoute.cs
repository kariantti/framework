﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Signum.Utilities;
using Signum.Entities.Reflection;
using Signum.Utilities.Reflection;
using System.Linq.Expressions;
using Signum.Utilities.ExpressionTrees;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Signum.Entities
{
    [Serializable]
    public class PropertyRoute : IEquatable<PropertyRoute>, ISerializable
    {
        Type type;
        public PropertyRouteType PropertyRouteType { get; private set; } 
        public FieldInfo FieldInfo { get; private set;}
        public PropertyInfo PropertyInfo { get; private set; }
        public PropertyRoute Parent { get; private set;}

        public MemberInfo[] Members
        {
            get
            {
                return this.FollowC(a => a.Parent).Select(a =>
                    a.PropertyRouteType == Entities.PropertyRouteType.Mixin ? a.type :
                    a.FieldInfo ?? (MemberInfo)a.PropertyInfo).Reverse().Skip(1).ToArray();
            }
        }

        public PropertyInfo[] Properties
        {
            get { return this.FollowC(a => a.Parent).Select(a => a.PropertyInfo).Reverse().Skip(1).ToArray(); }
        }

        public static PropertyRoute Construct<T, S>(Expression<Func<T, S>> propertyRoute)
            where T : IRootEntity
        {
            PropertyRoute result = Root(typeof(T));

            foreach (var mi in Reflector.GetMemberList(propertyRoute))
            {
                if (mi is MethodInfo && ((MethodInfo)mi).IsInstantiationOf(MixinDeclarations.miMixin))
                    result = result.Add(((MethodInfo)mi).GetGenericArguments()[0]);
                else
                    result = result.Add(mi);
            }
            return result;
        }

     
        public PropertyRoute Add(string fieldOrProperty)
        {
            return Add(GetMember(fieldOrProperty));
        }


        MemberInfo GetMember(string fieldOrProperty)
        {
            MemberInfo mi = (MemberInfo)Type.GetProperty(fieldOrProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ??
                            (MemberInfo)Type.GetField(fieldOrProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (mi == null && Type.IsIdentifiableEntity())
            {
                string name = ExtractMixin(fieldOrProperty);

                mi = MixinDeclarations.GetMixinDeclarations(type).FirstOrDefault(t => t.Name == name);
            }

            if (mi == null)
                throw new InvalidOperationException("{0}.{1} does not exist".Formato(this, fieldOrProperty));

            return mi;
        }

        static string ExtractMixin(string fieldOrProperty)
        {
            Match match = Regex.Match(fieldOrProperty, @"^\[(?<type>.*)\]$");

            if (!match.Success)
                return null;

            return match.Groups["type"].Value;
        }

        public PropertyRoute Add(MemberInfo fieldOrProperty)
        {
            if (this.Type.IsIIdentifiable() && PropertyRouteType != PropertyRouteType.Root)
            {
                Implementations imp = GetImplementations();

                Type only;
                if (imp.IsByAll || (only = imp.Types.Only()) == null)
                    throw new InvalidOperationException("Attempt to make a PropertyRoute on a {0}. Cast first".Formato(imp));

                return new PropertyRoute(Root(only), fieldOrProperty);
            }

            return new PropertyRoute(this, fieldOrProperty);
        }

        PropertyRoute(PropertyRoute parent, MemberInfo fieldOrProperty)
        {
            SetParentAndProperty(parent, fieldOrProperty);
        }

        void SetParentAndProperty(PropertyRoute parent, MemberInfo fieldOrProperty)
        {
            if (fieldOrProperty == null)
                throw new ArgumentNullException("fieldOrProperty");

            if (parent == null)
                throw new ArgumentNullException("parent");

            this.Parent = parent;

            if (parent.Type.IsIIdentifiable() && parent.PropertyRouteType != PropertyRouteType.Root)
                throw new ArgumentException("Parent can not be a non-root Identifiable");

            if (fieldOrProperty is PropertyInfo && Reflector.IsMList(parent.Type))
            {
                if (fieldOrProperty.Name != "Item")
                    throw new NotSupportedException("PropertyInfo {0} is not supported".Formato(fieldOrProperty.Name));

                PropertyInfo = (PropertyInfo)fieldOrProperty;
                PropertyRouteType = PropertyRouteType.MListItems;
            }
            else if (fieldOrProperty is PropertyInfo && parent.Type.IsLite())
            {
                if (fieldOrProperty.Name != "Entity" && fieldOrProperty.Name != "EntityOrNull")
                    throw new NotSupportedException("PropertyInfo {0} is not supported".Formato(fieldOrProperty.Name));

                PropertyInfo = (PropertyInfo)fieldOrProperty;
                PropertyRouteType = PropertyRouteType.LiteEntity;
            }
            else if (typeof(IdentifiableEntity).IsAssignableFrom(parent.type) && fieldOrProperty is Type)
            {
                MixinDeclarations.AssertDefined(parent.type, (Type)fieldOrProperty);

                type = (Type)fieldOrProperty;
                PropertyRouteType = PropertyRouteType.Mixin;
            }
            else if (typeof(ModifiableEntity).IsAssignableFrom(parent.Type) || typeof(IRootEntity).IsAssignableFrom(parent.Type))
            {
                PropertyRouteType = PropertyRouteType.FieldOrProperty;
                if (fieldOrProperty is PropertyInfo)
                {
                    if (!parent.Type.FollowC(a => a.BaseType).Contains(fieldOrProperty.DeclaringType))
                    {
                        var pi = (PropertyInfo)fieldOrProperty;

                        if (!parent.Type.GetInterfaces().Contains(fieldOrProperty.DeclaringType))
                            throw new ArgumentException("PropertyInfo {0} not found on {1}".Formato(pi.PropertyName(), parent.Type));

                        var otherProperty = parent.Type.FollowC(a => a.BaseType)
                            .Select(a => a.GetProperty(fieldOrProperty.Name, BindingFlags.Public | BindingFlags.Instance)).NotNull().FirstEx();

                        if (otherProperty == null)
                            throw new ArgumentException("PropertyInfo {0} not found on {1}".Formato(pi.PropertyName(), parent.Type));

                        fieldOrProperty = otherProperty;
                    }

                    PropertyInfo = (PropertyInfo)fieldOrProperty;
                    FieldInfo = Reflector.TryFindFieldInfo(Parent.Type, PropertyInfo);
                }
                else
                {
                    FieldInfo = (FieldInfo)fieldOrProperty;
                    PropertyInfo = Reflector.TryFindPropertyInfo(FieldInfo);
                }
            }
            else
                throw new NotSupportedException("Properties of {0} not supported".Formato(parent.Type));

        }

        public static PropertyRoute Root(Type rootEntity)
        {
            return new PropertyRoute(rootEntity);
        }

        PropertyRoute(Type type)
        {
            SetRootType(type);
        }

        void SetRootType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (!typeof(IRootEntity).IsAssignableFrom(type))
                throw new ArgumentException("Type must implement IPropertyRouteRoot");

            this.type = type;
            this.PropertyRouteType = PropertyRouteType.Root;
        }

        public Type Type
        {
            get
            {
                if (type != null)
                    return type;

                if (FieldInfo != null)
                    return FieldInfo.FieldType;

                if (PropertyInfo != null)
                    return PropertyInfo.PropertyType;

                throw new InvalidOperationException("No FieldInfo or PropertyInfo"); 
            }
        }

        public Type RootType
        {
            get
            {
                if (type != null && type.IsIRootEntity())
                    return type;

                return Parent.RootType;
            }
        }

        public override string ToString()
        {
            switch (PropertyRouteType)
            {
                case PropertyRouteType.Root:
                    return "({0})".Formato(type.Name);
                case PropertyRouteType.FieldOrProperty:
                    return Parent.ToString() + (Parent.PropertyRouteType == PropertyRouteType.MListItems ? "" : ".") + (PropertyInfo != null ? PropertyInfo.Name : FieldInfo.Name);
                case PropertyRouteType.Mixin:
                    return "[{0}]".Formato(type.Name);
                case PropertyRouteType.MListItems:
                    return Parent.ToString() + "/";
                case PropertyRouteType.LiteEntity:
                    return Parent.ToString() + ".Entity";
            }
            throw new InvalidOperationException();
        }

        public string PropertyString()
        {
            switch (PropertyRouteType)
            {
                case PropertyRouteType.Root:
                    throw new InvalidOperationException("Root has no PropertyString");
                case PropertyRouteType.FieldOrProperty:
                    switch (Parent.PropertyRouteType)
                    {
                        case PropertyRouteType.Root: return (PropertyInfo != null ? PropertyInfo.Name : FieldInfo.Name);
                        case PropertyRouteType.FieldOrProperty: 
                        case PropertyRouteType.Mixin:
                            return Parent.PropertyString() + "." + (PropertyInfo != null ? PropertyInfo.Name : FieldInfo.Name);
                        case PropertyRouteType.MListItems: return Parent.PropertyString() + PropertyInfo.Name;
                        default: throw new InvalidOperationException();
                    }

                case PropertyRouteType.Mixin:
                    return "[{0}]".Formato(type.Name);
                case PropertyRouteType.MListItems:
                    return Parent.PropertyString() + "/";
            }
            throw new InvalidOperationException();
        }


        public static PropertyRoute Parse(Type type, string route)
        {
            PropertyRoute result = PropertyRoute.Root(type);

            foreach (var part in route.Replace("/", ".Item.").TrimEnd('.').Split('.'))
            {
                result = result.Add(part);
            }

            return result;
        }

        public static void SetFindImplementationsCallback(Func<PropertyRoute, Implementations> findImplementations)
        {
            FindImplementations = findImplementations;
        }

        static Func<PropertyRoute, Implementations> FindImplementations;

        public Implementations? TryGetImplementations()
        {
            if (this.Type.CleanType().IsIIdentifiable() && PropertyRouteType != Entities.PropertyRouteType.Root)
                return GetImplementations();

            return null;
        }

        public Implementations GetImplementations()
        {
            if (FindImplementations == null)
                throw new InvalidOperationException("PropertyRoute.FindImplementations not set");

            return FindImplementations(this);
        }

        public static void SetIsAllowedCallback(Func<PropertyRoute, string> isAllowed)
        {
            IsAllowedCallback = isAllowed;
        }

        static Func<PropertyRoute, string> IsAllowedCallback;
        
        public string IsAllowed()
        {
            if (IsAllowedCallback != null)
                return IsAllowedCallback(this);

            return null;
        }


        public static List<PropertyRoute> GenerateRoutes(Type type)
        {
            PropertyRoute root = PropertyRoute.Root(type);
            List<PropertyRoute> result = new List<PropertyRoute>();

            foreach (PropertyInfo pi in Reflector.PublicInstancePropertiesInOrder(type))
            {
                PropertyRoute route = root.Add(pi);
                result.Add(route);

                if (Reflector.IsEmbeddedEntity(pi.PropertyType))
                    result.AddRange(GenerateEmbeddedProperties(route));

                if (Reflector.IsMList(pi.PropertyType))
                {
                    Type colType = pi.PropertyType.ElementType();
                    if (Reflector.IsEmbeddedEntity(colType))
                        result.AddRange(GenerateEmbeddedProperties(route.Add("Item")));
                }
            }

            foreach (var t in MixinDeclarations.GetMixinDeclarations(type))
            {
                result.AddRange(GenerateEmbeddedProperties(root.Add(t)));
            }

            return result;
        }

        static List<PropertyRoute> GenerateEmbeddedProperties(PropertyRoute embeddedProperty)
        {
            List<PropertyRoute> result = new List<PropertyRoute>();
            foreach (var pi in Reflector.PublicInstancePropertiesInOrder(embeddedProperty.Type))
            {
                PropertyRoute property = embeddedProperty.Add(pi);
                result.AddRange(property);

                if (Reflector.IsEmbeddedEntity(pi.PropertyType))
                    result.AddRange(GenerateEmbeddedProperties(property));
            }

            return result;
        }

        public bool Equals(PropertyRoute other)
        {
            if (other.PropertyRouteType != this.PropertyRouteType)
                return false;

            if (Type != other.Type)
                return false;

            if (!FieldsEquals(other))
                return false;

            if (!PropertyEquals(other))
                return false;

            return object.Equals(Parent, other.Parent);
        }

        private bool FieldsEquals(PropertyRoute other)
        {
            if (FieldInfo == null)
                return other.FieldInfo == null;

            return other.FieldInfo != null && ReflectionTools.FieldEquals(FieldInfo, other.FieldInfo);
        }

        private bool PropertyEquals(PropertyRoute other)
        {
            if (PropertyInfo == null)
                return other.PropertyInfo == null;

            return other.PropertyInfo != null && ReflectionTools.PropertyEquals(PropertyInfo, other.PropertyInfo);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PropertyRoute other = obj as PropertyRoute;

            if (obj == null)
                return false;

            return Equals(other);
        }

        internal PropertyRoute SimplifyNoRoot()
        {
            switch (PropertyRouteType)
            {
                case PropertyRouteType.FieldOrProperty: return this;
                case PropertyRouteType.LiteEntity: return this.Parent;
                case PropertyRouteType.MListItems: return this.Parent;

                default:
                    throw new InvalidOperationException("PropertyRoute of type Root not expected");
            }
        }

        private PropertyRoute(SerializationInfo info, StreamingContext ctxt)
        {
            string rootName = info.GetString("rootType");

            Type root = Type.GetType(rootName);

            string route = info.GetString("property");

            if (route == null)
                this.SetRootType(root);
            else
            {
                string before = route.TryBeforeLast(".");

                if (before != null)
                {
                    var parent = Parse(root, before);

                    SetParentAndProperty(parent, parent.GetMember(route.AfterLast('.')));
                }
                else
                {
                    var parent = Root(root);

                    SetParentAndProperty(parent, parent.GetMember(route)); 
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("rootType", RootType.AssemblyQualifiedName);

            string property = 
                PropertyRouteType == Entities.PropertyRouteType.Root ? null :
                (PropertyRouteType == Entities.PropertyRouteType.LiteEntity ? this.Parent.PropertyString() + ".Entity" :
                this.PropertyString()).Replace("/", ".Item.").TrimEnd('.');

            info.AddValue("property", property);
        }
    }

    public interface IImplementationsFinder
    {
        Implementations FindImplementations(PropertyRoute route);
    }

    public enum PropertyRouteType
    {
        Root,
        FieldOrProperty,
        Mixin,
        LiteEntity, 
        MListItems,
    }

   
}
