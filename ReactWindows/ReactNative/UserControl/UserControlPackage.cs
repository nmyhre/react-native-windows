using ReactNative.Bridge;
using ReactNative.Modules.Core;
using ReactNative.UIManager;
using System;
using System.Collections.Generic;

namespace ReactNative
{
    /// <summary>
    /// React package for the user control that takes a custom native object model of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The native module object model type.</typeparam>
    public class UserControlPackage<T> : IReactPackage where T: ReactContextNativeModuleBase, new()
    {
        /// <summary>
        /// Getter/setter for the native object model.
        /// </summary>
        public T NativeObjectModel { get; private set; }

        /// <summary>
        /// Implementation of Native modules creation.
        /// </summary>
        public IReadOnlyList<INativeModule> CreateNativeModules(ReactContext reactContext)
        {
            NativeObjectModel = Activator.CreateInstance(typeof(T), reactContext) as T;
            return new List<INativeModule> { NativeObjectModel };
        }

        /// <summary>
        /// Implementation of JavaScript modules creation, of which there are none.
        /// </summary>
        public IReadOnlyList<Type> CreateJavaScriptModulesConfig()
        {
            return new List<Type>(0);
        }

        /// <summary>
        /// Implementation of view managers creation, of which there are none.
        /// </summary>
        public IReadOnlyList<IViewManager> CreateViewManagers(
            ReactContext reactContext, Uri sourceuri)
        {
            return new List<IViewManager>(0);
        }
    }
}
