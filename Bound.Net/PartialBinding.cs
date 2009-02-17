using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Bound.Net
{
    /// <summary>
    /// 
    /// </summary>
    public class PartialBinding
    {
        private readonly Expression<Func<object>> _PropertyToNotify;
        private readonly INotifyPropertyChanged _PropertyMother;
        private readonly Func<PropertyChangedEventHandler> _PropertyEventProvider;

        public PartialBinding(
            Func<PropertyChangedEventHandler> propertyEventProvider, 
            INotifyPropertyChanged propertyMother, 
            Expression<Func<object>> propertyToNotify)
        {
            _PropertyToNotify = propertyToNotify;
            _PropertyMother = propertyMother;
            _PropertyEventProvider = propertyEventProvider;
        }

        /// <summary>
        /// Binds a given property to one already set on the object. This is 
        /// the object that will get updated.
        /// </summary>
        /// <param name="propertyToWatch"></param>
        /// <returns></returns>
        public Subscription From(Expression<Func<object>> propertyToWatch)
        {
            var ret = new Subscription();
            _PropertyMother.SubscribeToChange(
                propertyToWatch,
                (s, e) =>
                {
                    _PropertyEventProvider().Notify(_PropertyToNotify);

                    var lambda = _PropertyToNotify as LambdaExpression;
                    var member = (MemberExpression)lambda.Body;
                    var constant = (ConstantExpression)member.Expression;

                    ret.FireChangeHandlers(constant.Value, new PropertyChangedEventArgs(member.Member.Name));
                });
            return ret;
        }
    }
}
