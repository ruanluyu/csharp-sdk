﻿using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCloud.Storage;

namespace Storage.Test {
    public class QueryTest : BaseTest {
        [Test]
        public async Task BaseQuery() {
            LCQuery<LCObject> query = new LCQuery<LCObject>("Hello");
            query.Skip(1).Limit(2);
            ReadOnlyCollection<LCObject> list = await query.Find();
            TestContext.WriteLine(list.Count);
            Assert.AreEqual(list.Count, 2);

            foreach (LCObject item in list) {
                Assert.NotNull(item.ClassName);
                Assert.NotNull(item.ObjectId);
                Assert.NotNull(item.CreatedAt);
                Assert.NotNull(item.UpdatedAt);

                TestContext.WriteLine(item.ClassName);
                TestContext.WriteLine(item.ObjectId);
                TestContext.WriteLine(item.CreatedAt);
                TestContext.WriteLine(item.UpdatedAt);
                TestContext.WriteLine(item["intValue"]);
                TestContext.WriteLine(item["boolValue"]);
                TestContext.WriteLine(item["stringValue"]);
            }
        }

        [Test]
        public async Task Count() {
            LCQuery<LCObject> query = new LCQuery<LCObject>("Account");
            query.WhereGreaterThan("balance", 200);
            int count = await query.Count();
            TestContext.WriteLine(count);
            Assert.Greater(count, 0);
        }

        [Test]
        public async Task OrderBy() {
            LCQuery<LCObject> query = new LCQuery<LCObject>("Account");
            query.OrderByAscending("balance");
            ReadOnlyCollection<LCObject> results = await query.Find();
            Assert.LessOrEqual((int)results[0]["balance"], (int)results[1]["balance"]);

            query = new LCQuery<LCObject>("Account");
            query.OrderByDescending("balance");
            results = await query.Find();
            Assert.GreaterOrEqual((int)results[0]["balance"], (int)results[1]["balance"]);
        }

        [Test]
        public async Task AddOrder() {
            LCQuery<LCObject> query = new LCQuery<LCObject>("Account");
            query.AddAscendingOrder("balance");
            query.AddDescendingOrder("createdAt");
            ReadOnlyCollection<LCObject> results = await query.Find();
            for (int i = 0; i + 1 < results.Count; i++) {
                LCObject a1 = results[i];
                LCObject a2 = results[i + 1];
                int b1 = (int)a1["balance"];
                int b2 = (int)a2["balance"];
                Assert.IsTrue(b1 < b2 ||
                    a1.CreatedAt.CompareTo(a2.CreatedAt) >= 0);
            }
        }

        [Test]
        public async Task Include() {
            Hello hello = new Hello {
                World = new World {
                    Content = "7788"
                }
            };
            await hello.Save();

            LCQuery<LCObject> query = new LCQuery<LCObject>("Hello");
            query.Include("objectValue");
            Hello queryHello = (await query.Get(hello.ObjectId)) as Hello;
            World world = queryHello.World;
            TestContext.WriteLine(world.Content);
            Assert.AreEqual(world.Content, "7788");
        }

        [Test]
        public async Task Get() {
            Account account = new Account {
                Balance = 1024
            };
            await account.Save();
            LCQuery<LCObject> query = new LCQuery<LCObject>("Account");
            Account newAccount = (await query.Get(account.ObjectId)) as Account;
            Assert.AreEqual(newAccount.Balance, 1024);
        }

        [Test]
        public async Task First() {
            LCQuery<LCObject> query = new LCQuery<LCObject>("Account");
            LCObject account = await query.First();
            Assert.NotNull(account.ObjectId);
        }

        [Test]
        public async Task GreaterQuery() {
            LCQuery<LCObject> query = new LCQuery<LCObject>("Account");
            query.WhereGreaterThan("balance", 200);
            ReadOnlyCollection<LCObject> list = await query.Find();
            TestContext.WriteLine(list.Count);
            Assert.Greater(list.Count, 0);
        }

        [Test]
        public async Task And() {
            LCQuery<LCObject> q1 = new LCQuery<LCObject>("Account");
            q1.WhereGreaterThan("balance", 100);
            LCQuery<LCObject> q2 = new LCQuery<LCObject>("Account");
            q2.WhereLessThan("balance", 500);
            LCQuery<LCObject> query = LCQuery<LCObject>.And(new List<LCQuery<LCObject>> { q1, q2 });
            ReadOnlyCollection<LCObject> results = await query.Find();
            TestContext.WriteLine(results.Count);
            foreach (LCObject item in results) {
                int balance = (int)item["balance"];
                Assert.IsTrue(balance >= 100 || balance <= 500);
            }
        }

        [Test]
        public async Task Or() {
            LCQuery<LCObject> q1 = new LCQuery<LCObject>("Account");
            q1.WhereLessThanOrEqualTo("balance", 100);
            LCQuery<LCObject> q2 = new LCQuery<LCObject>("Account");
            q2.WhereGreaterThanOrEqualTo("balance", 500);
            LCQuery<LCObject> query = LCQuery<LCObject>.Or(new List<LCQuery<LCObject>> { q1, q2 });
            ReadOnlyCollection<LCObject> results = await query.Find();
            TestContext.WriteLine(results.Count);
            foreach (LCObject item in results) {
                int balance = (int)item["balance"];
                Assert.IsTrue(balance <= 100 || balance >= 500);
            }
        }

        [Test]
        public async Task WhereObjectEquals() {
            World world = new World();
            Hello hello = new Hello {
                World = world
            };
            await hello.Save();

            LCQuery<LCObject> worldQuery = new LCQuery<LCObject>("World");
            LCObject queryWorld = await worldQuery.Get(world.ObjectId);
            LCQuery<LCObject> helloQuery = new LCQuery<LCObject>("Hello");
            helloQuery.WhereEqualTo("objectValue", queryWorld);
            LCObject queryHello = await helloQuery.First();
            TestContext.WriteLine(queryHello.ObjectId);
            Assert.AreEqual(queryHello.ObjectId, hello.ObjectId);
        }

        [Test]
        public async Task Exist() {
            LCQuery<LCObject> query = new LCQuery<LCObject>("Account");
            query.WhereExists("user");
            ReadOnlyCollection<LCObject> results = await query.Find();
            foreach (LCObject item in results) {
                Assert.NotNull(item["user"]);
            }

            query = new LCQuery<LCObject>("Account");
            query.WhereDoesNotExist("user");
            results = await query.Find();
            foreach (LCObject item in results) {
                Assert.IsNull(item["user"]);
            }
        }

        [Test]
        public async Task Select() {
            LCQuery<LCObject> query = new LCQuery<LCObject>("Account");
            query.Select("balance");
            ReadOnlyCollection<LCObject> results = await query.Find();
            foreach (LCObject item in results) {
                Assert.NotNull(item["balance"]);
                Assert.IsNull(item["user"]);
            }
        }

        [Test]
        public async Task String() {
            // Start
            LCQuery<LCObject> query = new LCQuery<LCObject>("Hello");
            query.WhereStartsWith("stringValue", "hello");
            ReadOnlyCollection<LCObject> results = await query.Find();
            foreach (LCObject item in results) {
                string str = item["stringValue"] as string;
                Assert.IsTrue(str.StartsWith("hello"));
            }

            // End
            query = new LCQuery<LCObject>("Hello");
            query.WhereEndsWith("stringValue", "world");
            results = await query.Find();
            foreach (LCObject item in results) {
                string str = item["stringValue"] as string;
                Assert.IsTrue(str.EndsWith("world"));
            }

            // Contains
            query = new LCQuery<LCObject>("Hello");
            query.WhereContains("stringValue", ",");
            results = await query.Find();
            foreach (LCObject item in results) {
                string str = item["stringValue"] as string;
                Assert.IsTrue(str.Contains(','));
            }
        }

        [Test]
        public async Task Array() {
            // equal
            LCQuery<LCObject> query = new LCQuery<LCObject>("Book");
            query.WhereEqualTo("pages", 3);
            ReadOnlyCollection<LCObject>results = await query.Find();
            foreach (LCObject item in results) {
                List<object> pages = item["pages"] as List<object>;
                Assert.IsTrue(pages.Contains(3));
            }

            // contain all
            List<int> containAlls = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            query = new LCQuery<LCObject>("Book");
            query.WhereContainsAll("pages", containAlls);
            results = await query.Find();
            foreach (LCObject item in results) {
                List<object> pages = item["pages"] as List<object>;
                pages.ForEach(i => {
                    Assert.IsTrue(pages.Contains(i));
                });
            }

            // contain in
            List<int> containIns = new List<int> { 4, 5, 6 };
            query = new LCQuery<LCObject>("Book");
            query.WhereContainedIn("pages", containIns);
            results = await query.Find();
            foreach (LCObject item in results) {
                List<object> pages = item["pages"] as List<object>;
                bool f = false;
                containIns.ForEach(i => {
                    f |= pages.Contains(i);
                });
                Assert.IsTrue(f);
            }

            // not contain in
            List<int> notContainIns = new List<int> { 1, 2, 3 };
            query = new LCQuery<LCObject>("Book");
            query.WhereNotContainedIn("pages", notContainIns);
            results = await query.Find();
            foreach (LCObject item in results) {
                List<object> pages = item["pages"] as List<object>;
                bool f = true;
                notContainIns.ForEach(i => {
                    f &= !pages.Contains(i);
                });
                Assert.IsTrue(f);
            }

            // size
            query = new LCQuery<LCObject>("Book");
            query.WhereSizeEqualTo("pages", 7);
            results = await query.Find();
            foreach (LCObject item in results) {
                List<object> pages = item["pages"] as List<object>;
                Assert.AreEqual(pages.Count, 7);
            }
        }

        [Test]
        public async Task Geo() {
            LCObject obj = new LCObject("Todo");
            LCGeoPoint location = new LCGeoPoint(39.9, 116.4);
            obj["location"] = location;
            await obj.Save();

            // near
            LCQuery<LCObject> query = new LCQuery<LCObject>("Todo");
            LCGeoPoint point = new LCGeoPoint(39.91, 116.41);
            query.WhereNear("location", point);
            ReadOnlyCollection<LCObject> results = await query.Find();
            Assert.Greater(results.Count, 0);

            // in box
            query = new LCQuery<LCObject>("Todo");
            LCGeoPoint southwest = new LCGeoPoint(30, 115);
            LCGeoPoint northeast = new LCGeoPoint(40, 118);
            query.WhereWithinGeoBox("location", southwest, northeast);
            results = await query.Find();
            Assert.Greater(results.Count, 0);
        }

        [Test]
        public async Task Regex() {
            LCQuery<LCObject> query = new LCQuery<LCObject>("Hello");
            query.WhereMatches("stringValue", "^HEllo.*", modifiers: "i");
            ReadOnlyCollection<LCObject> results = await query.Find();
            Assert.Greater(results.Count, 0);
            foreach (LCObject item in results) {
                string str = item["stringValue"] as string;
                Assert.IsTrue(str.StartsWith("hello"));
            }
        }

        [Test]
        public async Task InQuery() {
            LCQuery<LCObject> worldQuery = new LCQuery<LCObject>("World");
            worldQuery.WhereEqualTo("content", "7788");
            LCQuery<LCObject> helloQuery = new LCQuery<LCObject>("Hello");
            helloQuery.WhereMatchesQuery("objectValue", worldQuery);
            helloQuery.Include("objectValue");
            ReadOnlyCollection<LCObject> hellos = await helloQuery.Find();
            Assert.Greater(hellos.Count, 0);
            foreach (LCObject item in hellos) {
                LCObject world = item["objectValue"] as LCObject;
                Assert.AreEqual(world["content"], "7788");
            }
        }

        [Test]
        public async Task NotInQuery() {
            LCQuery<LCObject> worldQuery = new LCQuery<LCObject>("World");
            worldQuery.WhereEqualTo("content", "7788");
            LCQuery<LCObject> helloQuery = new LCQuery<LCObject>("Hello");
            helloQuery.WhereDoesNotMatchQuery("objectValue", worldQuery);
            helloQuery.Include("objectValue");
            ReadOnlyCollection<LCObject> hellos = await helloQuery.Find();
            Assert.Greater(hellos.Count, 0);
            foreach (LCObject item in hellos) {
                LCObject world = item["objectValue"] as LCObject;
                //Assert.IsTrue(world == null ||
                //    world["content"] == null ||
                //    world["content"] as string != "7788");
            }
        }
    }
}
