﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Signum.Utilities;
using Signum.Utilities.Reflection;
using Signum.Utilities.ExpressionTrees;
using System.Data;
using Signum.Entities.DynamicQuery;
using Signum.Entities;
using Signum.Engine.Linq;
using System.Collections;
using Signum.Engine.Maps;
using Signum.Entities.Basics;
using System.Globalization;

namespace Signum.Engine.DynamicQuery
{
    public static class AutocompleteUtils
    {
        public static List<Lite<IdentifiableEntity>> FindLiteLike(Implementations implementations, string subString, int count)
        {
            if (implementations.IsByAll)
                throw new InvalidOperationException("ImplementedByAll not supported for FindLiteLike");

            try
            {
                using (ExecutionMode.UserInterface())
                    return FindLiteLike(implementations.Types, subString, count);
            }
            catch (Exception e)
            {
                e.Data["implementations"] = implementations.ToString();
                throw;
            }
        }


        static NumberStyles numberStyles = NumberStyles.Integer | NumberStyles.AllowThousands;

        static List<Lite<IdentifiableEntity>> FindLiteLike(IEnumerable<Type> types, string subString, int count)
        {
            types = types.Where(t => Schema.Current.IsAllowed(t) == null);

            List<Lite<IdentifiableEntity>> results = new List<Lite<IdentifiableEntity>>();
            int? id = subString.ToInt(numberStyles);
            if (id.HasValue)
            {
                foreach (var t in types)
                {
                    var lite = miLiteById.GetInvoker(t).Invoke(id.Value);
                    if (lite != null)
                    {
                        results.Add(lite);

                        if (results.Count >= count)
                            return results;
                    }
                }
            }
            else
            {
                if (subString.Trim('\'', '"').ToInt(numberStyles).HasValue)
                    subString = subString.Trim('\'', '"');

                foreach (var t in types)
                {
                    results.AddRange(miLiteStarting.GetInvoker(t)(subString, count - results.Count));

                    if (results.Count >= count)
                        return results;
                }

                foreach (var t in types)
                {
                    results.AddRange(miLiteContaining.GetInvoker(t)(subString, count - results.Count));

                    if (results.Count >= count)
                        return results;
                }
            }

            return results;
        }

        public static List<Lite<T>> Autocomplete<T>(this IQueryable<T> query, string subString, int count)
            where T : IdentifiableEntity
        {
            List<Lite<T>> results = new List<Lite<T>>();

            int? id = subString.ToInt();
            if (id.HasValue)
            {
                Lite<T> entity = query.Select(a => a.ToLite()).SingleOrDefaultEx(e => e.Id == id);

                if (entity != null)
                    results.Add(entity);

                if (results.Count >= count)
                    return results;
            }

            results.AddRange(query.Where(a => a.ToString().StartsWith(subString)).Select(a => a.ToLite()).Take(count - results.Count));

            if (results.Count >= count)
                return results;

            results.AddRange(query.Where(a => !a.ToString().StartsWith(subString) && a.ToString().Contains(subString)).Select(a => a.ToLite()).Take(count - results.Count));

            if (results.Count >= count)
                return results;

            return results;
        }

        static GenericInvoker<Func<int, Lite<IdentifiableEntity>>> miLiteById =
            new GenericInvoker<Func<int, Lite<IdentifiableEntity>>>(id => LiteById<TypeDN>(id));
        static Lite<IdentifiableEntity> LiteById<T>(int id)
            where T : IdentifiableEntity
        {
            return Database.Query<T>().Where(a => a.id == id).Select(a => a.ToLite()).SingleOrDefault();
        }

        static GenericInvoker<Func<string, int, List<Lite<IdentifiableEntity>>>> miLiteStarting = 
            new GenericInvoker<Func<string, int, List<Lite<IdentifiableEntity>>>>((ss, c) => LiteStarting<TypeDN>(ss, c));
        static List<Lite<IdentifiableEntity>> LiteStarting<T>(string subString, int count)
            where T : IdentifiableEntity
        {
            return Database.Query<T>().Where(a => a.ToString().StartsWith(subString)).Select(a => a.ToLite()).Take(count)
                .AsEnumerable().OrderBy(l => l.ToString()).Cast<Lite<IdentifiableEntity>>().ToList();
        }

        static GenericInvoker<Func<string, int, List<Lite<IdentifiableEntity>>>> miLiteContaining = 
            new GenericInvoker<Func<string, int, List<Lite<IdentifiableEntity>>>>((ss, c) => LiteContaining<TypeDN>(ss, c));
        static List<Lite<IdentifiableEntity>> LiteContaining<T>(string subString, int count)
            where T : IdentifiableEntity
        {
            return Database.Query<T>().Where(a => a.ToString().Contains(subString) && !a.ToString().StartsWith(subString)).Select(a => a.ToLite()).Take(count)
                .AsEnumerable().OrderBy(l => l.ToString()).Cast<Lite<IdentifiableEntity>>().ToList();
        }

        public static List<Lite<IdentifiableEntity>> FindAllLite(Implementations implementations)
        {
            if (implementations.IsByAll)
                throw new InvalidOperationException("ImplementedByAll is not supported for RetrieveAllLite");

            try
            {
                using (ExecutionMode.UserInterface())
                    return implementations.Types.SelectMany(type => Database.RetrieveAllLite(type)).ToList();
            }
            catch (Exception e)
            {
                e.Data["implementations"] = implementations.ToString();
                throw;
            }
        }
    }
}
