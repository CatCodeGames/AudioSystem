using System;

namespace CatCode.Audio
{
    public readonly struct Callback
    {
        private static class Invoker<T> where T : class
        {
            public static readonly Action<object, Delegate> Instance = Invoke;

            private static void Invoke(object state, Delegate callback)
                => ((Action<T>)callback)((T)state);
        }

        private static class Invoker
        {
            public static readonly Action<object, Delegate> Empty = EmptyInvoke;
            public static readonly Action<object, Delegate> NoState = ActionInvoke;

            private static void EmptyInvoke(object state, Delegate callback) { }

            private static void ActionInvoke(object state, Delegate callback)
                => ((Action)callback)();
        }

        private readonly object _state;
        private readonly Delegate _callback;
        private readonly Action<object, Delegate> _invoker;

        public void Invoke()
            => _invoker(_state, _callback);

        private Callback(object state, Delegate callback, Action<object, Delegate> invoker)
        {
            _state = state;
            _callback = callback;
            _invoker = invoker;
        }

        public static Callback Create(Action callback)
        {
            return (callback != null)
                ? new Callback(null, callback, Invoker.NoState)
                : Empty;
        }

        public static Callback Create<T>(T state, Action<T> callback) where T : class
        {
            return (callback != null)
                ? new Callback(state, callback, Invoker<T>.Instance)
                : Empty;
        }

        public static Callback Empty
            => new(null, null, Invoker.Empty);

    }

    public readonly struct Callback<T>
    {
        private static class Invoker<S> where S : class
        {
            public static readonly Action<T, object, Delegate> Instance = Invoke;

            private static void Invoke(T value, object state, Delegate callback)
                => ((Action<S, T>)callback)((S)state, value);
        }

        private static class Invoker
        {
            public static readonly Action<T, object, Delegate> Empty = EmptyInvoke;
            public static readonly Action<T, object, Delegate> NoState = ActionInvoke;

            private static void EmptyInvoke(T value, object state, Delegate callback) { }

            private static void ActionInvoke(T value, object state, Delegate callback)
                => ((Action<T>)callback)(value);
        }

        private readonly object _state;
        private readonly Delegate _callback;
        private readonly Action<T, object, Delegate> _invoker;

        public void Invoke(T value)
            => _invoker(value, _state, _callback);

        private Callback(object state, Delegate callback, Action<T, object, Delegate> invoker)
        {
            _state = state;
            _callback = callback;
            _invoker = invoker;
        }

        public static Callback<T> Create(Action<T> callback)
        {
            return (callback != null)
                ? new Callback<T>(null, callback, Invoker.NoState)
                : Empty;
        }

        public static Callback<T> Create<S>(S state, Action<S, T> callback) where S : class
        {
            return (callback != null)
                ? new Callback<T>(state, callback, Invoker<S>.Instance)
                : Empty;
        }

        public static Callback<T> Empty
            => new(null, null, Invoker.Empty);

    }
}