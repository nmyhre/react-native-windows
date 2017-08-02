using ReactNative.Bridge;
using ReactNative.Modules.Core;
using System.Collections.Generic;

namespace ReactNative
{
    /// <summary>
    /// User control native object module of user defined type <typeparamref name="T"/> that 
    /// can be used for simple XAML data binding.
    /// </summary>
    /// <typeparam name="T">The type of the data to bind and share across JavaScript to React.</typeparam>
    public class UserControlNativeObjectModel<T> : ReactContextNativeModuleBase, ILifecycleEventListener where T : class
    {
        private const string OnDataChangedEvent = "OnDataChangedEvent";

        /// <summary>
        /// Constructor that passes on the react context to the base class
        /// </summary>
        /// <param name="reactContext">The react context.</param>
        public UserControlNativeObjectModel(ReactContext reactContext) : base (reactContext) { }

        private T _data;
        /// <summary>
        /// Getter/setter of user defined type <typeparamref name="T"/> that holds a copy
        /// of the currently shared data.
        /// </summary>
        public T Data
        {
            get { return _data; }
            set
            {
                if (value != _data)
                {
                    _data = value;
                    // if the value changed we should send an event to any running script
                    Context.GetJavaScriptModule<RCTDeviceEventEmitter>().emit(OnDataChangedEvent, "");
                }
            }
        }

        /// <summary>
        /// The name of the native module in the JavaScript namespace.
        /// </summary>
        public override string Name
        {
            get
            {
                return "NativeObjectModel";
            }
        }

        private readonly Dictionary<string, object> _constants = new Dictionary<string, object>
            {
                { nameof(OnDataChangedEvent), OnDataChangedEvent },
            };

        /// <summary>
        /// Return the dictionary mapping from string name -> data.
        /// </summary>
        public override IReadOnlyDictionary<string, object> Constants
        {
            get
            {
                return _constants;
            }
        }

#pragma warning disable 1998
        /// <summary>
        /// Return the current data across the bridge in the promise. This
        /// needs to be done asynchronously
        /// </summary>
        /// <param name="promise">An already constructed promise that we can use to pass data back to JavaScript.</param>
        public virtual async void GetData(IPromise promise)
        {
            promise.Resolve(_data);
        }
#pragma warning restore 1998

        /// <summary>
        /// Handle OnSuspend by simply passing it along to our context.
        /// </summary>
        public virtual void OnSuspend()
        {
            Context.OnSuspend();
        }

        /// <summary>
        /// Handle OnResume by simply passing it along to our context.
        /// </summary>
        public virtual void OnResume()
        {
            Context.OnResume();
        }

        /// <summary>
        /// Handle OnDestroy. Nothing to do here
        /// </summary>
        public virtual void OnDestroy()
        {
        }
    }
}
