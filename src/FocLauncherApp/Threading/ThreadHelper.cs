using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FocLauncherApp.WaitDialog;
using Microsoft.VisualStudio.Threading;

namespace FocLauncherApp.Threading
{
    public abstract class ThreadHelper
    {
        private static Dispatcher _uiThreadDispatcher;
        private static ThreadHelper _generic;
        private static JoinableTaskContext _joinableTaskContextCache;

        public static JoinableTaskContext JoinableTaskContext => _joinableTaskContextCache ??= new JoinableTaskContext(Thread.CurrentThread,
            new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher, DispatcherPriority.Background));

        public static ThreadHelper Generic => _generic ??= new GenericThreadHelper();

        public static JoinableTaskFactory JoinableTaskFactory => JoinableTaskContext.Factory;

        private static Dispatcher DispatcherForUiThread
        {
            get
            {
                if (_uiThreadDispatcher == null && Application.Current != null)
                    _uiThreadDispatcher = Application.Current.Dispatcher;
                return _uiThreadDispatcher;
            }
        }

        static ThreadHelper()
        {
            SetUiThread();
        }

        /// <summary>
        ///     Determines whether the call is being made on the UI thread.
        /// </summary>
        /// <returns>Returns <see langword="true" /> if the call is on the UI thread, otherwise returns <see langword="false" />.</returns>
        public static bool CheckAccess()
        {
            var dispatcherForUiThread = DispatcherForUiThread;
            return dispatcherForUiThread != null && dispatcherForUiThread.CheckAccess();
        }

        public void Invoke(Action action)
        {
            using (GetInvocationWrapper())
            {
                if (CheckAccess())
                    action();
                else
                {
                    InvokeOnUIThread(new InvokableAction(action, true));
                    //action.OnUIThread();
                    //Application.Current?.Dispatcher.Invoke(action);
                }
            }
        }

        public static void ThrowIfNotOnUIThread([CallerMemberName] string callerMemberName = "")
        {
            if (!CheckAccess())
                throw new COMException(string.Format(CultureInfo.CurrentCulture, "{0} must be called on the UI thread.",
                    new object[]
                    {
                        callerMemberName
                    }), -2147417842);
        }

        public static void ThrowIfOnUIThread([CallerMemberName] string callerMemberName = "")
        {
            if (CheckAccess())
                throw new COMException(string.Format(CultureInfo.CurrentCulture,
                    "{0} must be called on a background thread.", new object[]
                    {
                        callerMemberName
                    }), -2147417842);
        }

        internal static void SetUiThread()
        {
            _uiThreadDispatcher = Dispatcher.CurrentDispatcher;
        }

        protected abstract IDisposable GetInvocationWrapper();

        private void InvokeOnUIThread(InvokableBase invokable)
        {
            var flag = false;
            Task<bool> task;
            do
            {
                var invoker = GetInvoker();
                var hr = invoker.Invoke(invokable);
                if (!flag && hr == -2147417848 && invokable.Exception == null)
                    flag = true;
                else if (hr >= 0)
                {
                    if (invokable.Exception != null)
                        throw WrapException(invokable.Exception);
                    break;
                }
                task = UiThreadReentrancyScope.TryExecuteActionAsyncInternal(invokable, 100);
            } while (!task.GetAwaiter().GetResult());
        }

        private static IInvokerPrivate GetInvoker()
        {
            return AppDispatcherInvoker.Instance;
        }

        private static Exception WrapException(Exception inner)
        {
            try
            {
                return (Exception)Activator.CreateInstance(inner.GetType(), inner.Message as object, inner as object);
            }
            catch
            {
                try
                {
                    return (Exception)Activator.CreateInstance(inner.GetType(), (object)inner);
                }
                catch
                {
                    return new TargetInvocationException(inner);
                }
            }
        }
    }

    internal sealed class AppDispatcherInvoker : IInvokerPrivate
    {
        private static IInvokerPrivate _instance;

        public static IInvokerPrivate Instance => _instance ??= new AppDispatcherInvoker();

        public int Invoke(IInvokablePrivate invokable)
        {
            return Application.Current.Dispatcher.Invoke(invokable.Invoke);
        }
    }

    internal class InvokableAction : InvokableBase
    {
        private readonly Action _a;
        private readonly ExecutionContextTrackerHelper.CapturedContext _context;

        public InvokableAction(Action a, bool captureContext = false)
        {
            _a = a;
            if (!captureContext)
                return;
            _context = ExecutionContextTrackerHelper.CaptureCurrentContext();
        }

        protected override void InvokeMethod()
        {
            if (_context != null)
                _context.ExecuteUnderContext(_a);
            else
                _a();
        }
    }

    internal abstract class InvokableBase : IInvokablePrivate
    {
        public Exception Exception { get; private set; }

        public int Invoke()
        {
            VerifyAccess();
            try
            {
                InvokeMethod();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }

            return 0;
        }

        protected abstract void InvokeMethod();

        private static void VerifyAccess()
        {
            if (!ThreadHelper.CheckAccess())
                throw new InvalidOperationException();
        }
    }

    public interface IInvokablePrivate
    {
        int Invoke();
    }

    public interface IInvokerPrivate
    {
        int Invoke(IInvokablePrivate pInvokable);
    }

    internal static class ExecutionContextTrackerHelper
    {
        private static IExecutionContextTracker _instance;

        public static IExecutionContextTracker Instance =>
            _instance ??= new ExecutionContextTracker();

        public static CapturedContext CaptureCurrentContext()
        {
            return new CapturedContext();
        }

        public static uint GetCurrentContext()
        {
            return Instance?.GetCurrentContext() ?? 0;
        }

        public class CapturedContext : IDisposable
        {
            private readonly uint _capturedContext;

            internal CapturedContext()
            {
                _capturedContext = 0U;
                if (Instance == null)
                    return;
                _capturedContext = Instance.GetCurrentContext();
            }

            public void Dispose()
            {
                Instance?.ReleaseContext(_capturedContext);
            }

            public void ExecuteUnderContext(Action t)
            {
                Instance?.PushContext(_capturedContext);
                t();
                Instance?.PopContext(_capturedContext);
            }
        }
    }

    public interface IExecutionContextTracker
    {
        void SetContextElement(Guid contextTypeGuid, Guid contextElementGuid);

        Guid SetAndGetContextElement(Guid contextTypeGuid, Guid contextElementGuid);

        Guid GetContextElement(Guid contextTypeGuid);

        void PushContext(uint contextCookie);

        void PopContext(uint contextCookie);

        uint GetCurrentContext();

        void ReleaseContext(uint contextCookie);

        void PushContextEx(uint contextCookie, bool fDontTrackAsyncWork);
    }

    internal sealed class ExecutionContextTracker : DisposableObject, IExecutionContextTracker
    {
        private readonly CookieTable<uint, ExecutionContextStorage> _contextCookies;
        private readonly ThreadLocal<ExecutionContextStorage> _noFlowContext;
        private readonly ThreadLocal<List<PendingNotification>> _pendingNotifications;
        private readonly object _syncLock;
        private readonly System.Threading.AsyncLocal<ExecutionContextStorage> _vsExecutionContextStore;
        private volatile HashSet<Tuple<Guid, IExecutionContextTrackerListener>> _listeners;

        public ExecutionContextTracker()
        {
            _contextCookies = new CookieTable<uint, ExecutionContextStorage>(UIntCookieTraits.Default);
            _noFlowContext = new ThreadLocal<ExecutionContextStorage> { Value = null };
            _pendingNotifications = new ThreadLocal<List<PendingNotification>>();
            _vsExecutionContextStore = new System.Threading.AsyncLocal<ExecutionContextStorage>(OnExecutionContextValueChanged);
            _listeners = new HashSet<Tuple<Guid, IExecutionContextTrackerListener>>();
            _syncLock = new object();
        }

        public Guid GetContextElement(Guid contextTypeGuid)
        {
            return (GetCurrentContextInternal() ?? ExecutionContextStorage.GetEmptyContext()).GetElement(
                contextTypeGuid);
        }

        public uint GetCurrentContext()
        {
            var newContext = GetCurrentContextInternal();
            if (newContext == null || newContext.IsEmpty)
                return 0;
            if (newContext.IsNoFlowContext)
                newContext = new ExecutionContextStorage(newContext.PreviousContext, newContext);
            return _contextCookies.Insert(newContext);
        }

        public void PopContext(uint contextCookie)
        {
            if (contextCookie == 0U || IsDisposed)
                return;
            var currentContextInternal = GetCurrentContextInternal();
            var previousContext = currentContextInternal?.PreviousContext;
            if (previousContext != null && _noFlowContext.Value != null && previousContext.IsNoFlowContext)
            {
                RaiseNotificationForContextChange(_noFlowContext.Value, previousContext);
                _noFlowContext.Value = previousContext;
            }
            else
            {
                if (_vsExecutionContextStore.Value == previousContext && currentContextInternal != null &&
                    currentContextInternal.IsNoFlowContext)
                    RaiseNotificationForContextChange(currentContextInternal, previousContext);
                else
                    _vsExecutionContextStore.Value = previousContext;
                _noFlowContext.Value = null;
            }
        }

        public void PushContext(uint contextCookie)
        {
            PushContextEx(contextCookie, false);
        }

        public void PushContextEx(uint contextCookie, bool dontTrackAsyncWork)
        {
            if (contextCookie == 0U || IsDisposed)
                return;
            if (!_contextCookies.TryGetValue(contextCookie, out var emptyContext))
                return;
            var currentContextInternal = GetCurrentContextInternal();
            var newContext = new ExecutionContextStorage(currentContextInternal, emptyContext);
            if (!dontTrackAsyncWork)
            {
                _vsExecutionContextStore.Value = newContext;
            }
            else
            {
                RaiseNotificationForContextChange(currentContextInternal, newContext);
                newContext.IsNoFlowContext = true;
                _noFlowContext.Value = newContext;
            }
        }

        public void Register(Guid contextValueType, IExecutionContextTrackerListener listener)
        {
            lock (_syncLock)
            {
                _listeners = new HashSet<Tuple<Guid, IExecutionContextTrackerListener>>(_listeners)
                {
                    Tuple.Create(contextValueType, listener)
                };
            }
        }

        public void ReleaseContext(uint cookieContext)
        {
            if (cookieContext == 0U)
                return;
            _contextCookies.Remove(cookieContext);
        }

        public Guid SetAndGetContextElement(Guid contextTypeGuid, Guid contextElementGuid)
        {
            if (IsDisposed)
                return Guid.Empty;
            var executionContextStorage1 = GetCurrentContextInternal() ?? ExecutionContextStorage.GetEmptyContext();
            var executionContextStorage2 =
                executionContextStorage1.UpdateElement(contextTypeGuid, contextElementGuid, out var previousValue);
            _vsExecutionContextStore.Value = executionContextStorage2.IsEmpty ? null : executionContextStorage2;
            _noFlowContext.Value = null;
            return previousValue;
        }

        public void SetContextElement(Guid contextTypeGuid, Guid contextElementGuid)
        {
            SetAndGetContextElement(contextTypeGuid, contextElementGuid);
        }

        public void Unregister(Guid contextValueType, IExecutionContextTrackerListener listener)
        {
            lock (_syncLock)
            {
                var tupleSet = new HashSet<Tuple<Guid, IExecutionContextTrackerListener>>(_listeners);
                tupleSet.Remove(Tuple.Create(contextValueType, listener));
                _listeners = tupleSet;
            }
        }

        private ExecutionContextStorage GetCurrentContextInternal()
        {
            if (IsDisposed)
                return ExecutionContextStorage.GetEmptyContext();
            if (_noFlowContext.Value != null)
                return _noFlowContext.Value;
            return _vsExecutionContextStore.Value;
        }

        private void OnExecutionContextValueChanged(AsyncLocalValueChangedArgs<ExecutionContextStorage> args)
        {
            if (IsDisposed)
                return;
            var previousValue = args.PreviousValue;
            if (_noFlowContext.Value != null)
                previousValue = _noFlowContext.Value;
            RaiseNotificationForContextChange(previousValue, args.CurrentValue);
        }

        private void RaiseNotificationForContextChange(ExecutionContextStorage oldContext, ExecutionContextStorage newContext)
        {
            var listeners = _listeners;
            if (listeners == null || listeners.Count == 0)
                return;
            foreach (var tuple in listeners)
            {
                var oldValue = oldContext?.GetElement(tuple.Item1) ?? Guid.Empty;
                var newValue = newContext?.GetElement(tuple.Item1) ?? Guid.Empty;
                if (oldValue != newValue)
                {
                    if (!_pendingNotifications.IsValueCreated)
                        _pendingNotifications.Value = new List<PendingNotification>();
                    _pendingNotifications.Value.Add(new PendingNotification(tuple.Item1, oldValue, newValue,
                        tuple.Item2));
                }
            }

            if (!_pendingNotifications.IsValueCreated)
                return;
            foreach (var pendingNotification in _pendingNotifications.Value)
                pendingNotification.Notify();
            _pendingNotifications.Value.Clear();
        }

        private struct PendingNotification
        {
            private readonly Guid _contextType;
            private readonly IExecutionContextTrackerListener _listener;
            private readonly Guid _newValue;
            private readonly Guid _oldValue;

            public PendingNotification(Guid type, Guid oldValue, Guid newValue, IExecutionContextTrackerListener listener)
            {
                _contextType = type;
                _oldValue = oldValue;
                _newValue = newValue;
                _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            }

            public void Notify()
            {
                _listener.OnExecutionContextValueChanged(_contextType, _oldValue, _newValue);
            }
        }
    }

    public interface IExecutionContextTrackerListener
    {
        void OnExecutionContextValueChanged(Guid contextValueType, Guid previousValue, Guid newValue);
    }

    internal class ExecutionContextStorage
    {
        private static readonly ExecutionContextStorage EmptyContext = new ExecutionContextStorage();
        private readonly List<Tuple<Guid, Guid>> _elements;

        public bool IsEmpty => _elements == null;

        public bool IsNoFlowContext { get; set; }

        public ExecutionContextStorage PreviousContext { get; }

        public ExecutionContextStorage(ExecutionContextStorage previousContext, ExecutionContextStorage newContext)
        {
            _elements = newContext._elements;
            PreviousContext = previousContext;
        }

        private ExecutionContextStorage()
        {
            PreviousContext = null;
        }

        private ExecutionContextStorage(ExecutionContextStorage previousContext, List<Tuple<Guid, Guid>> elements)
        {
            PreviousContext = previousContext;
            _elements = elements;
        }

        public static ExecutionContextStorage GetEmptyContext()
        {
            return EmptyContext;
        }

        public Guid GetElement(Guid elementType)
        {
            if (_elements == null)
                return Guid.Empty;
            foreach (var element in _elements)
                if (element.Item1 == elementType)
                    return element.Item2;
            return Guid.Empty;
        }

        public ExecutionContextStorage UpdateElement(Guid elementType, Guid value, out Guid previousValue)
        {
            GetElement(elementType);
            previousValue = Guid.Empty;
            if (value == Guid.Empty && (_elements == null || _elements.Count == 1))
                return EmptyContext;
            var elements = new List<Tuple<Guid, Guid>>();
            if (value != Guid.Empty)
                elements.Add(Tuple.Create(elementType, value));
            if (_elements != null)
                foreach (var element in _elements)
                    if (element.Item1 != elementType)
                        elements.Add(element);
                    else
                        previousValue = element.Item2;
            return new ExecutionContextStorage(PreviousContext, elements);
        }
    }

    public class CookieTable<TCookie, TValue> where TCookie : IComparable<TCookie>
    {
        private readonly object _syncLock = new object();
        private TCookie _currentCookie;
        private readonly CookieTraits<TCookie> _traits;
        private IDictionary<TCookie, TValue> _table;
        private PendingModifications _pendingMods;
        private uint _lockCount;

        private IDictionary<TCookie, TValue> Table
        {
            get => _table ??= new HybridDictionary<TCookie, TValue>();
            set => _table = value;
        }

        private PendingModifications PendingMods
        {
            get
            {
                if (!IsLocked)
                    throw new InvalidOperationException("Unlocked");
                return _pendingMods ??= new PendingModifications();
            }
            set => _pendingMods = value;
        }

        private bool HasPendingMods
        {
            get
            {
                var flag = _pendingMods != null;
                if (flag && !IsLocked)
                    throw new NotSupportedException("Locked");
                return flag;
            }
        }

        private TCookie NextCookie
        {
            get
            {
                lock (_syncLock)
                {
                    var table = Table;
                    var count = (uint)table.Count;
                    if (HasPendingMods)
                        count += (uint)PendingMods.PendingInsert.Count;
                    if (count >= _traits.UniqueCookies)
                        throw new ApplicationException("Empty");
                    do
                    {
                        _currentCookie = _traits.GetNextCookie(_currentCookie);
                    }
                    while (table.ContainsKey(_currentCookie) || HasPendingMods && PendingMods.PendingInsert.ContainsKey(_currentCookie));
                    return _currentCookie;
                }
            }
        }

        public CookieTable(CookieTraits<TCookie> traits)
        {
            _traits = traits;
            _currentCookie = _traits.InvalidCookie;
        }

        public TCookie Insert(TValue value)
        {
            lock (_syncLock)
            {
                using (new CookieTableLock(this))
                {
                    var nextCookie = NextCookie;
                    PendingMods.PendingInsert.Add(nextCookie, value);
                    return nextCookie;
                }
            }
        }

        public bool Remove(TCookie cookie)
        {
            lock (_syncLock)
            {
                var flag = false;
                using (new CookieTableLock(this))
                {
                    if (Table.ContainsKey(cookie) && !IsPendingDelete(cookie))
                    {
                        PendingMods.PendingDelete.Add(cookie);
                        flag = true;
                    }
                    if (!flag)
                    {
                        if (HasPendingMods)
                        {
                            if (PendingMods.PendingInsert.ContainsKey(cookie))
                            {
                                PendingMods.PendingInsert.Remove(cookie);
                                flag = true;
                            }
                        }
                    }
                }
                return flag;
            }
        }

        public void Clear()
        {
            lock (_syncLock)
            {
                using (new CookieTableLock(this))
                {
                    PendingMods.Reset();
                    PendingMods.PendingClear = true;
                }
            }
        }

        public bool ContainsCookie(TCookie cookie)
        {
            lock (_syncLock)
                return Table.ContainsKey(cookie);
        }

        public void ForEach(CookieTableCallback<TCookie, TValue> callback, bool skipRemoved)
        {
            using (new CookieTableLock(this))
            {
                Dictionary<TCookie, TValue> dictionary;
                lock (_syncLock)
                    dictionary = new Dictionary<TCookie, TValue>(Table);
                foreach (var keyValuePair in dictionary)
                {
                    if (!(IsPendingDelete(keyValuePair.Key) & skipRemoved))
                        callback(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        public void ForEach(CookieTableCallback<TCookie, TValue> callback)
        {
            ForEach(callback, true);
        }

        private bool IsPendingDelete(TCookie cookie)
        {
            lock (_syncLock)
            {
                if (!HasPendingMods)
                    return false;
                if (PendingMods.PendingClear)
                    return true;
                return PendingMods.PendingDelete.Contains(cookie);
            }
        }

        public bool TryGetValue(TCookie cookie, out TValue value)
        {
            lock (_syncLock)
            {
                var flag = Table.TryGetValue(cookie, out value);
                if (flag && IsLocked && IsPendingDelete(cookie))
                    flag = false;
                return flag;
            }
        }

        public ICollection<TCookie> Cookies
        {
            get
            {
                lock (_syncLock)
                {
                    var array = new TCookie[Table.Count];
                    Table.Keys.CopyTo(array, 0);
                    return array;
                }
            }
        }

        public TValue this[TCookie cookie]
        {
            get
            {
                if (!TryGetValue(cookie, out var obj))
                    throw new ArgumentException("Invalid cookie", nameof(cookie));
                return obj;
            }
        }

        public uint Size => (uint)Table.Count;

        public uint PendingSize
        {
            get
            {
                lock (_syncLock)
                {
                    var num = Size;
                    if (IsLocked && HasPendingMods)
                        num = (!PendingMods.PendingClear ? num - (uint)PendingMods.PendingDelete.Count : 0U) + (uint)PendingMods.PendingInsert.Count;
                    return num;
                }
            }
        }

        public uint MaxSize => _traits.UniqueCookies;

        public void Lock()
        {
            lock (_syncLock)
                _lockCount = _lockCount + 1U;
        }

        public void Unlock()
        {
            lock (_syncLock)
            {
                if ((int)_lockCount == 0)
                    throw new InvalidOperationException("Unlocked");
                try
                {
                    if ((int)_lockCount != 1 || !HasPendingMods)
                        return;
                    if (PendingMods.PendingClear)
                    {
                        Table = null;
                    }
                    else
                    {
                        foreach (var key in PendingMods.PendingDelete)
                            Table.Remove(key);
                        if (Table.Count == 0)
                            Table = null;
                    }
                    foreach (var keyValuePair in PendingMods.PendingInsert)
                    {
                        Table?.Add(keyValuePair);
                    }
                    PendingMods = null;
                }
                finally
                {
                    _lockCount = _lockCount - 1U;
                }
            }
        }

        public bool IsLocked
        {
            get
            {
                lock (_syncLock)
                    return _lockCount > 0U;
            }
        }

        private struct CookieTableLock : IDisposable
        {
            private readonly CookieTable<TCookie, TValue> _cookieTable;

            public CookieTableLock(CookieTable<TCookie, TValue> cookieTable)
            {
                _cookieTable = cookieTable;
                _cookieTable.Lock();
            }

            public void Dispose()
            {
                _cookieTable.Unlock();
            }
        }

        private class PendingModifications
        {
            public readonly IDictionary<TCookie, TValue> PendingInsert = new HybridDictionary<TCookie, TValue>();
            public readonly IList<TCookie> PendingDelete = new List<TCookie>();
            public bool PendingClear;

            public void Reset()
            {
                PendingClear = false;
                PendingDelete.Clear();
                PendingInsert.Clear();
            }
        }
    }

    internal class HybridDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private ICollection<KeyValuePair<TKey, TValue>> _inner;
        private readonly IEqualityComparer<TKey> _keyComparer;
        internal const int CutoverPoint = 32;

        public HybridDictionary()
            : this(0, null)
        {
        }

        public HybridDictionary(IEqualityComparer<TKey> keyComparer)
            : this(0, keyComparer)
        {
        }

        [Obsolete("Use the custructor that takes a custom comparer instead.")]
        public HybridDictionary(bool caseInsensitive)
            : this(0, caseInsensitive)
        {
        }

        public HybridDictionary(int capacity)
            : this(capacity, null)
        {
        }

        [Obsolete("Use the custructor that takes a custom comparer instead.")]
        public HybridDictionary(int capacity, bool caseInsensitive)
            : this(capacity, caseInsensitive ? (IEqualityComparer<TKey>)StringComparer.OrdinalIgnoreCase : null)
        {
        }

        public HybridDictionary(int capacity, IEqualityComparer<TKey> keyComparer)
        {
            if (capacity < 0)
                throw new IndexOutOfRangeException(nameof(capacity));
            _inner = capacity <= 32 ? (capacity == 0 ? null : new List<KeyValuePair<TKey, TValue>>(capacity)) : (ICollection<KeyValuePair<TKey, TValue>>)new Dictionary<TKey, TValue>(capacity, keyComparer);
            _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
        }

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        public bool ContainsKey(TKey key)
        {
            if (_inner == null)
                return false;
            Dictionary<TKey, TValue> asDictionary = AsDictionary;
            if (asDictionary != null)
                return asDictionary.ContainsKey(key);
            return IndexOfKey(List, key) != -1;
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (_inner == null)
                    return new TKey[0];
                Dictionary<TKey, TValue> asDictionary = AsDictionary;
                if (asDictionary != null)
                    return asDictionary.Keys;
                return _inner.Select(kvp => kvp.Key).ToArray();
            }
        }

        public bool Remove(TKey key)
        {
            if (_inner == null)
                return false;
            Dictionary<TKey, TValue> asDictionary = AsDictionary;
            if (asDictionary != null)
                return asDictionary.Remove(key);
            List<KeyValuePair<TKey, TValue>> list = List;
            int index = IndexOfKey(list, key);
            if (index < 0)
                return false;
            list.RemoveAt(index);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_inner == null)
            {
                value = default(TValue);
                return false;
            }
            Dictionary<TKey, TValue> asDictionary = AsDictionary;
            if (asDictionary != null)
                return asDictionary.TryGetValue(key, out value);
            List<KeyValuePair<TKey, TValue>> list = List;
            int index = IndexOfKey(list, key);
            if (index < 0)
            {
                value = default(TValue);
                return false;
            }
            value = list[index].Value;
            return true;
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (_inner == null)
                    return new TValue[0];
                Dictionary<TKey, TValue> asDictionary = AsDictionary;
                if (asDictionary != null)
                    return asDictionary.Values;
                return _inner.Select(kvp => kvp.Value).ToArray();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (_inner == null)
                    throw new KeyNotFoundException();
                Dictionary<TKey, TValue> asDictionary = AsDictionary;
                if (asDictionary != null)
                    return asDictionary[key];
                List<KeyValuePair<TKey, TValue>> list = List;
                int index = IndexOfKey(list, key);
                if (index < 0)
                    throw new KeyNotFoundException();
                return list[index].Value;
            }
            set => Insert(key, value, false);
        }

        private Dictionary<TKey, TValue> AsDictionary => _inner as Dictionary<TKey, TValue>;

        private List<KeyValuePair<TKey, TValue>> List => (List<KeyValuePair<TKey, TValue>>)_inner;

        private int IndexOfKey(List<KeyValuePair<TKey, TValue>> list, TKey key)
        {
            for (int index = 0; index < list.Count; ++index)
            {
                if (_keyComparer.Equals(list[index].Key, key))
                    return index;
            }
            return -1;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (_inner == null)
            {
                _inner = new List<KeyValuePair<TKey, TValue>>()
                {
                    new KeyValuePair<TKey, TValue>(key, value)
                };
            }
            else
            {
                Dictionary<TKey, TValue> asDictionary = AsDictionary;
                if (asDictionary != null)
                {
                    if (add)
                        asDictionary.Add(key, value);
                    else
                        asDictionary[key] = value;
                }
                else
                {
                    List<KeyValuePair<TKey, TValue>> list = List;
                    int index = IndexOfKey(list, key);
                    if (index >= 0)
                    {
                        if (add)
                            throw new ArgumentException("Adding a duplicate key");
                        list[index] = new KeyValuePair<TKey, TValue>(key, value);
                    }
                    else if (list.Count < 32)
                        list.Add(new KeyValuePair<TKey, TValue>(key, value));
                    else
                        UpgradeToDictionary(list).Add(key, value);
                }
            }
        }

        private Dictionary<TKey, TValue> UpgradeToDictionary(List<KeyValuePair<TKey, TValue>> list)
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(33, _keyComparer);
            foreach (KeyValuePair<TKey, TValue> keyValuePair in list)
                dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            list.Clear();
            _inner = dictionary;
            return dictionary;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _inner = null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!TryGetValue(item.Key, out var obj))
                return false;
            if (obj is IEquatable<TValue> equatable)
                return equatable.Equals(item.Value);
            if (obj is IComparable<TValue> comparable)
                return comparable.CompareTo(item.Value) == 0;
            return obj.Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _inner?.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                if (_inner != null)
                    return _inner.Count;
                return 0;
            }
        }

        public bool IsReadOnly => false;

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _inner != null && _inner.Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (_inner != null)
                return _inner.GetEnumerator();
            return Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public delegate void CookieTableCallback<in TCookie, in TValue>(TCookie cookie, TValue value);

    public abstract class CookieTraits<T> where T : IComparable<T>
    {
        protected CookieTraits(T min, T max, T invalid)
        {
            if (min.CompareTo(max) >= 0)
                throw new ArgumentException("Range");
            if (invalid.CompareTo(min) >= 0 && invalid.CompareTo(max) <= 0)
                throw new ArgumentException("Range");
            MinCookie = min;
            MaxCookie = max;
            InvalidCookie = invalid;
        }

        public T InvalidCookie { get; }

        public T MinCookie { get; }

        public T MaxCookie { get; }

        public T GetNextCookie(T current)
        {
            if (current.CompareTo(MaxCookie) < 0 && current.CompareTo(MinCookie) >= 0)
                return IncrementValue(current);
            return MinCookie;
        }

        public abstract T IncrementValue(T current);

        public abstract uint UniqueCookies { get; }
    }

    public class UIntCookieTraits : CookieTraits<uint>
    {
        public static UIntCookieTraits Default = new UIntCookieTraits();

        public UIntCookieTraits() : this(1U, uint.MaxValue, 0U)
        {
        }

        public UIntCookieTraits(uint min, uint max, uint invalid) : base(min, max, invalid)
        {
        }

        public override uint IncrementValue(uint current)
        {
            return checked(current + 1U);
        }

        public override uint UniqueCookies => (uint)((int)MaxCookie - (int)MinCookie + 1);
    }

    public static class UiThreadReentrancyScope
    {
        private static readonly object LockObj = new object();
        private static readonly Queue<PendingRequest> Queue = new Queue<PendingRequest>();
        private static TaskCompletionSource<object> _queueHasElement = new TaskCompletionSource<object>();

        private static Task RequestWaiter
        {
            get
            {
                lock (LockObj)
                {
                    return _queueHasElement.Task;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static async Task EnqueueActionAsync(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (ThreadHelper.CheckAccess())
                await TaskScheduler.Default.SwitchTo();
            var pendingRequest = new PendingRequest(new InvokableAction(action), false);
            lock (LockObj)
            {
                Queue.Enqueue(pendingRequest);
                _queueHasElement.TrySetResult(null);
            }
        }

        public static bool WaitOnTaskComplete(Task task, CancellationToken cancel, int ms)
        {
            return !ThreadHelper.CheckAccess() ? task.Wait(ms, cancel) : WaitOnTaskCompleteInternal(task, cancel, ms);
        }

        internal static async Task<bool> TryExecuteActionAsyncInternal(InvokableBase action, int timeout = -1)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            ThreadHelper.ThrowIfOnUIThread(nameof(TryExecuteActionAsyncInternal));
            var pr = new PendingRequest(action, true);
            lock (LockObj)
            {
                Queue.Enqueue(pr);
                _queueHasElement.TrySetResult(null);
            }

            if (timeout != -1)
            {
                var delayCancellation = new CancellationTokenSource();
                await Task.WhenAny(pr.Waiter, Task.Delay(timeout, delayCancellation.Token));
                delayCancellation.Cancel();
            }
            else
            {
                var _ = await pr.Waiter ? 1 : 0;
            }

            return await Dequeue(pr);
        }

        private static void ClearQueue()
        {
            lock (LockObj)
            {
                if (Queue.Count == 0)
                    return;
                List<PendingRequest> pendingRequestList = null;
                while (Queue.Count > 0)
                {
                    var pendingRequest = Queue.Dequeue();
                    if (!pendingRequest.AllowCleanup)
                    {
                        if (pendingRequestList == null)
                            pendingRequestList = new List<PendingRequest>();
                        pendingRequestList.Add(pendingRequest);
                    }
                    else
                    {
                        pendingRequest.SkipExecution().Forget();
                    }
                }

                if (pendingRequestList != null)
                {
                    pendingRequestList.Reverse();
                    foreach (var pendingRequest in pendingRequestList)
                        Queue.Enqueue(pendingRequest);
                }

                if (Queue.Count != 0)
                    return;
                _queueHasElement.TrySetResult(null);
                _queueHasElement = new TaskCompletionSource<object>();
            }
        }

        private static Task<bool> Dequeue(PendingRequest pr)
        {
            Task<bool> task;
            lock (LockObj)
            {
                task = pr.SkipExecution();
                while (Queue.Count > 0 && Queue.Peek().Revoked)
                    Queue.Dequeue();
                if (Queue.Count == 0)
                {
                    _queueHasElement.TrySetResult(null);
                    _queueHasElement = new TaskCompletionSource<object>();
                }
            }

            return task;
        }

        private static bool ExecuteOne()
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(ExecuteOne));
            TaskCompletionSource<bool> completeEvent = null;
            InvokableBase action = null;
            lock (LockObj)
            {
                PendingRequest pendingRequest = null;
                while (Queue.Count > 0 && pendingRequest == null)
                {
                    pendingRequest = Queue.Dequeue();
                    if (pendingRequest.Revoked)
                        pendingRequest = null;
                }

                pendingRequest?.InitiateExecute(out completeEvent, out action);
                if (Queue.Count == 0)
                {
                    _queueHasElement.TrySetResult(null);
                    _queueHasElement = new TaskCompletionSource<object>();
                }
            }

            if (completeEvent == null)
                return false;
            var num = action.Invoke();
            if (num >= 0)
                completeEvent.TrySetResult(true);
            else
                completeEvent.TrySetException(Marshal.GetExceptionForHR(num));
            return true;
        }

        private static void Flush()
        {
            do
            {
                ;
            } while (ExecuteOne());
        }

        private static bool WaitOnTaskCompleteInternal(Task task, CancellationToken cancel, int ms)
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(WaitOnTaskCompleteInternal));
            var tasks = new[]
            {
                task,
                null
            };
            bool flag2;
            while (!task.IsCompleted)
            {
                tasks[1] = RequestWaiter;
                var stopwatch = Stopwatch.StartNew();
                switch (Task.WaitAny(tasks, ms, cancel))
                {
                    case 0:
                        task.Wait(ms, cancel);
                        break;
                    case 1:
                        Flush();
                        break;
                    default:
                        break;
                }

                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                if (ms != -1)
                {
                    if (elapsedMilliseconds < 0L || elapsedMilliseconds >= ms)
                    {
                        flag2 = false;
                        goto label_11;
                    }

                    ms -= (int)elapsedMilliseconds;
                }
            }

            task.GetAwaiter().GetResult();
            flag2 = true;
            label_11:
            ClearQueue();
            return flag2;
        }

        internal class PendingRequest
        {
            internal bool AllowCleanup { get; }

            internal bool Revoked => InvokeAction == null;

            internal Task<bool> Waiter => WorkCompleteEvent.Task;

            private InvokableBase InvokeAction { get; set; }

            private bool Started { get; set; }
            private TaskCompletionSource<bool> WorkCompleteEvent { get; }

            internal PendingRequest(InvokableBase action, bool guaranteeExecution)
            {
                InvokeAction = action;
                Started = false;
                WorkCompleteEvent = new TaskCompletionSource<bool>();
                AllowCleanup = !guaranteeExecution;
            }

            internal void InitiateExecute(out TaskCompletionSource<bool> completeEvent, out InvokableBase action)
            {
                if (!Revoked)
                {
                    completeEvent = WorkCompleteEvent;
                    action = InvokeAction;
                    Started = true;
                }
                else
                {
                    completeEvent = null;
                    action = null;
                }

                InvokeAction = null;
            }

            internal Task<bool> SkipExecution()
            {
                if (!Started)
                {
                    WorkCompleteEvent.TrySetResult(false);
                    InvokeAction = null;
                }

                return Waiter;
            }
        }
    }
}
