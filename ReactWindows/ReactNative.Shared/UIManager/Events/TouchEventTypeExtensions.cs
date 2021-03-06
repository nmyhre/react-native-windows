// Copyright (c) Microsoft Corporation. All rights reserved.
// Portions derived from React Native:
// Copyright (c) 2015-present, Facebook, Inc.
// Licensed under the MIT License.

using System;

namespace ReactNative.UIManager.Events
{
    static class TouchEventTypeExtensions
    {
        public static string GetJavaScriptEventName(this TouchEventType eventType)
        {
            switch (eventType)
            {
                case TouchEventType.Start:
                    return "topTouchStart";
                case TouchEventType.End:
                    return "topTouchEnd";
                case TouchEventType.Move:
                    return "topTouchMove";
                case TouchEventType.Cancel:
                    return "topTouchCancel";
                case TouchEventType.Entered:
                    return "topMouseOver";
                case TouchEventType.Exited:
                    return "topMouseOut";
                case TouchEventType.Wheel:
                    return "topWheelChanged";
                default:
                    throw new NotSupportedException("Unsupported touch event type.");
            }
        }
    }
}
