using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using ReactNative.Modules.Core;
using ReactNative.Shell;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ReactNative
{
    /// <summary>
    /// Base user control class for React Native embedded applications. <typeparamref name="T"/> is
    /// a native module that optionally implements an object model to be bound to the control. 
    /// </summary>
    /// <typeparam name="T">The native module object model type.</typeparam>
    public abstract class ReactUserControl<T> : UserControl, Bridge.IAsyncDisposable where T : ReactContextNativeModuleBase, new()
    {
        private ReactInstanceManager _reactInstanceManager;

        private bool _isShiftKeyDown;
        private bool _isControlKeyDown;

        /// <summary>
        /// Access the internal object model that is used to bind XAML properties with data.
        /// </summary>
        protected UserControlPackage<T> NativePackage { get; private set; }

        /// <summary>
        /// Instantiates the <see cref="ReactUserControl{T}"/>
        /// </summary>
        protected ReactUserControl()
        {
            CreateNewInstance();
        }

        private void CreateNewInstance()
        {
            _reactInstanceManager = CreateReactInstanceManager();
            // The react context is created and re-created in the background on a private async
            // that we cannot wait on, instead we must use event handling to informed when it is complete
            _reactInstanceManager.ReactContextInitialized += OnReactContextInitialized;
            RootView = CreateRootView();
            Content = RootView;
        }

        /// <summary>
        /// Virtual event handler called when the React context is initialized.
        /// </summary>
        /// <param name="sender">The origin of the event.</param>
        /// <param name="e">The arguments for the event of type <see cref="ReactContextInitializedEventArgs"/>.</param>
        protected virtual void OnReactContextInitialized(object sender, ReactContextInitializedEventArgs e)
        {
        }

        /// <summary>
        /// The custom path of the bundle file.
        /// </summary>
        /// <remarks>
        /// This is used in cases where the bundle should be loaded from a
        /// custom path.
        /// </remarks>
        public virtual string JavaScriptBundleFile
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The name of the main module.
        /// </summary>
        /// <remarks>
        /// This is used to determine the URL used to fetch the JavaScript
        /// bundle from the packager server. It is only used when dev support
        /// is enabled.
        /// </remarks>
        public virtual string JavaScriptMainModuleName
        {
            get
            {
                return "index.windows";
            }
        }

        /// <summary>
        /// Instantiates the JavaScript executor.
        /// </summary>
        public virtual Func<IJavaScriptExecutor> JavaScriptExecutorFactory
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The name of the main component registered from JavaScript.
        /// </summary>
        public abstract string MainComponentName { get; }

        /// <summary>
        /// Signals whether developer mode should be enabled.
        /// </summary>
        public abstract bool UseDeveloperSupport { get; }

        /// <summary>
        /// The list of <see cref="IReactPackage"/>s used by the application.
        /// </summary>
        public virtual List<IReactPackage> Packages
        {
            get
            {
                NativePackage = new UserControlPackage<T>();
                return new List<IReactPackage>
                {
                    new MainReactPackage(),
                    NativePackage
                };
            }
        }

        /// <summary>
        /// The root view managed by the page.
        /// </summary>
        public ReactRootView RootView { get; set; }

        /// <summary>
        /// Called when the application is first initialized.
        /// </summary>
        /// <param name="arguments">The launch arguments.</param>
        public void OnCreate(string arguments)
        {
            OnCreate(arguments, default(JObject));
        }

        /// <summary>
        /// Called when the application is first initialized.
        /// </summary>
        /// <param name="arguments">The launch arguments.</param>
        /// <param name="initialProps">The initialProps.</param>
        public void OnCreate(string arguments, JObject initialProps)
        {
            RootView.Background = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];

            ApplyArguments(arguments);
            RootView.StartReactApplication(_reactInstanceManager, MainComponentName, initialProps);

            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, args) =>
            {
                _reactInstanceManager.OnBackPressed();
                args.Handled = true;
            };

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += OnAcceleratorKeyActivated;
        }

        /// <summary>
        /// Called to reload a new bundle, asynchronously.
        /// </summary>
        public async Task OnNewReactBundle()
        {
            // Reset react to point to a new bundle
            Content = null;
            RootView = null;
            await _reactInstanceManager.DisposeAsync();
            _reactInstanceManager = null;
            NativePackage = null;

            CreateNewInstance();

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= OnAcceleratorKeyActivated;
            OnCreate(null);
        }

        /// <summary>
        /// Called before the application is suspended.
        /// </summary>
        public void OnSuspend()
        {
            _reactInstanceManager.OnSuspend();
        }

        /// <summary>
        /// Called when the application is resumed.
        /// </summary>
        /// <param name="onBackPressed">
        /// Default action to take when back pressed.
        /// </param>
        public void OnResume(Action onBackPressed)
        {
            _reactInstanceManager.OnResume(onBackPressed);
        }

        /// <summary>
        /// Called before the application shuts down.
        /// </summary>
        public Task DisposeAsync()
        {
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= OnAcceleratorKeyActivated;

            return _reactInstanceManager.DisposeAsync();
        }

        /// <summary>
        /// Creates the React root view.
        /// </summary>
        /// <returns>The root view.</returns>
        /// <remarks>
        /// Subclasses may override this method if it needs to use a custom
        /// root view.
        /// </remarks>
        protected virtual ReactRootView CreateRootView()
        {
            return new ReactRootView();
        }

        /// <summary>
        /// Captures the all key downs and Ups. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
        {
            if (_reactInstanceManager.DevSupportManager.IsEnabled)
            {
                if (e.VirtualKey == VirtualKey.Shift)
                {
                    _isShiftKeyDown = e.EventType == CoreAcceleratorKeyEventType.KeyDown;
                }
                else if (e.VirtualKey == VirtualKey.Control)
                {
                    _isControlKeyDown = e.EventType == CoreAcceleratorKeyEventType.KeyDown;
                }
                else if ((_isShiftKeyDown && e.VirtualKey == VirtualKey.F10) ||
                          (e.EventType == CoreAcceleratorKeyEventType.KeyDown && e.VirtualKey == VirtualKey.Menu))
                {
                    _reactInstanceManager.DevSupportManager.ShowDevOptionsDialog();
                }
                else if (e.EventType == CoreAcceleratorKeyEventType.KeyUp && _isControlKeyDown && e.VirtualKey == VirtualKey.R)
                {
                    _reactInstanceManager.DevSupportManager.HandleReloadJavaScript();
                }
            }
        }

        private ReactInstanceManager CreateReactInstanceManager()
        {
            var builder = new ReactInstanceManagerBuilder
            {
                UseDeveloperSupport = UseDeveloperSupport,
                InitialLifecycleState = Common.LifecycleState.BeforeCreate,
                JavaScriptBundleFile = JavaScriptBundleFile,
                JavaScriptMainModuleName = JavaScriptMainModuleName,
                JavaScriptExecutorFactory = JavaScriptExecutorFactory,
            };

            builder.Packages.AddRange(Packages);
            return builder.Build();
        }

        private void ApplyArguments(string arguments)
        {
            if (!string.IsNullOrEmpty(arguments))
            {
                var args = arguments.Split(',');

                var index = Array.IndexOf(args, "remoteDebugging");
                if (index < 0)
                {
                    return;
                }

                if (args.Length <= index + 1)
                {
                    throw new ArgumentException("Expected value for remoteDebugging argument.", nameof(arguments));
                }

                bool isRemoteDebuggingEnabled;
                if (bool.TryParse(args[index + 1], out isRemoteDebuggingEnabled))
                {
                    _reactInstanceManager.DevSupportManager.IsRemoteDebuggingEnabled = isRemoteDebuggingEnabled;
                }
            }
        }
    }
}
