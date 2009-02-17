using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Bound.Net
{
    /// <summary>
    /// Used to subscribe a set of event handlers to a given event.
    /// </summary>
    public class Subscription
    {
        public event PropertyChangedEventHandler ChangeHandlers;

        public void FireChangeHandlers(object sender, PropertyChangedEventArgs args)
        {
            if (ChangeHandlers != null)
                ChangeHandlers(sender, args);
        }

        public void OnChangeDo(PropertyChangedEventHandler evt)
        {
            ChangeHandlers += evt;
        }
    }
}
