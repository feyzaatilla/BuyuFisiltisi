using System;
using System.Collections.Generic;
using BuyuFisiltisi.Core.Events;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode
{
    /// <summary>EventBus'ın temel davranışlarını doğrulayan birim testleri.</summary>
    public class EventBusTests
    {
        private struct TestEvent
        {
            public int Value;
        }

        [Test]
        public void Publish_DeliversEventToSubscriber()
        {
            var bus = new EventBus();
            var received = -1;
            bus.Subscribe<TestEvent>(e => received = e.Value);

            bus.Publish(new TestEvent { Value = 42 });

            Assert.AreEqual(42, received);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            var bus = new EventBus();
            var callCount = 0;
            Action<TestEvent> handler = _ => callCount++;

            bus.Subscribe(handler);
            bus.Publish(new TestEvent());
            bus.Unsubscribe(handler);
            bus.Publish(new TestEvent());

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void FailingHandler_DoesNotStopOtherHandlers()
        {
            var errors = new List<Exception>();
            var bus = new EventBus(errors.Add);
            var secondCalled = false;

            bus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("test hatası"));
            bus.Subscribe<TestEvent>(_ => secondCalled = true);

            bus.Publish(new TestEvent());

            Assert.IsTrue(secondCalled, "Hata fırlatan dinleyiciden sonraki dinleyici de çağrılmalı.");
            Assert.AreEqual(1, errors.Count, "Hata, hata işleyicisine raporlanmalı.");
        }
    }
}
