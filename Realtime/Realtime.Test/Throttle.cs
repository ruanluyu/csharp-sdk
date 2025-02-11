﻿using NUnit.Framework;
using System;
using System.Threading.Tasks;
using LeanCloud.Realtime;
using LeanCloud.Realtime.Internal.Protocol;

namespace Realtime.Test {
    public class Throttle : BaseTest {
        private LCIMClient c1;
        private LCIMClient c2;

        private LCIMConversation conversation;

        [SetUp]
        public override async Task SetUp() {
            await base.SetUp();
            c1 = new LCIMClient(Guid.NewGuid().ToString());
            c2 = new LCIMClient(Guid.NewGuid().ToString());
            await c1.Open();
            await c2.Open();

            conversation = await c1.CreateConversation(new string[] { Guid.NewGuid().ToString() });
        }

        [TearDown]
        public override async Task TearDown() {
            await c1.Close();
            await c2.Close();
            await base.TearDown();
        }

        [Test]
        public void Equality() {
            GenericCommand cmd1 = new GenericCommand {
                Cmd = CommandType.Session,
                Op = OpType.Open,
                PeerId = "hello",
                I = 1,
                SessionMessage = new SessionCommand {
                    Code = 123
                }
            };
            GenericCommand cmd2 = new GenericCommand {
                Cmd = CommandType.Session,
                Op = OpType.Open,
                PeerId = "hello",
                I = 2,
                SessionMessage = new SessionCommand {
                    Code = 123
                }
            };
            Assert.IsFalse(Equals(cmd1, cmd2));
            cmd2.I = cmd1.I;
            Assert.IsTrue(Equals(cmd1, cmd2));
        }

        [Test]
        public async Task RemoveMemberTwice() {
            Task t1 = conversation.RemoveMembers(new string[] { c2.Id }).ContinueWith(t => {
                Assert.IsTrue(t.IsCompleted && !t.IsFaulted);
            });
            Task t2 = conversation.RemoveMembers(new string[] { c2.Id }).ContinueWith(t => {
                Assert.IsTrue(t.IsCompleted && !t.IsFaulted);
            });

            await Task.WhenAll(t1, t2);
        }
    }
}
