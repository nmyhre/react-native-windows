using ReactNative;
using ReactNative.Bridge;
using ReactNative.Modules.Core;
using ReactNative.Shell;
using System.Collections.Generic;

namespace Playground
{
    internal class ReactControlObjectModel : UserControlNativeObjectModel<string>
    {
        public ReactControlObjectModel() : base(null) { }

        public ReactControlObjectModel(ReactContext reactContext) : base(reactContext) { }
#pragma warning disable 1998
        [ReactMethod]
        public override async void GetData(IPromise promise)
        {
            base.GetData(promise);
        }
#pragma warning restore 1998
    }

    class AppReactPage : ReactUserControl<ReactControlObjectModel>
    {
        public override string MainComponentName
        {
            get
            {
                return "Playground";
            }
        }

        public override string JavaScriptMainModuleName
        {
            get
            {
                return "ReactWindows/Playground/index.windows";
            }
        }

#if BUNDLE
        public override string JavaScriptBundleFile
        {
            get
            {
                return "ms-appx:///ReactAssets/index.windows.bundle";
            }
        }
#endif

        public override List<IReactPackage> Packages
        {
            get
            {
                return new List<IReactPackage>
                {
                    new MainReactPackage(),
                };
            }
        }

        public override bool UseDeveloperSupport
        {
            get
            {
#if !BUNDLE || DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
