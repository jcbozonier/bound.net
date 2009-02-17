using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Bound.Net
{
    /// <summary>
    /// Notifies subscribers of the event handler that the provided property was modified.
    /// </summary>
    public static class NotificationExtensions
    {
        /// <summary>
        /// Fires the set of events in the handler and passes the name of the changed
        /// property along as the changed property.
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="changedProperty"></param>
        public static void Notify(
            this PropertyChangedEventHandler eventHandler, 
            Expression<Func<object>> changedProperty)
        {
            if (null == eventHandler) return;

            ConstantExpression constantExpression;
            var memberExpression = _GetMemberExpression(changedProperty, out constantExpression);
            var propertyInfo = (PropertyInfo)memberExpression.Member;

            foreach (var del in eventHandler.GetInvocationList())
            {
                del.DynamicInvoke(new[]
                                      {
                                          constantExpression.Value,
                                          new PropertyChangedEventArgs(propertyInfo.Name)
                                      });
            }
        }

        /// <summary>
        /// Subscribes to a change on the objectThatNotifies on the given property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectThatNotifies"></param>
        /// <param name="propertyThatCouldChange"></param>
        /// <param name="handler"></param>
        public static void SubscribeToChange<T>(
            this T objectThatNotifies, 
            Expression<Func<object>> propertyThatCouldChange, 
            PropertyChangedEventHandler handler)
            where T : INotifyPropertyChanged
        {
            objectThatNotifies.PropertyChanged +=
                (s, e) =>
                {
                    var lambda = propertyThatCouldChange as LambdaExpression;
                    var memberExpression = _ConvertToMemberExpression(lambda);
                    var propertyInfo = memberExpression.Member as PropertyInfo;
                    if(propertyInfo == null)
                        throw new NullReferenceException("Somehow the Subscribe to change method got a null propertyInfo.");

                    if (e.PropertyName.Equals(propertyInfo.Name))
                        handler(objectThatNotifies, e);
                };
        }

        private static MemberExpression _ConvertToMemberExpression(LambdaExpression lambda)
        {
            return (MemberExpression)
                   ((lambda.Body is UnaryExpression)
                        ? ((UnaryExpression) lambda.Body).Operand
                        : (MemberExpression) lambda.Body);
        }


        private static MemberExpression _GetMemberExpression(
            Expression<Func<object>> expression,
            out ConstantExpression constantExpression)
        {
            var lambda = expression as LambdaExpression;
            var memberExpression = _ConvertToMemberExpression(lambda);
            constantExpression = memberExpression.Expression as ConstantExpression;

            return memberExpression;
        }
    }
}
