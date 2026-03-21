// ============================================================================
//
// SessionStoreTests.cs — Unit tests for SessionStore.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Hivemind.Configuration;
using Foundation.Networking.Hivemind.Sessions;

namespace Foundation.Networking.Hivemind.Tests.Sessions
{
    public class SessionStoreTests
    {
        private HivemindConfiguration CreateConfig()
        {
            return new HivemindConfiguration
            {
                SessionTtlSeconds = 300,
                SessionSlidingExpiry = true,
                CleanupIntervalSeconds = 3600
            };
        }


        // ── Create / Get ─────────────────────────────────────────────────


        [Fact]
        public void CreateSession_ReturnsNonEmptyId()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                string id = store.CreateSession();

                Assert.False(string.IsNullOrEmpty(id));
            }
        }


        [Fact]
        public void CreateSession_IncrementsCount()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                store.CreateSession();
                store.CreateSession();

                Assert.Equal(2, store.Count);
            }
        }


        [Fact]
        public void GetSession_ValidId_ReturnsSession()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                string id = store.CreateSession();

                Session session = store.GetSession(id);

                Assert.NotNull(session);
                Assert.Equal(id, session.SessionId);
            }
        }


        [Fact]
        public void GetSession_InvalidId_ReturnsNull()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                Assert.Null(store.GetSession("nonexistent"));
            }
        }


        // ── Expiry ───────────────────────────────────────────────────────


        [Fact]
        public void GetSession_Expired_ReturnsNull()
        {
            HivemindConfiguration config = CreateConfig();
            config.SessionTtlSeconds = 0;

            using (SessionStore store = new SessionStore(config))
            {
                string id = store.CreateSession();
                System.Threading.Thread.Sleep(50);

                Assert.Null(store.GetSession(id));
            }
        }


        [Fact]
        public void SessionExists_Valid_ReturnsTrue()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                string id = store.CreateSession();

                Assert.True(store.SessionExists(id));
            }
        }


        [Fact]
        public void SessionExists_Invalid_ReturnsFalse()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                Assert.False(store.SessionExists("missing"));
            }
        }


        // ── Destroy ──────────────────────────────────────────────────────


        [Fact]
        public void DestroySession_RemovesSession()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                string id = store.CreateSession();
                bool destroyed = store.DestroySession(id);

                Assert.True(destroyed);
                Assert.Equal(0, store.Count);
            }
        }


        [Fact]
        public void DestroySession_NonExistent_ReturnsFalse()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                Assert.False(store.DestroySession("missing"));
            }
        }


        // ── Session Data ─────────────────────────────────────────────────


        [Fact]
        public void SetValue_And_GetValue_Works()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                string id = store.CreateSession();

                store.SetValue(id, "username", "demetree");

                Assert.Equal("demetree", store.GetValue(id, "username"));
            }
        }


        [Fact]
        public void GetValue_MissingKey_ReturnsNull()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                string id = store.CreateSession();

                Assert.Null(store.GetValue(id, "missing"));
            }
        }


        [Fact]
        public void SetValue_InvalidSession_ReturnsFalse()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                Assert.False(store.SetValue("missing", "key", "val"));
            }
        }


        [Fact]
        public void RemoveValue_DeletesKey()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                string id = store.CreateSession();
                store.SetValue(id, "key", "value");

                Assert.True(store.RemoveValue(id, "key"));
                Assert.Null(store.GetValue(id, "key"));
            }
        }


        [Fact]
        public void GetKeys_ReturnsAllKeys()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                string id = store.CreateSession();
                store.SetValue(id, "a", "1");
                store.SetValue(id, "b", "2");
                store.SetValue(id, "c", "3");

                List<string> keys = store.GetKeys(id);

                Assert.Equal(3, keys.Count);
                Assert.Contains("a", keys);
                Assert.Contains("b", keys);
                Assert.Contains("c", keys);
            }
        }


        [Fact]
        public void GetKeys_InvalidSession_ReturnsEmpty()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                Assert.Empty(store.GetKeys("missing"));
            }
        }


        // ── Statistics ───────────────────────────────────────────────────


        [Fact]
        public void GetStatistics_TracksCreated()
        {
            using (SessionStore store = new SessionStore(CreateConfig()))
            {
                store.CreateSession();
                store.CreateSession();

                SessionStoreStatistics stats = store.GetStatistics();

                Assert.Equal(2, stats.ActiveSessions);
                Assert.Equal(2, stats.TotalCreated);
            }
        }
    }
}
