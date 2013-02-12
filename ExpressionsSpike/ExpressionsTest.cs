namespace ExpressionsSpike
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExpressionsTest
    {
        [TestMethod]
        public void LessThanOrEqualTest()
        {
            var date = new DateTime(1970, 8, 8);

            var exp = GetExpression<Album, DateTime>(
                ComparisonType.LessThanOrEqual, album => album.ReleaseDate, date);

            var queryable = this.albums.AsQueryable().Where(exp);
            var results = queryable.ToList();

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.Any(a => a.Name.Equals("Electric Ladyland")));
            Assert.IsTrue(results.Any(a => a.Name.Equals("Space Oddity")));
        }

        [TestMethod]
        public void EqualTest()
        {
            var date = new DateTime(1987, 3, 9);

            var exp = GetExpression<Album, DateTime>(
                ComparisonType.Equal, album => album.ReleaseDate, date);

            var queryable = this.albums.AsQueryable().Where(exp);
            var results = queryable.ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Any(a => a.Name.Equals("The Joshua Tree")));
        }

        [TestMethod]
        public void GreaterThanTest()
        {
            var date = new DateTime(1990, 1, 1);

            var exp = GetExpression<Album, DateTime>(
                ComparisonType.GreaterThan, album => album.ReleaseDate, date);

            var queryable = this.albums.AsQueryable().Where(exp);
            var results = queryable.ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Any(a => a.Name.Equals("Apple")));
        }

        [TestMethod]
        public void ContainsTest()
        {
            var exp = GetExpression<Album, string>(
                ComparisonType.Contains, album => album.Artist, "Bowie");

            var queryable = this.albums.AsQueryable().Where(exp);
            var results = queryable.ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Any(a => a.Name.Equals("Space Oddity")));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ContainsWithNonStringShouldFailTest()
        {
            GetExpression<Album, DateTime>(
                ComparisonType.Contains, album => album.ReleaseDate, new DateTime(2011, 8, 28));
        }

        private static Expression<Func<TObject, bool>> GetExpression<TObject, TProperty>(
            ComparisonType comparisonType, Expression<Func<TObject, TProperty>> property, TProperty value)
        {
            ExpressionType expressionType;

            switch (comparisonType)
            {
                case ComparisonType.LessThanOrEqual:
                    expressionType = ExpressionType.LessThanOrEqual;
                    break;
                case ComparisonType.LessThan:
                    expressionType = ExpressionType.LessThan;
                    break;
                case ComparisonType.GreaterThanOrEqual:
                    expressionType = ExpressionType.GreaterThanOrEqual;
                    break;
                case ComparisonType.GreaterThan:
                    expressionType = ExpressionType.GreaterThan;
                    break;
                case ComparisonType.Equal:
                    expressionType = ExpressionType.Equal;
                    break;
                case ComparisonType.Contains:
                    if (typeof(TProperty) != typeof(string))
                    {
                        throw new ArgumentException("ComparisonType.Contains is just for strings", "comparisonType");
                    }
                    expressionType = ExpressionType.Call;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("comparisonType");
            }

            var pe = Expression.Parameter(typeof(TObject), "obj");
            var prop = (MemberExpression)(property.Body);
            var propExp = Expression.Property(pe, (PropertyInfo)prop.Member);
            var constExp = Expression.Constant(value);

            Expression exp;

            if (comparisonType == ComparisonType.Contains)
            {
                var methodContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                exp = Expression.Call(
                    instance: propExp,
                    method: methodContains,
                    arguments: constExp);
            }
            else
            {
                exp = Expression.MakeBinary(
                    expressionType,
                    propExp, 
                    constExp);
            }
            
            var lambda = Expression.Lambda<Func<TObject, bool>>(
                exp, new[] { pe });

            return lambda;
        }

        private List<Album> albums = new List<Album>
            {
                new Album
                    {
                        Name = "The Joshua Tree",
                        Artist = "U2",
                        ReleaseDate = new DateTime(1987, 3, 9)
                    },
                new Album
                    {
                        Name = "Electric Ladyland",
                        Artist = "Jimi Hendrix",
                        ReleaseDate = new DateTime(1968, 10, 1)
                    },
                new Album
                    {
                        Name = "Space Oddity",
                        Artist = "David Bowie",
                        ReleaseDate = new DateTime(1969, 1, 1)
                    },
                new Album
                    {
                        Name = "Apple",
                        Artist = "Mother Love Bone",
                        ReleaseDate = new DateTime(1992, 1, 1)
                    }
            };

    }
}