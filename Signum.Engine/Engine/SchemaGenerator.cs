﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Engine.Maps;
using Signum.Utilities;
using Signum.Entities;

namespace Signum.Engine
{
    public static class SchemaGenerator
    {
        public static SqlPreCommand CreateSchemasScript()
        {
            return Schema.Current.GetDatabaseTables()
                .Select(a => a.Name.Schema)
                .Where(s => s.Name != "dbo")
                .Distinct()
                .Select(SqlBuilder.CreateSchema)
                .Combine(Spacing.Simple);
        }

        public static SqlPreCommand CreateTablesScript()
        {
            List<ITable> tables = Schema.Current.GetDatabaseTables().ToList();

            SqlPreCommand createTables = tables.Select(SqlBuilder.CreateTableSql).Combine(Spacing.Double);

            SqlPreCommand foreignKeys = tables.Select(SqlBuilder.AlterTableForeignKeys).Combine(Spacing.Double);
            
            SqlPreCommand indices = tables.Select(SqlBuilder.CreateAllIndices).NotNull().Combine(Spacing.Double);

            return SqlPreCommand.Combine(Spacing.Triple, createTables, foreignKeys, indices);
        }

        public static SqlPreCommand InsertEnumValuesScript()
        {
            return (from t in Schema.Current.Tables.Values
                    let enumType = EnumEntity.Extract(t.Type)
                    where enumType != null
                    select (from ie in EnumEntity.GetEntities(enumType)
                            select t.InsertSqlSync(ie)).Combine(Spacing.Simple)).Combine(Spacing.Double);
        }
    }
}
