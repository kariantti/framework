﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using Signum.Utilities;
using System.Reflection;
using Signum.Entities;
using Signum.Utilities.ExpressionTrees;
using Signum.Engine;
using Signum.Utilities.DataStructures;
using Signum.Utilities.Reflection;
using Signum.Engine.Maps;
using Signum.Entities.Reflection;
using System.Diagnostics;

namespace Signum.Engine.Linq
{
    internal class EntityExpression : DbExpression
    {
        public static readonly FieldInfo IdField = ReflectionTools.GetFieldInfo((IdentifiableEntity ei) =>ei.id);
        public static readonly FieldInfo ToStrField = ReflectionTools.GetFieldInfo((IdentifiableEntity ie) =>ie.toStr);
        public static readonly MethodInfo ToStringMethod = ReflectionTools.GetMethodInfo((object o) => o.ToString());

        public readonly Table Table;
        public readonly Expression ExternalId;

        //Optional
        public readonly Alias TableAlias;
        public readonly ReadOnlyCollection<FieldBinding> Bindings;
        public readonly ReadOnlyCollection<MixinEntityExpression> Mixins;

        public readonly bool AvoidExpandOnRetrieving;

        public EntityExpression(Type type, Expression externalId, Alias tableAlias, IEnumerable<FieldBinding> bindings, IEnumerable<MixinEntityExpression> mixins, bool avoidExpandOnRetrieving)
            : base(DbExpressionType.Entity, type)
        {
            if (type == null) 
                throw new ArgumentNullException("type");

            if (!type.IsIdentifiableEntity())
                throw new ArgumentException("type");
            
            if (externalId == null) 
                throw new ArgumentNullException("externalId");
            
            this.Table = Schema.Current.Table(type);
            this.ExternalId = externalId;

            this.TableAlias = tableAlias;
            this.Bindings = bindings.ToReadOnly();
            this.Mixins = mixins.ToReadOnly();

            this.AvoidExpandOnRetrieving = avoidExpandOnRetrieving;
        }

        public override string ToString()
        {
            var constructor = "new {0}{1}({2})".Formato(Type.TypeName(), AvoidExpandOnRetrieving ? "?": "",
                ExternalId.NiceToString());

            return constructor +
                (Bindings == null ? null : ("\r\n{\r\n " + Bindings.ToString(",\r\n ").Indent(4) + "\r\n}")) +
                (Mixins == null ? null : ("\r\n" + Mixins.ToString(m => ".Mixin({0})".Formato(m), "\r\n")));
        }

        public Expression GetBinding(FieldInfo fi)
        {
            if (Bindings == null)
                throw new InvalidOperationException("EntityInitiExpression not completed");

            FieldBinding binding = Bindings.Where(fb => ReflectionTools.FieldEquals(fi, fb.FieldInfo)).SingleEx(() => "field '{0}' in {1} (field Ignored?)".Formato(fi.Name, this.Type.TypeName()));
            
            return binding.Binding;
        }
    }

  
    internal class EmbeddedEntityExpression : DbExpression
    {
        public readonly Expression HasValue; 

        public readonly ReadOnlyCollection<FieldBinding> Bindings;

        public readonly FieldEmbedded FieldEmbedded; //used for updates

        public EmbeddedEntityExpression(Type type, Expression hasValue, IEnumerable<FieldBinding> bindings, FieldEmbedded fieldEmbedded)
            : base(DbExpressionType.EmbeddedInit, type)
        {
            if (bindings == null)
                throw new ArgumentNullException("bindings");

            if (hasValue != null && hasValue.Type != typeof(bool))
                throw new ArgumentException("hasValue should be a boolean expression");

            HasValue = hasValue;

            Bindings = bindings.ToReadOnly();

            FieldEmbedded = fieldEmbedded; 
        }

        public Expression GetBinding(FieldInfo fi)
        {
            return Bindings.SingleEx(fb => ReflectionTools.FieldEquals(fi, fb.FieldInfo)).Binding;
        }

        public override string ToString()
        {
            string constructor = "new {0}".Formato(Type.TypeName());

            string bindings = Bindings.TryCC(b => b.ToString(",\r\n ")) ?? "";

            return bindings.HasText() ? 
                constructor + "\r\n{" + bindings.Indent(4) + "\r\n}" : 
                constructor;
        }
    }

    internal class MixinEntityExpression : DbExpression
    {
        public readonly ReadOnlyCollection<FieldBinding> Bindings;

        public readonly FieldMixin FieldMixin; //used for updates

        public MixinEntityExpression(Type type, IEnumerable<FieldBinding> bindings, FieldMixin fieldMixin)
            : base(DbExpressionType.MixinInit, type)
        {
            if (bindings == null)
                throw new ArgumentNullException("bindings");

            Bindings = bindings.ToReadOnly();

            FieldMixin = fieldMixin;
        }

        public Expression GetBinding(FieldInfo fi)
        {
            return Bindings.SingleEx(fb => ReflectionTools.FieldEquals(fi, fb.FieldInfo)).Binding;
        }

        public override string ToString()
        {
            string constructor = "new {0}".Formato(Type.TypeName());

            string bindings = Bindings.TryCC(b => b.ToString(",\r\n ")) ?? "";

            return bindings.HasText() ?
                constructor + "\r\n{" + bindings.Indent(4) + "\r\n}" :
                constructor;
        }
    }

   

    internal class FieldBinding
    {
        public readonly FieldInfo FieldInfo;
        public readonly Expression Binding;

        public FieldBinding(FieldInfo fieldInfo, Expression binding)
        {
            if (!fieldInfo.FieldType.IsAssignableFrom(binding.Type))
                throw new ArgumentException("Type of expression is {0} but type of field is {1}".Formato(binding.Type.TypeName(), fieldInfo.FieldType.TypeName()));
            
            this.FieldInfo = fieldInfo;
            this.Binding = binding;
        }

        public override string ToString()
        {
            return "{0} = {1}".Formato(FieldInfo.Name, Binding.NiceToString());
        }
    }

    internal class ImplementedByExpression : DbExpression//, IPropertyInitExpression
    {
        public readonly ReadOnlyDictionary<Type, EntityExpression> Implementations;

        public readonly CombineStrategy Strategy;

        public ImplementedByExpression(Type type, CombineStrategy strategy, IDictionary<Type, EntityExpression> implementations)
            : base(DbExpressionType.ImplementedBy, type)
        {
            this.Implementations = implementations.ToReadOnly();
            this.Strategy = strategy;
        }

        public override string ToString()
        {
            return "ImplementedBy({0}){{\r\n{1}\r\n}}".Formato(Strategy,
                Implementations.ToString(kvp => "{0} ->  {1}".Formato(kvp.Key.NiceName(), kvp.Value.NiceToString()), "\r\n").Indent(4)
                );
        }
    }

    internal class ImplementedByAllExpression : DbExpression
    {
        public readonly Expression Id;
        public readonly TypeImplementedByAllExpression TypeId;

        public ImplementedByAllExpression(Type type, Expression id, TypeImplementedByAllExpression typeId)
            : base(DbExpressionType.ImplementedByAll, type)
        {
            this.Id = id;
            this.TypeId = typeId;
        }

        public override string ToString()
        {
            return "ImplementedByAll{{ ID = {0}, Type = {1} }}".Formato(Id, TypeId);
        }
    }

    internal class LiteReferenceExpression : DbExpression
    {
        public readonly Expression Reference; //Fie, ImplementedBy, ImplementedByAll or Constant to NullEntityExpression
        public readonly Expression CustomToStr; //Not readonly

        public LiteReferenceExpression(Type type, Expression reference, Expression customToStr) :
            base(DbExpressionType.LiteReference, type)
        {
            Type cleanType = Lite.Extract(type);

            if (cleanType != reference.Type)
                throw new ArgumentException("The type {0} is not the Lite version of {1}".Formato(type.TypeName(), reference.Type.TypeName()));

            this.Reference = reference;

            this.CustomToStr = customToStr;
        }

        public override string ToString()
        {
            return "({0}).ToLite({1})".Formato(Reference.NiceToString(), CustomToStr == null ? null : ("customToStr: " + CustomToStr.NiceToString()));
        }
    }

    internal class LiteValueExpression : DbExpression
    {
        public readonly Expression TypeId;
        public readonly Expression Id;
        public readonly Expression ToStr; //Not readonly
        

        public LiteValueExpression(Type type, Expression typeId, Expression id, Expression toStr) :
            base(DbExpressionType.LiteValue, type)
        {
            this.TypeId = typeId;
            this.Id = id;
            this.ToStr = toStr;
        }

        public override string ToString()
        {
            return "new Lite<{0}>({1},{2},{3})".Formato(Type.CleanType().TypeName(), TypeId.NiceToString(), Id.NiceToString(), ToStr.NiceToString());
        }
    }

    internal class TypeEntityExpression : DbExpression
    {
        public readonly Expression ExternalId;
        public readonly Type TypeValue;

        public TypeEntityExpression(Expression externalId, Type typeValue)
            : base(DbExpressionType.TypeEntity, typeof(Type))
        {
            if (externalId == null || externalId.Type.UnNullify() != typeof(int))
                throw new ArgumentException("typeId");

            if (typeValue == null)
                throw new ArgumentException("typeValue"); 

            this.TypeValue = typeValue;
            this.ExternalId = externalId;
        }

        public override string ToString()
        {
            return "TypeFie({0};{1})".Formato(TypeValue.TypeName(), ExternalId.NiceToString());
        }
    }

    internal class TypeImplementedByExpression : DbExpression
    {
        public readonly ReadOnlyDictionary<Type, Expression> TypeImplementations;

        public TypeImplementedByExpression(IDictionary<Type, Expression> typeImplementations)
            : base(DbExpressionType.TypeImplementedBy, typeof(Type))
        {
            if (typeImplementations == null || typeImplementations.Any(a => a.Value.Type.UnNullify() != typeof(int)))
                throw new ArgumentException("typeId");

            this.TypeImplementations = typeImplementations.ToReadOnly();
        }

        public override string ToString()
        {
            return "TypeIb({0})".Formato(TypeImplementations.ToString(kvp => "{0}({1})".Formato(kvp.Key.TypeName(), kvp.Value.NiceToString()), " | "));
        }
    }

    internal class TypeImplementedByAllExpression : DbExpression
    {
        public readonly Expression TypeColumn;

        public TypeImplementedByAllExpression(Expression TypeColumn)
            : base(DbExpressionType.TypeImplementedByAll, typeof(Type))
        {
            if (TypeColumn == null || TypeColumn.Type.UnNullify() != typeof(int))
                throw new ArgumentException("typeId");

            this.TypeColumn = TypeColumn;
        }

        public override string ToString()
        {
            return "TypeIba({0})".Formato(TypeColumn.NiceToString());
        }
    }

    internal class MListExpression : DbExpression
    {
        public readonly Expression BackID; // not readonly
        public readonly RelationalTable RelationalTable;

        public MListExpression(Type type, Expression backID, RelationalTable tr)
            :base(DbExpressionType.MList, type)
        {
            this.BackID = backID;
            this.RelationalTable = tr;
        }

        public override string ToString()
        {
            return "new MList({0},{1})".Formato(RelationalTable.Name, BackID);
        }
    }

    internal class MListProjectionExpression : DbExpression
    {
        public readonly ProjectionExpression Projection;

        public MListProjectionExpression(Type type, ProjectionExpression projection)
            : base(DbExpressionType.MListProjection, type)
        {
            this.Projection = projection;
        }

        public override string ToString()
        {
            return "new MList({0})".Formato(Projection.NiceToString());
        }
    }

    internal class MListElementExpression : DbExpression
    {
        public readonly Expression RowId;
        public readonly EntityExpression Parent;
        public readonly Expression Element;

        public readonly RelationalTable Table;

        public MListElementExpression(Expression rowId, EntityExpression parent, Expression element, RelationalTable table)
            : base(DbExpressionType.MListElement, typeof(MListElement<,>).MakeGenericType(parent.Type, element.Type))
        {
            this.RowId = rowId;
            this.Parent = parent;
            this.Element = element;
            this.Table = table;
        }

        public override string ToString()
        {
            return "MListElement({0})\r\n{{\r\nParent={1},\r\nElement={2}}})".Formato(RowId, Parent, Element);
        }
    }
}
