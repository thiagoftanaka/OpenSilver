﻿
/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

namespace System.Windows
{
    /// <summary>
    /// Contains state information and event data associated with a routed event.
    /// </summary>
    public class RoutedEventArgs : EventArgs
    {
        private object _originalSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedEventArgs"/> class.
        /// </summary>
        public RoutedEventArgs() { }

        /// <summary>
        /// Gets a reference to the object that raised the event.
        /// </summary>
        public object OriginalSource
        {
            get { return _originalSource; }
            internal set
            {
                if (value == null || value is not DependencyObject)
                {
                    throw new ArgumentException();
                }

                _originalSource = value;
            }
        }

        /// <summary>
        /// Returns the <see cref="RoutedEvent"/> associated
        /// with this <see cref="RoutedEventArgs"/>
        /// </summary>
        internal RoutedEvent RoutedEvent { get; set; }

        internal bool HandledImpl { get; set; }

        // Calls the InvokeEventHandler protected
        // virtual method
        //
        // This method is needed because
        // delegates are invoked from
        // RoutedEventHandler which is not a
        // sub-class of RoutedEventArgs
        // and hence cannot invoke protected
        // method RoutedEventArgs.FireEventHandler
        internal virtual void InvokeHandler(Delegate handler, object target)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (handler is RoutedEventHandler)
            {
                ((RoutedEventHandler)handler)(target, this);
            }
            else
            {
                // Restricted Action - reflection permission required
                handler.DynamicInvoke(new object[] { target, this });
            }
        }

        internal object UIEventArg { get; set; }

        internal bool Cancellable { get; set; } = true;

        internal void PreventDefault()
        {
            if (!Cancellable)
            {
                return;
            }

            if (UIEventArg != null)
            {
                OpenSilver.Interop.ExecuteJavaScriptVoid(
                    $"{OpenSilver.Interop.GetVariableStringForJS(UIEventArg)}.preventDefault();");
            }
        }
    }
}
