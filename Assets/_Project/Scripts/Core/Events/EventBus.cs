using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuyuFisiltisi.Core.Events
{
    /// <summary>
    /// Tip tabanlı olay yolu. Her olay tipi için ayrı bir dinleyici listesi tutar.
    /// Bir dinleyici hata fırlatırsa hata raporlanır ama diğer dinleyiciler çalışmaya devam eder.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly Action<Exception> _errorHandler;

        /// <param name="errorHandler">
        /// Dinleyici hatalarının nereye raporlanacağı. Verilmezse Unity Console'a yazılır.
        /// Testlerde kendi işleyicimizi vererek hatayı yakalayabiliriz.
        /// </param>
        public EventBus(Action<Exception> errorHandler = null)
        {
            if (errorHandler != null) _errorHandler = errorHandler;
            else _errorHandler = LogToConsole;
        }

        private static void LogToConsole(Exception ex)
        {
            Debug.LogException(ex);
        }

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!_handlers.TryGetValue(typeof(T), out var list))
            {
                list = new List<Delegate>();
                _handlers[typeof(T)] = list;
            }

            // Aynı dinleyici iki kez eklenmesin
            if (!list.Contains(handler)) list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            if (_handlers.TryGetValue(typeof(T), out var list)) list.Remove(handler);
        }

        public void Publish<T>(T gameEvent)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list) || list.Count == 0) return;

            // Kopya üzerinde dönüyoruz: bir dinleyici yayın sırasında abonelikten çıkarsa liste bozulmasın
            var snapshot = list.ToArray();
            foreach (var handler in snapshot)
            {
                try
                {
                    ((Action<T>)handler).Invoke(gameEvent);
                }
                catch (Exception ex)
                {
                    _errorHandler(ex);
                }
            }
        }
    }
}
