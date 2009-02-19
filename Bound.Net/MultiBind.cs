using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Bound.Net
{
    namespace FluentInterface
    {
        public interface IDerivedProperties
        {
            IRegistration Add(Expression<Func<object>> targetProperty);
        }

        public interface IRegistration
        {
            IDerivedProperties DerivedProperties { get; }
            IRegistration AfterNotifyDo(Action<object> action, object argument);
            IRegistration AfterNotifyDo(Action action);
        }
    }

    public class PropertyBinder
    {

        private readonly INotifyPropertyChanged _Source;
        private readonly Func<PropertyChangingEventHandler> _EventHandlerProvider;

        public PropertyBinder(INotifyPropertyChanged source, Func<PropertyChangingEventHandler> eventHandlerProvider)
        {
            _Source = source;
            _EventHandlerProvider = eventHandlerProvider;
        }

        private static void _CrackMemberRef(Expression<Func<object>> reference, out string memberName, out object targetObject)
        {
            if(reference == null)
                throw new ArgumentNullException("reference");

            var member = reference.Body as MemberExpression;
            if(member == null)
                throw new ArgumentException("??? (1)", "reference");

            var constant = member.Expression as ConstantExpression;
            if(constant == null)
                throw new ArgumentException("??? (2)", "reference");

            memberName = member.Member.Name;
            targetObject = constant.Value;
        }

        private class FluentInterfaceRoot: FluentInterface.IRegistration, FluentInterface.IDerivedProperties
        {
            private readonly List<Action> _AfterNotifyActions = new List<Action>();
            private readonly List<string> _DerivedProperties = new List<string>();

            FluentInterface.IDerivedProperties FluentInterface.IRegistration.DerivedProperties
            {
                get { return this; }
            }

            FluentInterface.IRegistration FluentInterface.IRegistration.AfterNotifyDo(Action<object> action, object argument)
            {
                if(action == null)
                    throw new ArgumentNullException("action");
                _AfterNotifyActions.Add(() => action(argument));
                return this;
            }

            FluentInterface.IRegistration FluentInterface.IRegistration.AfterNotifyDo(Action action)
            {
                if(action == null)
                    throw new ArgumentNullException("action");
                _AfterNotifyActions.Add(action);
                return this;
            }

            FluentInterface.IRegistration FluentInterface.IDerivedProperties.Add(Expression<Func<object>> targetProperty)
            {
                string memberName;
                object targetObject;
                _CrackMemberRef(targetProperty, out memberName, out targetObject);
                _DerivedProperties.Add(memberName);
                return this;
            }

            public void ExecuteDerivedProperties(Action<string> derivedPropertyHandler)
            {
                foreach (var propertyName in _DerivedProperties)
                    derivedPropertyHandler(propertyName);
            }

            public void ExecuteAfterActions()
            {
                foreach (var action in _AfterNotifyActions)
                    action();
            }

        }

        public FluentInterface.IRegistration RegisterPropertyConnection(Expression<Func<object>> sourceProperty, Expression<Func<object>> targetProperty)
        {
            string sourceMemberName, targetMemberName;
            object sourceObject, targetObject;

            _CrackMemberRef(sourceProperty, out sourceMemberName, out sourceObject);
            _CrackMemberRef(targetProperty, out targetMemberName, out targetObject);

            var registration = new FluentInterfaceRoot();

            _Source.PropertyChanged += (s, e) =>
            {
                // if it's the wrong property, don't handle it
                if(e.PropertyName != sourceMemberName)
                    return;

                // if the listeners are null, don't do any notifications
                var propChanged = _EventHandlerProvider();
                if (propChanged != null)
                {

                    propChanged(targetObject, new PropertyChangingEventArgs(targetMemberName));
                    registration.ExecuteDerivedProperties(propName => propChanged(targetObject, new PropertyChangingEventArgs(propName)));
                }

                // always do the 'after' stuff, even if there's no listeners
                registration.ExecuteAfterActions();
            };

            return registration;
        }
    }
}
