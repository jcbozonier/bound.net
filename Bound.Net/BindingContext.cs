using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Bound.Net
{
    public class BindingContext
    {
        private readonly INotifyPropertyChanged _PropertyMother;
        private readonly Func<PropertyChangedEventHandler> _PropertyEventProvider;

        public BindingContext(
            INotifyPropertyChanged propertyMotherToWatch, 
            Func<PropertyChangedEventHandler> propertyEventProvider)
        {
            _PropertyMother = propertyMotherToWatch;
            _PropertyEventProvider = propertyEventProvider;
        }

        /// <summary>
        /// Creates a partial binding with the given property. This is the property
        /// that mirrors the property we're binding to.
        /// </summary>
        /// <param name="propertyToNotify"></param>
        /// <returns></returns>
        public PartialBinding Bind(Expression<Func<object>> propertyToNotify)
        {
            return new PartialBinding(_PropertyEventProvider, _PropertyMother, propertyToNotify);
        }
    }
}
