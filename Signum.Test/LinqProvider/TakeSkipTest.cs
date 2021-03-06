﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Signum.Engine;
using Signum.Entities;
using System.Diagnostics;
using System.IO;
using Signum.Utilities;
using Signum.Test.Environment;

namespace Signum.Test.LinqProvider
{
    [TestClass]
    public class TakeSkipTest
    {
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Starter.StartAndLoad();
        }

        [TestInitialize]
        public void Initialize()
        {
            Connector.CurrentLogger = new DebugTextWriter();
        }

        [TestMethod]
        public void Take()
        {
            var takeArtist = Database.Query<ArtistDN>().Take(2).ToList();
            Assert.AreEqual(takeArtist.Count, 2);
        }

        [TestMethod]
        public void TakeOrder()
        {
            var takeArtist = Database.Query<ArtistDN>().OrderBy(a => a.Name).Take(2).ToList();
            Assert.AreEqual(takeArtist.Count, 2);
        }

        [TestMethod]
        public void TakeSql()
        {
            var takeAlbum = Database.Query<AlbumDN>().Select(a => new { a.Name, TwoSongs = a.Songs.Take(2) }).ToList();
            Assert.IsTrue(takeAlbum.All(a => a.TwoSongs.Count() <= 2));
        }

        [TestMethod]
        public void Skip()
        {
            var skipArtist = Database.Query<ArtistDN>().Skip(2).ToList();
        }

        [TestMethod]
        public void SkipOrder()
        {
            var skipArtist = Database.Query<ArtistDN>().OrderBy(a => a.Name).Skip(2).ToList();
        }

        [TestMethod]
        public void SkipSql()
        {
            var takeAlbum = Database.Query<AlbumDN>().Select(a => new { a.Name, TwoSongs = a.Songs.Skip(2) }).ToList();
        }

        [TestMethod]
        public void SkipTake()
        {
            var skipArtist = Database.Query<ArtistDN>().Skip(2).Take(1).ToList();
        }

        [TestMethod]
        public void SkipTakeOrder()
        {
            var skipArtist = Database.Query<ArtistDN>().OrderBy(a => a.Name).Skip(2).Take(1).ToList();
        }

        [TestMethod]
        public void OrderByCommonSelectPaginate()
        {
            TestPaginate(Database.Query<ArtistDN>().OrderBy(a => a.Sex).Select(a => a.Name));
        }

        [TestMethod]
        public void OrderBySelectPaginate()
        {
            TestPaginate(Database.Query<ArtistDN>().OrderBy(a => a.Name).Select(a => a.Name));
        }

        [TestMethod]
        public void OrderByDescendingSelectPaginate()
        {
            TestPaginate(Database.Query<ArtistDN>().OrderByDescending(a => a.Name).Select(a => a.Name));
        }

        [TestMethod]
        public void OrderByThenBySelectPaginate()
        {
            TestPaginate(Database.Query<ArtistDN>().OrderBy(a => a.Name).ThenBy(a => a.Id).Select(a => a.Name));
        }

        [TestMethod]
        public void SelectOrderByPaginate()
        {
            TestPaginate(Database.Query<ArtistDN>().Select(a => a.Name).OrderBy(a => a));
        }

        [TestMethod]
        public void SelectOrderByDescendingPaginate()
        {
            TestPaginate(Database.Query<ArtistDN>().Select(a => a.Name).OrderByDescending(a => a));
        }

        private void TestPaginate<T>(IQueryable<T> query)
        {
            var list = query.ToList();

            int pageSize = 2;

            var list2 = 0.To(((list.Count / pageSize) + 1)).SelectMany(page =>
                query.OrderAlsoByKeys().Skip(pageSize * page).Take(pageSize).ToList()).ToList();

            Assert.IsTrue(list.SequenceEqual(list2)); 
        }
    }
}
