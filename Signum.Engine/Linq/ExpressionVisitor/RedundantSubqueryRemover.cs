﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using Signum.Utilities;
using Signum.Utilities.ExpressionTrees;

namespace Signum.Engine.Linq
{
    class RedundantSubqueryRemover : DbExpressionVisitor
    {
        private RedundantSubqueryRemover()
        {
        }

        public static Expression Remove(Expression expression)
        {
            expression = new RedundantSubqueryRemover().Visit(expression);
            expression = SubqueryMerger.Merge(expression);
            return expression;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);

            // first remove all purely redundant subqueries
            List<SelectExpression> redundant = RedundantSubqueryGatherer.Gather(select.From);
            if (redundant != null)
            {
                select = (SelectExpression)SubqueryRemover.Remove(select, redundant);
            }

            return select;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            proj = (ProjectionExpression)base.VisitProjection(proj);
            if (proj.Select.From is SelectExpression)
            {
                List<SelectExpression> redundant = RedundantSubqueryGatherer.Gather(proj.Select);
                if (redundant != null)
                {
                    proj = (ProjectionExpression)SubqueryRemover.Remove(proj, redundant);
                }
            }
            return proj;
        }

        internal static bool IsSimpleProjection(SelectExpression select)
        {
            foreach (ColumnDeclaration decl in select.Columns)
            {
                ColumnExpression col = decl.Expression as ColumnExpression;
                if (col == null || decl.Name != col.Name)
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsNameMapProjection(SelectExpression select)
        {
            if (select.From is TableExpression) return false;
            SelectExpression fromSelect = select.From as SelectExpression;
            if (fromSelect == null || select.Columns.Count != fromSelect.Columns.Count)
                return false;
            ReadOnlyCollection<ColumnDeclaration> fromColumns = fromSelect.Columns;
            // test that all columns in 'select' are refering to columns in the same position
            // in from.
            for (int i = 0, n = select.Columns.Count; i < n; i++)
            {
                ColumnExpression col = select.Columns[i].Expression as ColumnExpression;
                if (col == null || !(col.Name == fromColumns[i].Name))
                    return false;
            }
            return true;
        }

        internal static bool IsInitialProjection(SelectExpression select)
        {
            return select.From is TableExpression;
        }

        class RedundantSubqueryGatherer : DbExpressionVisitor
        {
            List<SelectExpression> redundant;

            private RedundantSubqueryGatherer()
            {
            }

            internal static List<SelectExpression> Gather(Expression source)
            {
                RedundantSubqueryGatherer gatherer = new RedundantSubqueryGatherer();
                gatherer.Visit(source);
                return gatherer.redundant;
            }

            private static bool IsRedudantSubquery(SelectExpression select)
            {
                return (IsSimpleProjection(select) || IsNameMapProjection(select))
                    && !select.IsDistinct
                    && !select.IsReverse
                    && select.Top == null
                    //&& select.Skip == null
                    && select.Where == null
                    && (select.OrderBy == null || select.OrderBy.Count == 0)
                    && (select.GroupBy == null || select.GroupBy.Count == 0);
            }

            protected override Expression VisitSelect(SelectExpression select)
            {
                if (IsRedudantSubquery(select))
                {
                    if (this.redundant == null)
                    {
                        this.redundant = new List<SelectExpression>();
                    }
                    this.redundant.Add(select);
                }
                return select;
            }

            protected override Expression VisitSubquery(SubqueryExpression subquery)
            {
                // don't gather inside scalar & exists
                return subquery;
            }
        }

        class SubqueryMerger : DbExpressionVisitor
        {
            private SubqueryMerger()
            {
            }

            internal static Expression Merge(Expression expression)
            {
                return new SubqueryMerger().Visit(expression);
            }

            bool isTopLevel = true;

            protected override Expression VisitSelect(SelectExpression select)
            {
                bool wasTopLevel = isTopLevel;
                isTopLevel = false;

                select = (SelectExpression)base.VisitSelect(select);

                // next attempt to merge subqueries that would have been removed by the above
                // logic except for the existence of a where clause
                while (CanMergeWithFrom(select, wasTopLevel))
                {
                    SelectExpression fromSelect = GetLeftMostSelect(select.From);

                    // remove the redundant subquery
                    select = (SelectExpression)SubqueryRemover.Remove(select, new[] { fromSelect });

                    // merge where expressions 
                    Expression where = select.Where;
                    if (fromSelect.Where != null)
                    {
                        if (where != null)
                        {
                            where = Expression.And(fromSelect.Where, where);
                        }
                        else
                        {
                            where = fromSelect.Where;
                        }
                    }
                    var orderBy = select.OrderBy != null && select.OrderBy.Count > 0 ? select.OrderBy : fromSelect.OrderBy;
                    var groupBy = select.GroupBy != null && select.GroupBy.Count > 0 ? select.GroupBy : fromSelect.GroupBy;
                    //Expression skip = select.Skip != null ? select.Skip : fromSelect.Skip;
                    Expression top = select.Top != null ? select.Top : fromSelect.Top;
                    bool isDistinct = select.IsDistinct | fromSelect.IsDistinct;

                    if (where != select.Where
                        || orderBy != select.OrderBy
                        || groupBy != select.GroupBy
                        || isDistinct != select.IsDistinct
                        //|| skip != select.Skip
                        || top != select.Top)
                    {
                        select = new SelectExpression(select.Alias, isDistinct, top, select.Columns, select.From, where, orderBy, groupBy, select.SelectOptions);
                    }
                }

                return select;
            }

            static bool IsColumnProjection(SelectExpression select)
            {
                for (int i = 0, n = select.Columns.Count; i < n; i++)
                {
                    var cd = select.Columns[i];
                    if (cd.Expression.NodeType != (ExpressionType)DbExpressionType.Column &&
                        cd.Expression.NodeType != ExpressionType.Constant)
                        return false;
                }
                return true;
            }

            static bool CanMergeWithFrom(SelectExpression select, bool isTopLevel)
            {
                SelectExpression fromSelect = GetLeftMostSelect(select.From);
                if (fromSelect == null)
                    return false;
                if (!IsColumnProjection(fromSelect))
                    return false;
                bool selHasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
                bool selHasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
               
                bool frmHasOrderBy = fromSelect.OrderBy != null && fromSelect.OrderBy.Count > 0;
                bool frmHasGroupBy = fromSelect.GroupBy != null && fromSelect.GroupBy.Count > 0;
                // both cannot have orderby
                if (selHasOrderBy && frmHasOrderBy)
                    return false;
                // both cannot have groupby
                if (selHasGroupBy && frmHasGroupBy)
                    return false;
                // this are distinct operations 
                if (select.IsReverse || fromSelect.IsReverse)
                    return false;

                // cannot move forward order-by if outer has group-by
                if (frmHasOrderBy && (selHasGroupBy || select.IsDistinct || AggregateChecker.HasAggregates(select)))
                    return false;
                // cannot move forward group-by if outer has where clause
                if (frmHasGroupBy /*&& (select.Where != null)*/) // need to assert projection is the same in order to move group-by forward
                    return false;

                // cannot move forward a take if outer has take or skip or distinct
                if (fromSelect.Top != null && (select.Top != null || /*select.Skip != null ||*/ select.IsDistinct || selHasGroupBy || HasApplyJoin(select.From) ))
                    return false;
                // cannot move forward a skip if outer has skip or distinct
                //if (fromSelect.Skip != null && (select.Skip != null || select.Distinct || selHasAggregates || selHasGroupBy))
                //    return false;
                // cannot move forward a distinct if outer has take, skip, groupby or a different projection
                if (fromSelect.IsDistinct && (select.Top != null || /*select.Skip != null ||*/ !IsNameMapProjection(select) || selHasGroupBy || (selHasOrderBy && !isTopLevel) || AggregateChecker.HasAggregates(select)))
                    return false;
                return true;
            }

            static SelectExpression GetLeftMostSelect(Expression source)
            {
                SelectExpression select = source as SelectExpression;
                if (select != null)
                    return select;
                JoinExpression join = source as JoinExpression;

                if (join != null)
                    return GetLeftMostSelect(join.Left);
                return null;
            }

            static bool HasApplyJoin(SourceExpression source)
            {
                var join = source as JoinExpression;

                if (join == null)
                    return false;

                return join.JoinType == JoinType.CrossApply || join.JoinType == JoinType.OuterApply || HasApplyJoin(join.Left) || HasApplyJoin(join.Right);
            }
        }

        class AggregateChecker : DbExpressionVisitor
        {
            bool hasAggregate = false;
            private AggregateChecker()
            {
            }

            internal static bool HasAggregates(SelectExpression expression)
            {
                AggregateChecker checker = new AggregateChecker();
                checker.Visit(expression);
                return checker.hasAggregate;
            }

            protected override Expression VisitAggregate(AggregateExpression aggregate)
            {
                this.hasAggregate = true;
                return aggregate;
            }

            protected override Expression VisitSelect(SelectExpression select)
            {
                // only consider aggregates in these locations
                this.Visit(select.Where);

                select.OrderBy.NewIfChange(VisitOrderBy);
                select.Columns.NewIfChange(VisitColumnDeclaration);
                return select;
            }

            protected override Expression VisitSubquery(SubqueryExpression subquery)
            {
                // don't count aggregates in subqueries
                return subquery;
            }
        }
    }
}
