#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Signum.Utilities;
using Signum.Utilities.Reflection;
using Signum.Entities;
using Signum.Engine.Maps;
using Signum.Utilities.DataStructures;
using Signum.Utilities.ExpressionTrees;
using Signum.Engine;
using Signum.Entities.Reflection;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Web.Mvc;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Globalization;
using Signum.Web;
using Microsoft.SqlServer.Types;
#endregion

namespace Signum.Web
{
    public delegate T Mapping<T>(MappingContext<T> ctx);

    public static class Mapping
    {
        static Mapping()
        {
            MappingRepository<bool>.Mapping = GetValue(ctx => ParseHtmlBool(ctx.Input));
            MappingRepository<byte>.Mapping = GetValue(ctx => byte.Parse(ctx.Input));
            MappingRepository<sbyte>.Mapping = GetValue(ctx => sbyte.Parse(ctx.Input));
            MappingRepository<short>.Mapping = GetValue(ctx => short.Parse(ctx.Input));
            MappingRepository<ushort>.Mapping = GetValue(ctx => ushort.Parse(ctx.Input));
            MappingRepository<int>.Mapping = GetValue(ctx => int.Parse(ctx.Input));
            MappingRepository<uint>.Mapping = GetValue(ctx => uint.Parse(ctx.Input));
            MappingRepository<long>.Mapping = GetValue(ctx => long.Parse(ctx.Input));
            MappingRepository<ulong>.Mapping = GetValue(ctx => ulong.Parse(ctx.Input));
            MappingRepository<float>.Mapping = GetValue(ctx => ctx.PropertyRoute != null && ReflectionTools.IsPercentage(Reflector.FormatString(ctx.PropertyRoute), CultureInfo.CurrentCulture) ? (float)ReflectionTools.ParsePercentage(ctx.Input, typeof(float), CultureInfo.CurrentCulture): float.Parse(ctx.Input));
            MappingRepository<double>.Mapping = GetValue(ctx => ctx.PropertyRoute != null && ReflectionTools.IsPercentage(Reflector.FormatString(ctx.PropertyRoute), CultureInfo.CurrentCulture) ? (double)ReflectionTools.ParsePercentage(ctx.Input, typeof(double), CultureInfo.CurrentCulture) : double.Parse(ctx.Input));
            MappingRepository<decimal>.Mapping = GetValue(ctx => ctx.PropertyRoute != null && ReflectionTools.IsPercentage(Reflector.FormatString(ctx.PropertyRoute), CultureInfo.CurrentCulture) ? (decimal)ReflectionTools.ParsePercentage(ctx.Input, typeof(decimal), CultureInfo.CurrentCulture) : decimal.Parse(ctx.Input));
            MappingRepository<DateTime>.Mapping = GetValue(ctx => DateTime.Parse(ctx.Input).FromUserInterface());
            MappingRepository<Guid>.Mapping = GetValue(ctx => Guid.Parse(ctx.Input));
            MappingRepository<TimeSpan>.Mapping = GetValue(ctx => TimeSpan.Parse(ctx.Input));
            MappingRepository<SqlHierarchyId>.Mapping = GetValue(ctx => SqlHierarchyId.Parse(ctx.Input));

            MappingRepository<bool?>.Mapping = GetValueNullable(ctx => ParseHtmlBool(ctx.Input));
            MappingRepository<byte?>.Mapping = GetValueNullable(ctx => byte.Parse(ctx.Input));
            MappingRepository<sbyte?>.Mapping = GetValueNullable(ctx => sbyte.Parse(ctx.Input));
            MappingRepository<short?>.Mapping = GetValueNullable(ctx => short.Parse(ctx.Input));
            MappingRepository<ushort?>.Mapping = GetValueNullable(ctx => ushort.Parse(ctx.Input));
            MappingRepository<int?>.Mapping = GetValueNullable(ctx => int.Parse(ctx.Input));
            MappingRepository<uint?>.Mapping = GetValueNullable(ctx => uint.Parse(ctx.Input));
            MappingRepository<long?>.Mapping = GetValueNullable(ctx => long.Parse(ctx.Input));
            MappingRepository<ulong?>.Mapping = GetValueNullable(ctx => ulong.Parse(ctx.Input));
            MappingRepository<float?>.Mapping = GetValueNullable(ctx => ctx.PropertyRoute != null && ReflectionTools.IsPercentage(Reflector.FormatString(ctx.PropertyRoute), CultureInfo.CurrentCulture) ? (float)ReflectionTools.ParsePercentage(ctx.Input, typeof(float), CultureInfo.CurrentCulture) : float.Parse(ctx.Input));
            MappingRepository<double?>.Mapping = GetValueNullable(ctx => ctx.PropertyRoute != null && ReflectionTools.IsPercentage(Reflector.FormatString(ctx.PropertyRoute), CultureInfo.CurrentCulture) ? (double)ReflectionTools.ParsePercentage(ctx.Input, typeof(double), CultureInfo.CurrentCulture) : double.Parse(ctx.Input));
            MappingRepository<decimal?>.Mapping = GetValueNullable(ctx => ctx.PropertyRoute != null && ReflectionTools.IsPercentage(Reflector.FormatString(ctx.PropertyRoute), CultureInfo.CurrentCulture) ? (decimal)ReflectionTools.ParsePercentage(ctx.Input, typeof(decimal), CultureInfo.CurrentCulture) : decimal.Parse(ctx.Input));
            MappingRepository<DateTime?>.Mapping = GetValueNullable(ctx => DateTime.Parse(ctx.Input).FromUserInterface());
            MappingRepository<Guid?>.Mapping = GetValueNullable(ctx => Guid.Parse(ctx.Input));
            MappingRepository<TimeSpan?>.Mapping = GetValueNullable(ctx => TimeSpan.Parse(ctx.Input));


            MappingRepository<string>.Mapping = ctx =>
            {
                if (ctx.Empty())
                    return ctx.None();

                return ctx.Input;
            };
        }

        public static DateTime DateHourMinute(MappingContext<DateTime> ctx)
        {
            if (ctx.Parent.Empty())
                return ctx.None();

            DateTime dateStart;
            int hours;
            int mins;
            if (ctx.Parse(out dateStart) & ctx.Parse("Hour", out hours) & ctx.Parse("Minute", out mins))
                return dateStart.AddHours(hours).AddMinutes(mins);

            return ctx.None();
        }

        public static DateTime? DateHourMinute(MappingContext<DateTime?> ctx)
        {
            if (ctx.Parent.Empty())
                return ctx.None();

            DateTime? dateStart;
            
            if (ctx.Parse(out dateStart))
            {
                if (dateStart.HasValue)
                {
                    int hours;
                    int mins;
                    if(ctx.Parse("Hour", out hours) & ctx.Parse("Minute", out mins))
                        return dateStart.Value.AddHours(hours).AddMinutes(mins);
                }
                else
                {
                    if (ctx.IsEmpty("Hour") && ctx.IsEmpty("Minute"))
                        return null;
                }
            }

            return ctx.None();
        }

        public static TimeSpan TimeSpanHourMinute(MappingContext<TimeSpan> ctx)
        {
            if (ctx.Parent.Empty())
                return ctx.None();

            int hours;
            int mins;
            if (ctx.Parse("Hour", out hours) & ctx.Parse("Minute", out mins))
                return new TimeSpan(hours, mins, 0);

            return ctx.None();
        }

        public static TimeSpan? TimeSpanHourMinute(MappingContext<TimeSpan?> ctx)
        {
            if (ctx.Parent.Empty())
                return ctx.None();

            int hours;
            int mins;
            if (ctx.Parse("Hour", out hours) & ctx.Parse("Minute", out mins))
                return new TimeSpan(hours, mins, 0);
                
            if (ctx.IsEmpty("Hour") && ctx.IsEmpty("Minute"))
                return null;

            return ctx.None();
        }

        public static void RegisterValue<T>(Mapping<T> mapping)
        {
            MappingRepository<T>.Mapping = mapping;
        }

        public static EntityMapping<T> AsEntityMapping<T>(this Mapping<T> mapping) where T : ModifiableEntity
        {
            return (EntityMapping<T>)mapping.Target;
        }

        public static AutoEntityMapping<T> AsAutoEntityMapping<T>(this Mapping<T> mapping) where T : class
        {
            return (AutoEntityMapping<T>)mapping.Target;
        }

        public static MListMapping<T> AsMListMapping<T>(this Mapping<MList<T>> mapping) where T : class
        {
            return (MListMapping<T>)mapping.Target;
        }

        internal static readonly string[] specialProperties = new[] 
        { 
            EntityBaseKeys.RuntimeInfo,
            EntityBaseKeys.ToStr, 
            EntityComboKeys.Combo,
            EntityListBaseKeys.Indexes,
            EntityListBaseKeys.List
        };

        static GenericInvoker<Func<Delegate>> giForAutoEntity = new GenericInvoker<Func<Delegate>>(() => ForAutoEntity<IIdentifiable>());
        static Mapping<T> ForAutoEntity<T>() where T : class
        {
            return new AutoEntityMapping<T>().GetValue;
        }

        static GenericInvoker<Func<Delegate>> giForLite = new GenericInvoker<Func<Delegate>>(() => ForLite<IIdentifiable>());
        static Mapping<Lite<S>> ForLite<S>() where S : class, IIdentifiable
        {
            return new LiteMapping<S>().GetValue;
        }

        static GenericInvoker<Func<Delegate>> giForMList = new GenericInvoker<Func<Delegate>>(() => ForMList<int>());
        static Mapping<MList<S>> ForMList<S>()
        {
            return new MListMapping<S>().GetValue;
        }

        static GenericInvoker<Func<Delegate>> giForEnum = new GenericInvoker<Func<Delegate>>(() => ForEnum<DayOfWeek>());
        static Mapping<T> ForEnum<T>() where T : struct
        {
            return MappingRepository<T>.Mapping = GetValue<T>(ctx => (T)Enum.Parse(typeof(T), ctx.Input));
        }

        static GenericInvoker<Func<Delegate>> giForEnumNullable = new GenericInvoker<Func<Delegate>>(() => ForEnumNullable<DayOfWeek>());
        static Mapping<T?> ForEnumNullable<T>() where T : struct
        {
            return MappingRepository<T?>.Mapping = GetValueNullable<T>(ctx => (T)Enum.Parse(typeof(T), ctx.Input));
        }


        public static Mapping<T> ForValue<T>()
        {
            var result = MappingRepository<T>.Mapping;

            if (result != null)
                return result;

            if (typeof(T).UnNullify().IsEnum)
            {
                MappingRepository<T>.Mapping = (Mapping<T>)(typeof(T).IsNullable() ? giForEnumNullable : giForEnum).GetInvoker(typeof(T).UnNullify())();

                return MappingRepository<T>.Mapping;
            }

            return null;
        }

        static class MappingRepository<T>
        {
            public static Mapping<T> Mapping; 
        }

        public static Mapping<T> New<T>()
        {
            var result = ForValue<T>();
            if (result != null)
                return result;

            if (typeof(T).IsModifiableEntity() || typeof(T).IsIIdentifiable())
                return (Mapping<T>)giForAutoEntity.GetInvoker(typeof(T))(); ;

            if (typeof(T).IsLite())
                return (Mapping<T>)giForLite.GetInvoker(Lite.Extract(typeof(T)))();

            if (Reflector.IsMList(typeof(T)))
                return (Mapping<T>)giForMList.GetInvoker(typeof(T).ElementType())();

            return ctx => { throw new InvalidOperationException("No mapping implemented for {0}".Formato(typeof(T).TypeName())); };
        }

        static Mapping<T> GetValue<T>(Func<MappingContext, T> parse) where T : struct
        {
            return ctx =>
            {
                if (ctx.Empty())
                    return ctx.None();

                try
                {
                    return parse(ctx);
                }
                catch (FormatException)
                {
                    return ctx.None(ctx.PropertyValidator != null ? ValidationMessage._0HasAnInvalidFormat.NiceToString().Formato(ctx.PropertyValidator.PropertyInfo.NiceName()) : ValidationMessage.InvalidFormat.NiceToString());
                }
            };
        }

        static Mapping<T?> GetValueNullable<T>(Func<MappingContext, T> parse) where T : struct
        {
            return ctx =>
            {
                if (ctx.Empty())
                    return ctx.None();

                string input = ctx.Input;
                if (string.IsNullOrWhiteSpace(input))
                    return null;

                try
                {
                    return parse(ctx);
                }
                catch (FormatException)
                {
                    return ctx.None(ctx.PropertyValidator != null ? ValidationMessage._0HasAnInvalidFormat.NiceToString().Formato(ctx.PropertyValidator.PropertyInfo.NiceName()) : ValidationMessage.InvalidFormat.NiceToString());
                }
            };
        }

        public static bool ParseHtmlBool(string input)
        {
            string[] vals = input.Split(',');
            return vals[0] == "true" || vals[0] == "True";
        }

        public static List<string> IndexPrefixes(this IDictionary<string, string> inputs)
        {
            return inputs.Keys
                .Where(k => k != EntityListBaseKeys.ListPresent && k != EntityListBaseKeys.List)
                .Select(str => str.Substring(0, str.IndexOf(TypeContext.Separator)))
                .Distinct()
                .OrderBy(a => int.Parse(a))
                .ToList();
        }

        public static List<IDictionary<string, string>> IndexSubDictionaries(this IDictionary<string, string> inputs)
        {
            return inputs.IndexPrefixes().Select(pf => inputs.SubDictionary(pf)).ToList(); 
        }

        public static IDictionary<string, string> SubDictionary(this IDictionary<string, string> sortedList, string nameToAppend)
        {
            var csl = sortedList as ContextualSortedList<string>;

            string controlID = csl == null ? nameToAppend : csl.ControlID + nameToAppend;

            return new ContextualSortedList<string>(sortedList, controlID + TypeContext.Separator);
        }
    }

    public abstract class BaseMapping<T>
    {
        public abstract T GetValue(MappingContext<T> mapping);

        public static implicit operator Mapping<T>(BaseMapping<T> mapping)
        {
            return mapping.GetValue;
        }
    }

    public class AutoEntityMapping<T> : BaseMapping<T> where T : class 
    {
        public Dictionary<Type, Delegate> AllowedMappings;

        public AutoEntityMapping<T> RegisterMapping<R>(EntityMapping<R> mapping) where R : ModifiableEntity
        {
            return RegisterMapping((Mapping<R>) mapping);
        }

        public AutoEntityMapping<T> RegisterMapping<R>(Mapping<R> mapping) where R : ModifiableEntity
        {
            if (AllowedMappings == null)
                AllowedMappings = new Dictionary<Type, Delegate>();

            AllowedMappings.Add(typeof(R), mapping);

            return this;
        }

        public bool DisambiguateRuntimeInfo { get; set; }

        public override T GetValue(MappingContext<T> ctx)
        {
            using (HeavyProfiler.LogNoStackTrace("GetValue", () => "AutoEntityMapping<{0}>".Formato(typeof(T).TypeName())))
            {
                if (ctx.Empty())
                    return ctx.None();

                 Type entityType;
                 entityType = GetRuntimeType(ctx);
          

                if (entityType == null)
                    return (T)(object)null;

                if (typeof(T) == entityType || typeof(T).IsEmbeddedEntity())
                    return GetRuntimeValue<T>(ctx, ctx.PropertyRoute);

                return miGetRuntimeValue.GetInvoker(entityType)(this, ctx, PropertyRoute.Root(entityType));
            }
        }

        private Type GetRuntimeType(MappingContext<T> ctx)
        {
            string strRuntimeInfo;
            if (ctx.Inputs.TryGetValue(EntityBaseKeys.RuntimeInfo, out strRuntimeInfo))
            {
                if (!DisambiguateRuntimeInfo)
                    return RuntimeInfo.FromFormValue(strRuntimeInfo).EntityType;
                else
                {
                    RuntimeInfo ri = strRuntimeInfo.Split(',')
                        .Select(r => RuntimeInfo.FromFormValue(r))
                        .OrderBy(a => !a.ToLite().RefersTo((IdentifiableEntity)(object)ctx.Value))
                        .FirstEx();

                    return ri.EntityType;
                }
            }
            else
                return ctx.Value.TryCC(t => t.GetType());
        }

        static GenericInvoker<Func<AutoEntityMapping<T>, MappingContext<T>, PropertyRoute, T>> miGetRuntimeValue = 
           new GenericInvoker<Func<AutoEntityMapping<T>, MappingContext<T>, PropertyRoute, T>>((aem, mc, pr)=>aem.GetRuntimeValue<T>(mc, pr));
        public R GetRuntimeValue<R>(MappingContext<T> ctx, PropertyRoute route)
            where R : class, T 
        {
            if (AllowedMappings != null && !AllowedMappings.ContainsKey(typeof(R)))
            {
                return (R)(object)ctx.None(ValidationMessage.Type0NotAllowed.NiceToString().Formato(typeof(R)));
            }

            Mapping<R> mapping =  (Mapping<R>)(AllowedMappings.TryGetC(typeof(R)) ?? Navigator.EntitySettings(typeof(R)).UntypedMappingLine);
            SubContext<R> sc = new SubContext<R>(ctx.ControlID, null, route, ctx) { Value = ctx.Value as R }; // If the type is different, the AutoEntityMapping has the current value but EntityMapping just null
            sc.Value = mapping(sc);
            ctx.SupressChange = sc.SupressChange;
            ctx.AddChild(sc);
            return sc.Value;
        }
    }

    abstract class PropertyMapping<T> where T : ModifiableEntity
    {
        public readonly IPropertyValidator PropertyValidator;

        protected PropertyMapping(IPropertyValidator pv)
        {
            this.PropertyValidator = pv;
        }

        public static PropertyMapping<T> Create(IPropertyValidator pv)
        {
            return (PropertyMapping<T>)Activator.CreateInstance(typeof(PropertyMapping<,>).MakeGenericType(typeof(T), pv.PropertyInfo.PropertyType), pv);
        }

        public abstract void SetProperty(MappingContext<T> parent);

        public override string ToString()
        {
            return PropertyValidator.PropertyInfo.PropertyName();
        }
    }

    class PropertyMapping<T, P> : PropertyMapping<T> where T : ModifiableEntity
    {
        public readonly Func<T, P> GetValue;
        public readonly Action<T, P> SetValue;

        public Mapping<P> Mapping { get; set; }

        public PropertyMapping(PropertyInfo pi)
            : this(Validator.TryGetPropertyValidator(typeof(T), pi.Name) ?? new PropertyValidator<T>(pi))
        {
        }

        public PropertyMapping(IPropertyValidator pv)
            : base(pv)
        {
            GetValue = ReflectionTools.CreateGetter<T, P>(pv.PropertyInfo);
            SetValue = ReflectionTools.CreateSetter<T, P>(pv.PropertyInfo);
            Mapping = Signum.Web.Mapping.New<P>();
        }

        public override void SetProperty(MappingContext<T> parent)
        {
            SubContext<P> ctx = CreateSubContext(parent);

            try
            {
                ctx.Value = Mapping(ctx);

                if (!ctx.SupressChange)
                    SetValue(parent.Value, ctx.Value);
            }
            catch (Exception e)
            {
                string error = e is FormatException ? ValidationMessage._0HasAnInvalidFormat.NiceToString() : ValidationMessage.NotPossibleToaAssign0.NiceToString();

                ctx.Error.Add(error.Formato(PropertyValidator.PropertyInfo.NiceName()));
            }

            if (!ctx.Empty())
                parent.AddChild(ctx);
        }

        public SubContext<P> CreateSubContext(MappingContext<T> parent)
        {
            string newControlId = TypeContextUtilities.Compose(parent.ControlID, PropertyValidator.PropertyInfo.Name);
            PropertyRoute route = parent.PropertyRoute.Add(this.PropertyValidator.PropertyInfo);

            SubContext<P> ctx = new SubContext<P>(newControlId, PropertyValidator, route, parent);
            if (parent.Value != null)
                ctx.Value = GetValue(parent.Value);
            return ctx;
        }
    }

    abstract class MixinMapping<T> : ModifiableEntity
    {
        public abstract void SetMixinProperties(MappingContext<T> parent);

        public static MixinMapping<T> Create(Type mt, bool fillProperties)
        {
            return (MixinMapping<T>)Activator.CreateInstance(typeof(MixinMapping<,>).MakeGenericType(typeof(T), mt), new object[] { fillProperties });
        }

        public abstract void CreateProperty<P>(PropertyInfo pi);
        public abstract void ReplaceProperty<P>(PropertyInfo pi, Func<Mapping<P>, Mapping<P>> replacer);
        public abstract Mapping<P> GetProperty<P>(PropertyInfo pi);
        public abstract void SetProperty<P>(PropertyInfo pi, Mapping<P> mapping);
        public abstract void RemoveProperty(PropertyInfo pi);
        public abstract void ClearProperties();
    }

    class MixinMapping<T, M> : MixinMapping<T>
        where T : IdentifiableEntity
        where M : MixinEntity
    {
        Dictionary<string, PropertyMapping<M>> properties = new Dictionary<string, PropertyMapping<M>>();

        public override void SetMixinProperties(MappingContext<T> parent)
        {
            MixinContext<M> ctx = new MixinContext<M>(parent.PropertyRoute.Add(typeof(M)), parent);
            if (parent.Value != null)
                ctx.Value = parent.Value.Mixin<M>();

            foreach (PropertyMapping<M> item in properties.Values)
            {
                item.SetProperty(ctx);
            }
        }

        public MixinMapping(bool fillProperties)
        {
            properties = Validator.GetPropertyValidators(typeof(M))
                   .Where(kvp => !kvp.Value.PropertyInfo.IsReadOnly())
                   .ToDictionary(kvp => kvp.Key, kvp => PropertyMapping<M>.Create(kvp.Value));
        }


        public override void CreateProperty<P>(PropertyInfo pi)
        {
            PropertyMapping<M, P> propertyMapping = (PropertyMapping<M, P>)properties.GetOrCreate(pi.Name, () => new PropertyMapping<M, P>(pi));

            propertyMapping.Mapping = Mapping.New<P>();
        }

        public override void ReplaceProperty<P>(PropertyInfo pi, Func<Mapping<P>, Mapping<P>> replacer)
        {
            var pm = (PropertyMapping<M, P>)properties.GetOrThrow(pi.Name);

            pm.Mapping = replacer(pm.Mapping);
        }

        public override Mapping<P> GetProperty<P>(PropertyInfo pi)
        {
            return ((PropertyMapping<M, P>)properties.GetOrThrow(pi.Name)).Mapping;
        }

        public override void SetProperty<P>(PropertyInfo pi, Mapping<P> mapping)
        {
            PropertyMapping<M, P> propertyMapping = (PropertyMapping<M, P>)properties.GetOrCreate(pi.Name, () => new PropertyMapping<M, P>(pi));

            propertyMapping.Mapping = mapping;
        }

        public override void RemoveProperty(PropertyInfo pi)
        {
            properties.Remove(pi.Name);
        }

        public override void ClearProperties()
        {
            properties.Clear();
        }
    }

    public class EntityMapping<T>: BaseMapping<T> where T : ModifiableEntity
    { 
        Dictionary<string, PropertyMapping<T>> properties = new Dictionary<string, PropertyMapping<T>>();
        Dictionary<Type, MixinMapping<T>> mixinMappings;

        public EntityMapping(bool fillProperties)
        {
            if (fillProperties)
            {
                properties = Validator.GetPropertyValidators(typeof(T))
                    .Where(kvp => !kvp.Value.PropertyInfo.IsReadOnly())
                    .ToDictionary(kvp => kvp.Key, kvp => PropertyMapping<T>.Create(kvp.Value));
            }

            if (typeof(IdentifiableEntity).IsAssignableFrom(typeof(T)))
            {
                mixinMappings = MixinDeclarations.GetMixinDeclarations(typeof(T))
                    .ToDictionary(t => t, t => MixinMapping<T>.Create(t, fillProperties));
            }
        }

        public override T GetValue(MappingContext<T> ctx)
        {
            using (HeavyProfiler.LogNoStackTrace("GetValue", () => "EntityMapping<{0}>".Formato(typeof(T).TypeName())))
            {
                if (ctx.Empty())
                    return ctx.None();

                var val = GetEntity(ctx);

                if (val == ctx.Value)
                    ctx.SupressChange = true;
                else
                    ctx.Value = val;

                SetValueProperties(ctx);

                RecursiveValidation(ctx);

                return val;
            }
        }

        public virtual void SetValueProperties(MappingContext<T> ctx)
        {
            foreach (PropertyMapping<T> item in properties.Values)
            {
                item.SetProperty(ctx);
            }

            if (mixinMappings != null)
            {
                foreach (MixinMapping<T> mt in mixinMappings.Values)
                {
                    mt.SetMixinProperties(ctx);
                }
            }
        }

        public virtual void RecursiveValidation(MappingContext<T> ctx)
        {
            ModifiableEntity entity = ctx.Value;
            foreach (MappingContext childCtx in ctx.Children())
            {
                string error = childCtx.PropertyValidator.PropertyCheck(entity);
                if (error.HasText())
                    childCtx.Error.AddRange(error.Lines());
            }
        }

        public bool DesambiguateRuntimeInfo { get; set; }

        public virtual T GetEntity(MappingContext<T> ctx)
        {
            string strRuntimeInfo;
            if (!ctx.Inputs.TryGetValue(EntityBaseKeys.RuntimeInfo, out strRuntimeInfo))
                return ctx.Value; //I only have some ValueLines of an Entity (so no Runtime, Id or anything)

            RuntimeInfo runtimeInfo; 
            if (!DesambiguateRuntimeInfo)
                runtimeInfo = RuntimeInfo.FromFormValue(strRuntimeInfo);
            else
            {
                runtimeInfo = strRuntimeInfo.Split(',')
                    .Select(r => RuntimeInfo.FromFormValue(r))
                    .OrderBy(a => !a.ToLite().RefersTo((IdentifiableEntity)(object)ctx.Value))
                    .FirstEx();
            }

            if (runtimeInfo.EntityType == null)
                return null;

            if (typeof(T).IsEmbeddedEntity())
            {
                if (runtimeInfo.IsNew || ctx.Value == null)
                    return Constructor.Construct<T>();

                return ctx.Value;
            }
            else
            {
                IdentifiableEntity identifiable = (IdentifiableEntity)(ModifiableEntity)ctx.Value;

                 if (runtimeInfo.IsNew)
                 {
                     if(identifiable != null && identifiable.IsNew)
                         return (T)(ModifiableEntity)identifiable;
                     else
                         return Constructor.Construct<T>();
                 }

                 if (identifiable != null && runtimeInfo.IdOrNull == identifiable.IdOrNull && runtimeInfo.EntityType == identifiable.GetType())
                     return (T)(ModifiableEntity)identifiable;
                 else
                     return (T)(ModifiableEntity)Database.Retrieve(runtimeInfo.EntityType, runtimeInfo.IdOrNull.Value);
            }
        }
 

        public EntityMapping<T> CreateProperty<P>(Expression<Func<T, P>> property)
        {
            MixinMapping<T> mixin;
            PropertyInfo pi = GetPropertyInfo(property, out mixin);
            if (mixin != null)
                mixin.CreateProperty<P>(pi);
            else
            {
                var pm = (PropertyMapping<T, P>)properties.GetOrCreate(pi.Name, () => new PropertyMapping<T, P>(pi));
                pm.Mapping = Mapping.New<P>();
            }

            return this;
        }

     

        public EntityMapping<T> ReplaceProperty<P>(Expression<Func<T, P>> property, Func<Mapping<P>, Mapping<P>> replacer)
        {
            MixinMapping<T> mixin;
            PropertyInfo pi = GetPropertyInfo(property, out mixin);
            if (mixin != null)
                mixin.ReplaceProperty<P>(pi, replacer);
            else
            {
                var pm = (PropertyMapping<T, P>)properties.GetOrThrow(pi.Name);
                pm.Mapping = replacer(pm.Mapping);
            }

            return this;
        }

        public EntityMapping<T> GetProperty<P>(Expression<Func<T, P>> property, Action<Mapping<P>> continuation)
        {
            MixinMapping<T> mixin;
            PropertyInfo pi = GetPropertyInfo(property, out mixin);
            if (mixin != null)
                continuation(mixin.GetProperty<P>(pi));
            else
                continuation(((PropertyMapping<T, P>)properties.GetOrThrow(pi.Name)).Mapping);

            return this;
        }

        public EntityMapping<T> SetProperty<P>(Expression<Func<T, P>> property, Mapping<P> mapping)
        {
            MixinMapping<T> mixin;
            PropertyInfo pi = GetPropertyInfo(property, out mixin);
            if (mixin != null)
                mixin.SetProperty(pi, mapping);
            else
            {
                var pm = (PropertyMapping<T, P>)properties.GetOrCreate(pi.Name, () => new PropertyMapping<T, P>(pi));
                pm.Mapping = mapping;
            }
            
            return this;
        }

        public EntityMapping<T> RemoveProperty<P>(Expression<Func<T, P>> property)
        {
            MixinMapping<T> mixin;
            PropertyInfo pi = GetPropertyInfo(property, out mixin);
            if (mixin != null)
                mixin.RemoveProperty(pi);
            else
                properties.Remove(pi.Name);

            return this;
        }

        public EntityMapping<T> ClearProperties()
        {
            properties.Clear();

            if (mixinMappings != null)
                foreach (var mm in mixinMappings.Values)
                    mm.ClearProperties();

            return this;
        }


        PropertyInfo GetPropertyInfo(LambdaExpression property, out MixinMapping<T> mixin)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            Expression body = property.Body;
            if (body.NodeType == ExpressionType.Convert)
                body = ((UnaryExpression)body).Operand;

            MemberExpression ex = body as MemberExpression;
            if (ex == null)
                throw new ArgumentException("The lambda 'property' should be an expression accessing a property");

            PropertyInfo pi = ex.Member as PropertyInfo;
            if (pi == null)
                throw new ArgumentException("The lambda 'property' should be an expression accessing a property");

            var mce = ex.Expression as MethodCallExpression;
            if (ex.Expression == property.Parameters.Only())
            {
                mixin = null;

                return pi;
            }
            else if (mce != null && mce.Method.IsInstantiationOf(MixinDeclarations.miMixin))
            {
                var type = mce.Method.GetGenericArguments()[0];

                mixin = mixinMappings.TryGetC(type);

                if (mixin == null)
                    throw new ArgumentException("The mixin {0} used in lambda 'property' is not registered".Formato(type.TypeName()));

                return pi;
            }
            else
                throw new ArgumentException("The lambda 'property' should be an expression accessing a property");
        }
    }


    public class LiteMapping<S> where S : class, IIdentifiable
    {
        public Mapping<S> EntityMapping { get; set; }

        public LiteMapping()
        {
            EntityMapping = Mapping.New<S>();

            EntityHasChanges = AnyNonSpecialImput; 
        }

        public Lite<S> GetValue(MappingContext<Lite<S>> ctx)
        {
            using (HeavyProfiler.LogNoStackTrace("GetValue", () => "LiteMapping<{0}>".Formato(typeof(S).TypeName())))
            {
                if (ctx.Empty())
                    return ctx.None();

                var newLite = Change(ctx);
                if (newLite == ctx.Value)
                    ctx.SupressChange = true;

                return newLite;
            }
        }

        public Lite<S> Change(MappingContext<Lite<S>> ctx)
        {
            string strRuntimeInfo;
            if (!ctx.Inputs.TryGetValue(EntityBaseKeys.RuntimeInfo, out strRuntimeInfo)) //I only have some ValueLines of an Entity (so no Runtime, Id or anything)
                return TryModifyEntity(ctx, ctx.Value);

            RuntimeInfo runtimeInfo = RuntimeInfo.FromFormValue(strRuntimeInfo);

            Lite<S> lite = (Lite<S>)ctx.Value;

            if (runtimeInfo.EntityType == null)
                return null;

            if (runtimeInfo.IsNew)
            {
                if (lite != null && lite.EntityOrNull != null && lite.EntityOrNull.IsNew)
                    return TryModifyEntity(ctx, lite);

                return TryModifyEntity(ctx, (Lite<S>)((IdentifiableEntity)Constructor.Construct(runtimeInfo.EntityType)).ToLiteFat());
            }

            if (lite == null)
                return TryModifyEntity(ctx, (Lite<S>)Database.RetrieveLite(runtimeInfo.EntityType, runtimeInfo.IdOrNull.Value));

            if (runtimeInfo.IdOrNull.Value == lite.IdOrNull && runtimeInfo.EntityType == lite.EntityType)
                return TryModifyEntity(ctx, lite);

            return TryModifyEntity(ctx, (Lite<S>)Database.RetrieveLite(runtimeInfo.EntityType, runtimeInfo.IdOrNull.Value));
        }

        public Lite<S> TryModifyEntity(MappingContext<Lite<S>> ctx, Lite<S> lite)
        {
            //commented out because of Lite<FileDN/FilePathDN>
            if (!EntityHasChanges(ctx))
                return lite; // If form does not contains changes to the entity

            if (EntityMapping == null)
                throw new InvalidOperationException("Changes to Entity {0} are not allowed because EntityMapping is null".Formato(lite.TryToString()));

            var sc = new SubContext<S>(ctx.ControlID, null, ctx.PropertyRoute.Add("Entity"), ctx) { Value = lite.Retrieve() };
            sc.Value = EntityMapping(sc);

            ctx.AddChild(sc);

            if (sc.SupressChange)
                return lite;

            return sc.Value.ToLite(sc.Value.IsNew);
        }

        public Func<MappingContext<Lite<S>>, bool> EntityHasChanges;

        private static bool AnyNonSpecialImput(MappingContext<Lite<S>> ctx)
        {
            return ctx.Inputs.Keys.Except(Mapping.specialProperties).Any();
        }
    }

    public abstract class BaseMListMapping<S> : BaseMapping<MList<S>>
    {
        public Mapping<S> ElementMapping { get; set; }

        public BaseMListMapping()
        {
            ElementMapping = Mapping.New<S>();
        }

        public BaseMListMapping(Mapping<S> elementMapping)
        {
            this.ElementMapping = elementMapping;
        }

        public BaseMListMapping<S> SetElementMapping(Mapping<S> elementMapping)
        {
            this.ElementMapping = elementMapping;
            return this; 
        }

        public IEnumerable<MappingContext<S>> GenerateItemContexts(MappingContext<MList<S>> ctx)
        {
            PropertyRoute route = ctx.PropertyRoute.Add("Item");

            var indexPrefixes = ctx.Inputs.IndexPrefixes();

            foreach (var index in indexPrefixes.OrderBy(ip => (ctx.GlobalInputs.TryGetC(TypeContextUtilities.Compose(ctx.ControlID, ip, EntityListBaseKeys.Indexes)).TryCC(i => i.Split(new[] { ';' })[1]) ?? ip).ToInt()))
            {
                SubContext<S> itemCtx = new SubContext<S>(TypeContextUtilities.Compose(ctx.ControlID, index), null, route, ctx);

                yield return itemCtx;
            }
        }
    }

    public class MListMapping<S> : BaseMListMapping<S>
    {
        public MListMapping()
            : base()
        {
        }

        public MListMapping(Mapping<S> elementMapping)
            : base(elementMapping)
        {
        }

        public override MList<S> GetValue(MappingContext<MList<S>> ctx)
        {
            using (HeavyProfiler.LogNoStackTrace("GetValue", () => "MListMapping<{0}>".Formato(typeof(S).TypeName())))
            {
                if (ctx.Empty())
                    return ctx.None();

                MList<S> oldList = ctx.Value;

                MList<S> newList = new MList<S>();

                foreach (MappingContext<S> itemCtx in GenerateItemContexts(ctx))
                {
                    Debug.Assert(!itemCtx.Empty());

                    int? oldIndex = itemCtx.Inputs.TryGetC(EntityListBaseKeys.Indexes).TryCC(s => s.Split(new[] { ';' })[0]).ToInt();

                    if (oldIndex.HasValue && oldList != null && oldList.Count > oldIndex.Value)
                        itemCtx.Value = oldList[oldIndex.Value];

                    itemCtx.Value = ElementMapping(itemCtx);

                    ctx.AddChild(itemCtx);
                    if (itemCtx.Value != null)
                        newList.Add(itemCtx.Value);
                }
                return newList;
            }
        }

    }

    public class MListCorrelatedMapping<S> : MListMapping<S>
    {
         public MListCorrelatedMapping()
            : base()
        {
        }

         public MListCorrelatedMapping(Mapping<S> elementMapping)
            : base(elementMapping)
        {
        }

        public override MList<S> GetValue(MappingContext<MList<S>> ctx)
        {
            using (HeavyProfiler.LogNoStackTrace("GetValue", () => "MListCorrelatedMapping<{0}>".Formato(typeof(S).TypeName())))
            {
                MList<S> list = ctx.Value;
                int i = 0;

                foreach (MappingContext<S> itemCtx in GenerateItemContexts(ctx).OrderBy(mc => mc.ControlID.Substring(mc.ControlID.LastIndexOf("_") + 1).ToInt().Value))
                {
                    Debug.Assert(!itemCtx.Empty());

                    itemCtx.Value = list[i];
                    itemCtx.Value = ElementMapping(itemCtx);

                    ctx.AddChild(itemCtx);
                    list[i] = itemCtx.Value;

                    i++;
                }

                return list;
            }
        }
    }

    public class MListDictionaryMapping<S, K> : BaseMListMapping<S>
        where S : ModifiableEntity
    {
        Func<S, K> GetKey;

        public string Route { get; set; }

        public Func<S, bool> FilterElements;

        public Mapping<K> KeyPropertyMapping{get;set;}
        
        public MListDictionaryMapping(Func<S, K> getKey, string route)
        {
            this.GetKey = getKey;

            this.KeyPropertyMapping = Mapping.New<K>();

            this.Route = route;
        }

        public MListDictionaryMapping(Func<S, K> getKey, string route, Mapping<S> elementMapping): base(elementMapping)
        {
            this.GetKey = getKey;

            this.KeyPropertyMapping = Mapping.New<K>();

            this.Route = route;
        }

        public override MList<S> GetValue(MappingContext<MList<S>> ctx)
        {
            using (HeavyProfiler.LogNoStackTrace("GetValue", () => "MListDictionaryMapping<{0}>".Formato(typeof(S).TypeName())))
            {
                if (ctx.Empty())
                    return ctx.None();

                MList<S> list = ctx.Value;

                var dic = (FilterElements == null ? list : list.Where(FilterElements)).ToDictionary(GetKey);

                PropertyRoute route = ctx.PropertyRoute.Add("Item");

                using(var log = HeavyProfiler.LogNoStackTrace("Log"))
                foreach (MappingContext<S> itemCtx in GenerateItemContexts(ctx))
                {
                    log.Switch("SubContext");

                    SubContext<K> subContext = new SubContext<K>(TypeContextUtilities.Compose(itemCtx.ControlID, Route), null, route, itemCtx);

                    log.Switch("KeyPropertyMapping");

                    subContext.Value = KeyPropertyMapping(subContext);

                    log.Switch("Dic");

                    itemCtx.Value = dic[subContext.Value];

                    log.Switch("ElementMapping");

                    itemCtx.Value = ElementMapping(itemCtx);

                    log.Switch("AddChild");

                    ctx.AddChild(itemCtx);

                    log.Switch("Log");
                }

                return list;
            }
        }
    }
}
