using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Observable.Repository.Builders;
using Observable.Repository.Core;
using Observable.Repository.Producers;

namespace Observable.Repository
{
    /// <summary>
    /// This class provides extensions methods to help the use of interfaces like <see cref="IRepository{TKey, TValue}"/>  and <see cref="IRepositoryContainer"/>.
    /// </summary>
    public static class RepositoryExtensions
    {
        #region IRepositoryContainer Extensions

        #region Build

        /// <summary>
        /// Build a new <see cref="IRepository{TKey, TValue}"/> instance.
        /// Follow the builder methods to build correctly the repository.
        /// 
        /// The full build need Build().[Join*()].[DefineCtor()].[AddBehavior()].[Create()|Register()]
        /// </summary>
        /// <typeparam name="TKey">Type of <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <typeparam name="TLeft">Type of <see cref="IRepository{TKey, TValue}"/> source.</typeparam>
        /// <param name="container">Container</param>
        /// <param name="getKey"><see cref="IRepository{TKey, TValue}"/> key getter.</param>
        /// <param name="onUpdate">Define the update method to avoid create a new instance for each updates. Null by default (Null = disabled)</param>
        /// <param name="leftSourceName">Source name for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="filter">Filter on the source for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="disposeWhenValueIsRemoved">Define if value instances should be disposed when there is removed.</param>
        /// <param name="dispatchNotifications">Dispatcher for all repository notifications.</param>
        /// <returns>The <see cref="IRepository{TKey, TValue}"/> builder.</returns>
        public static IRepositoryJoinBuilder<TKey, TValue, TLeft> Build<TKey, TValue, TLeft>(
            this IRepositoryContainer container, 
            Func<TLeft, TKey> getKey,
            Action<TValue, TLeft> onUpdate = null,
            string leftSourceName = null, 
            Func<TLeft, bool> filter = null, 
            bool disposeWhenValueIsRemoved = false, 
            Action<Action> dispatchNotifications = null) 
            => container.Build(null, getKey, onUpdate, leftSourceName, filter, disposeWhenValueIsRemoved, dispatchNotifications);

        /// <summary>
        /// Build a new <see cref="IRepository{TKey, TValue}"/> instance.
        /// Follow the builder methods to build correctly the repository.
        /// 
        /// The full build need Build().[Join*()].[DefineCtor()].[AddBehavior()].[Create()|Register()]
        /// </summary>
        /// <typeparam name="TKey">Type of <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <param name="container">Container</param>
        /// <param name="name"><see cref="IRepository{TKey, TValue}"/> name.</param>
        /// <param name="getKey"><see cref="IRepository{TKey, TValue}"/> key getter.</param>
        /// <param name="onUpdate">Define the update method to avoid create a new instance for each updates. Null by default (Null = disabled)</param>
        /// <param name="leftSourceName">Source name for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="filter">Filter on the source for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="disposeWhenValueIsRemoved">Define if value instances should be disposed when there is removed.</param>
        /// <param name="dispatchNotifications">Dispatcher for all repository notifications.</param>
        /// <returns>The <see cref="IRepository{TKey, TValue}"/> builder.</returns>
        public static IRepositoryJoinBuilder<TKey, TValue, TValue> Build<TKey, TValue>(
            this IRepositoryContainer container,
            string name,
            Func<TValue, TKey> getKey,
            Action<TValue, TValue> onUpdate = null,
            string leftSourceName = null,
            Func<TValue, bool> filter = null,
            bool disposeWhenValueIsRemoved = false,
            Action<Action> dispatchNotifications = null) 
            => container.Build(name, getKey, onUpdate, leftSourceName, filter, disposeWhenValueIsRemoved, dispatchNotifications);

        /// <summary>
        /// Build a new <see cref="IRepository{TKey, TValue}"/> instance.
        /// Follow the builder methods to build correctly the repository.
        /// 
        /// The full build need Build().[Join*()].[DefineCtor()].[AddBehavior()].[Create()|Register()]
        /// </summary>
        /// <typeparam name="TKey">Type of <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <param name="container">Container</param>
        /// <param name="getKey"><see cref="IRepository{TKey, TValue}"/> key getter.</param>
        /// <param name="onUpdate">Define the update method to avoid create a new instance for each updates. Null by default (Null = disabled)</param>
        /// <param name="leftSourceName">Source name for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="filter">Filter on the source for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="disposeWhenValueIsRemoved">Define if value instances should be disposed when there is removed.</param>
        /// <param name="dispatchNotifications">Dispatcher for all repository notifications.</param>
        /// <returns>The <see cref="IRepository{TKey, TValue}"/> builder.</returns>
        public static IRepositoryJoinBuilder<TKey, TValue, TValue> Build<TKey, TValue>(
            this IRepositoryContainer container,
            Func<TValue, TKey> getKey,
            Action<TValue, TValue> onUpdate = null,
            string leftSourceName = null,
            Func<TValue, bool> filter = null,
            bool disposeWhenValueIsRemoved = false,
            Action<Action> dispatchNotifications = null) 
            => container.Build(null, getKey, onUpdate, leftSourceName, filter, disposeWhenValueIsRemoved, dispatchNotifications);

        #endregion

        #region Register

        public static IRepositoryContainer Register<TKey, TValue, TLeft>(
            this IRepositoryContainer container,
            string name,
            Func<TLeft, TKey> getKey,
            Action<TValue, TLeft> onUpdate = null,
            string leftSourceName = null,
            Func<TLeft, bool> filter = null,
            bool disposeWhenValueIsRemoved = false,
            Action<Action> dispatchNotifications = null) 
            => container.Build(name, getKey, onUpdate, leftSourceName, filter, disposeWhenValueIsRemoved, dispatchNotifications)
                .Register();

        public static IRepositoryContainer Register<TKey, TValue, TLeft>(
            this IRepositoryContainer container,
            Func<TLeft, TKey> getKey,
            Action<TValue, TLeft> onUpdate = null,
            string leftSourceName = null,
            Func<TLeft, bool> filter = null,
            bool disposeWhenValueIsRemoved = false,
            Action<Action> dispatchNotifications = null) 
            => container.Build(null, getKey, onUpdate, leftSourceName, filter, disposeWhenValueIsRemoved, dispatchNotifications)
                .Register();

        public static IRepositoryContainer Register<TKey, TValue>(
            this IRepositoryContainer container, 
            string name, 
            Func<TValue, TKey> getKey,
            Action<TValue, TValue> onUpdate = null,
            string leftSourceName = null,
            Func<TValue, bool> filter = null,
            bool disposeWhenValueIsRemoved = false,
            Action<Action> dispatchNotifications = null) 
            => container.Build(name, getKey, onUpdate, leftSourceName, filter, disposeWhenValueIsRemoved, dispatchNotifications)
                .Register();

        public static IRepositoryContainer Register<TKey, TValue>(
            this IRepositoryContainer container,
            Func<TValue, TKey> getKey,
            Action<TValue, TValue> onUpdate = null,
            string leftSourceName = null,
            Func<TValue, bool> filter = null,
            bool disposeWhenValueIsRemoved = false,
            Action<Action> dispatchNotifications = null) 
            => container.Build(null, getKey, onUpdate, leftSourceName, filter, disposeWhenValueIsRemoved, dispatchNotifications)
                .Register();

        #endregion

        #region Manage Producer

        /// <summary>
        /// Gets a producer multicast subject instance specified by its type and its name.
        /// There is only one producer instance by couple of data type and name in a container.
        /// All producers which are added or removed with double key will be done inside this same instance.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="container"><see cref="IRepositoryContainer"/></param>
        /// <param name="name">Name of producer.</param>
        /// <returns>Gets <see cref="Producer{T}"/> instance. Returns null if not any producer has been found</returns>
        public static Producer<T> GetProducer<T>(this IRepositoryContainer container, string name = null) 
            => container?.DataProducer.GetProducer<T>(name);

        /// <summary>
        /// Add a producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="container"><see cref="IRepositoryContainer"/></param>
        /// <param name="action">Action producer.</param>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        public static IRepositoryContainer AddProducer<T>(this IRepositoryContainer container, ActionType action, IObservable<T> producer, string name = null)
        {
            container?.DataProducer.AddProducer(action, producer, name);
            return container;
        }

        /// <summary>
        /// Add a producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published inside a list.</typeparam>
        /// <param name="container"><see cref="IRepositoryContainer"/></param>
        /// <param name="action">Action from producer.</param>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        public static IRepositoryContainer AddProducer<T>(this IRepositoryContainer container, ActionType action, IObservable<List<T>> producer, string name = null)
        {
            container?.DataProducer.AddProducer(action, producer, name);
            return container;
        }

        /// <summary>
        /// Add a producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="container"><see cref="IRepositoryContainer"/></param>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        public static IRepositoryContainer AddProducer<T>(this IRepositoryContainer container, IObservable<RepositoryNotification<T>> producer, string name = null)
        {
            container?.DataProducer.AddProducer(producer, name);
            return container;
        }

        /// <summary>
        /// Remove producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="container"><see cref="IRepositoryContainer"/></param>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        public static IRepositoryContainer RemoveProducer<T>(this IRepositoryContainer container, IObservable<T> producer, string name = null)
        {
            container?.DataProducer.RemoveProducer(producer, name);
            return container;
        }

        /// <summary>
        /// Remove producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published inside a list.</typeparam>
        /// <param name="container"><see cref="IRepositoryContainer"/></param>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        public static IRepositoryContainer RemoveProducer<T>(this IRepositoryContainer container, IObservable<List<T>> producer, string name = null)
        {
            container?.DataProducer.RemoveProducer(producer, name);
            return container;
        }

        /// <summary>
        /// Remove producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="container"><see cref="IRepositoryContainer"/></param>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        public static IRepositoryContainer RemoveProducer<T>(this IRepositoryContainer container, IObservable<RepositoryNotification<T>> producer, string name = null)
        {
            container?.DataProducer.RemoveProducer(producer, name);
            return container;
        }

        #endregion 

        #endregion // IRepositoryContainer Extensions

        #region IRepository Extensions

        #region IObservable<RepositoryNotification<KeyValue<TKey, TValue>>>

        /// <summary>
        /// Subscribe for values only on the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of keys from <see cref="IRepository{TKey, TValue}"/>.</typeparam>
        /// <typeparam name="TValue">Type of values from <see cref="IRepository{TKey, TValue}"/>.</typeparam>
        /// <param name="observable">Observable repository.</param>
        /// <returns>Returns an selector observable.</returns>
        public static IObservable<RepositoryNotification<TValue>> SelectValues<TKey, TValue>(this IObservable<RepositoryNotification<KeyValue<TKey, TValue>>> observable) 
            => observable.Select(e =>
                new RepositoryNotification<TValue>(
                    e.Action,
                    e.OldItems.Select(p => p.Value),
                    e.NewItems.Select(p => p.Value)));

        /// <summary>
        /// Subscribe for keys only on the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of keys from <see cref="IRepository{TKey, TValue}"/>.</typeparam>
        /// <typeparam name="TValue">Type of values from <see cref="IRepository{TKey, TValue}"/>.</typeparam>
        /// <param name="observable">Observable repository.</param>
        /// <returns>Returns an selector observable.</returns>
        public static IObservable<RepositoryNotification<TKey>> SelectKeys<TKey, TValue>(this IObservable<RepositoryNotification<KeyValue<TKey, TValue>>> observable) 
            => observable.Select(e =>
                new RepositoryNotification<TKey>(
                    e.Action,
                    e.OldItems.Select(p => p.Key),
                    e.NewItems.Select(p => p.Key)));

        #endregion

        /// <summary>
        /// Subscribe a <see cref="IList{TSelect}"/> on the repository.
        /// </summary>
        /// <typeparam name="TKey">Type of the repository keys.</typeparam>
        /// <typeparam name="TValue">Type of the items list.</typeparam>
        /// <param name="repository"></param>
        /// <param name="view">Instance of the <see cref="IList{TSelect}"/>.</param>
        /// <param name="filter">Filter values from <see cref="IRepository{TKey, TValue}"/>.</param>
        /// <param name="synchronize">Define if the <see cref="IList{TSelect}"/> have to be synchronized with the <see cref="IRepository{TKey, TValue}"/> during the subscription.</param>
        /// <param name="viewDispatcher">Define the dispatcher where the <see cref="IList{TSelect}"/> will be managed.</param>
        /// <returns>Returns the <see cref="IListView{TSelect}"/> instance. Dispose it to release the <see cref="IList{TSelect}"/> instance.</returns>
        public static IListView<TValue> Subscribe<TKey, TValue>(this IRepository<TKey, TValue> repository, IList<TValue> view, Predicate<TValue> filter = null, bool synchronize = true, Action<Action> viewDispatcher = null) 
            => repository.Subscribe(view, p => p, filter, synchronize, viewDispatcher);

        #endregion

        #region Misc

        /// <summary>
        /// Test if type2 inherit from  or is type1
        /// </summary>
        public static bool IsBaseType(this Type type1, Type type2)
        {
            if (type1 == null || type2 == null) return false;
            if (type2.IsInterface)
                return type1.GetInterfaces().Any(p => p == type2);

            var reference = type1;
            while (reference != null)
            {
                if (reference == type2) return true;
                reference = reference.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Create the ctor method for <see cref="IRepository{TKey, TValue}"/> items.
        /// </summary>
        /// <param name="ctorArgsTypes">List of arguments type.</param>
        /// <typeparam name="TLeft">Type of <see cref="IRepository{TKey, TValue}"/> source.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <returns>The ctor delegate</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static Func<TLeft, object[], TValue> CreateCtor<TLeft, TValue>(this List<Type> ctorArgsTypes)
        {
            var count = ctorArgsTypes.Count;
            var array1 = new Type[count];
            var array2 = new Type[count + 1];
            for (var i = 0; i < count; i++)
            {
                array1[i] = ctorArgsTypes[i];
                array2[i] = ctorArgsTypes[i];
            }
            array2[count] = typeof (TValue);

            var ctor = typeof (TValue).GetConstructor(array1);

            if(ctor == null)
                throw new NotImplementedException($"A ctor have to be implemented for {typeof(TValue).Name}, with this arguments {string.Join(",", array1.Select(p => p.Name))}");

            switch (count)
            {
                case 1:
                    return CreateCtor<TLeft, TValue>(createCtor1, ctor, array2);
                case 2:
                    return CreateCtor<TLeft, TValue>(createCtor2, ctor, array2);
                case 3:
                    return CreateCtor<TLeft, TValue>(createCtor3, ctor, array2);
                case 4:
                    return CreateCtor<TLeft, TValue>(createCtor4, ctor, array2);
                case 5:
                    return CreateCtor<TLeft, TValue>(createCtor5, ctor, array2);
            }

            throw new NotSupportedException("Too many arguments");
        }

        private static Func<TLeft, object[], TValue> CreateCtor<TLeft, TValue>(MethodInfo method, ConstructorInfo ctor, Type[] array)
        {
            return (Func<TLeft, object[], TValue>)method
                .MakeGenericMethod(array)
                .Invoke(null, new object[] { ctor });
        }

        private static readonly MethodInfo createCtor1 = typeof (RepositoryExtensions)
            .GetMethod("CreateCtor1",BindingFlags.NonPublic | BindingFlags.Static);
// ReSharper disable UnusedMember.Local
        private static Func<TLeft, object[], TValue> CreateCtor1<TLeft, TValue>(ConstructorInfo ctorInfo)
// ReSharper restore UnusedMember.Local
        {
            var ctor = (Func<TLeft, TValue>)ctorInfo.CreateDelegate(typeof (Func<TLeft, TValue>));
            return (left, rights) => ctor(left);
        }

        private static readonly MethodInfo createCtor2 = typeof(RepositoryExtensions)
            .GetMethod("CreateCtor2", BindingFlags.NonPublic | BindingFlags.Static);
// ReSharper disable UnusedMember.Local
        private static Func<TLeft, object[], TValue> CreateCtor2<TLeft, TRight1, TValue>(ConstructorInfo ctorInfo)
// ReSharper restore UnusedMember.Local
        {
            var ctor = (Func<TLeft, TRight1, TValue>)ctorInfo.CreateDelegate(typeof (Func<TLeft, TRight1, TValue>));
            return (left, rights) => ctor(left, (TRight1)rights[0]);
        }

        private static readonly MethodInfo createCtor3 = typeof(RepositoryExtensions)
            .GetMethod("CreateCtor3", BindingFlags.NonPublic | BindingFlags.Static);
// ReSharper disable UnusedMember.Local
        private static Func<TLeft, object[], TValue> CreateCtor3<TLeft, TRight1, TRight2, TValue>(ConstructorInfo ctorInfo)
// ReSharper restore UnusedMember.Local
        {
            var ctor = (Func<TLeft, TRight1, TRight2, TValue>)ctorInfo.CreateDelegate(typeof(Func<TLeft, TRight1, TRight2, TValue>));
            return (left, rights) => ctor(left, (TRight1)rights[0], (TRight2)rights[1]);
        }

        private static readonly MethodInfo createCtor4 = typeof(RepositoryExtensions)
            .GetMethod("CreateCtor4", BindingFlags.NonPublic | BindingFlags.Static);
// ReSharper disable UnusedMember.Local
        private static Func<TLeft, object[], TValue> CreateCtor4<TLeft, TRight1, TRight2, TRight3, TValue>(ConstructorInfo ctorInfo)
// ReSharper restore UnusedMember.Local
        {
            var ctor = (Func<TLeft, TRight1, TRight2, TRight3, TValue>)ctorInfo.CreateDelegate(typeof(Func<TLeft, TRight1, TRight2, TRight3, TValue>));
            return (left, rights) => ctor(left, (TRight1)rights[0], (TRight2)rights[1], (TRight3)rights[2]);
        }

        private static readonly MethodInfo createCtor5 = typeof(RepositoryExtensions)
            .GetMethod("CreateCtor5", BindingFlags.NonPublic | BindingFlags.Static);
// ReSharper disable UnusedMember.Local
        private static Func<TLeft, object[], TValue> CreateCtor5<TLeft, TRight1, TRight2, TRight3, TRight4, TValue>(ConstructorInfo ctorInfo)
// ReSharper restore UnusedMember.Local
        {
            var ctor = (Func<TLeft, TRight1, TRight2, TRight3, TRight4, TValue>)ctorInfo.CreateDelegate(typeof(Func<TLeft, TRight1, TRight2, TRight3, TRight4, TValue>));
            return (left, rights) => ctor(left, (TRight1)rights[0], (TRight2)rights[1], (TRight3)rights[2], (TRight4)rights[3]);
        }

// ReSharper disable UnusedMember.Local
        private static Func<TLeft, object[], TValue> CreateCtor6<TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TValue>(ConstructorInfo ctorInfo)
// ReSharper restore UnusedMember.Local
        {
            var ctor = (Func<TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TValue>)ctorInfo.CreateDelegate(typeof(Func<TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TValue>));
            return (left, rights) => ctor(left, (TRight1)rights[0], (TRight2)rights[1], (TRight3)rights[2], (TRight4)rights[3], (TRight5)rights[4]);
        }

        /// <summary>
        /// Create a ctor delegate from a <see cref="ConstructorInfo"/>.
        /// </summary>
        /// <param name="constructor">Constructor info</param>
        /// <param name="delegateType">Type of the delegate</param>
        /// <returns>Returns the delegate</returns>
        public static Delegate CreateDelegate(this ConstructorInfo constructor, Type delegateType)
        {
            if (constructor == null || delegateType == null) return null;
            
            var constructorParam = constructor.GetParameters();

            // Create the dynamic method
            var method = new DynamicMethod(
                $"GeneratedCtor_{Guid.NewGuid().ToString().Replace("-", "")}",
                constructor.DeclaringType,
                Array.ConvertAll(constructorParam, p => p.ParameterType), true);

            // Create the il
            var gen = method.GetILGenerator();
            for (var i = 0; i < constructorParam.Length; i++)
            {
                if (i < 4)
                {
                    switch (i)
                    {
                        case 0:
                            gen.Emit(OpCodes.Ldarg_0);
                            break;
                        case 1:
                            gen.Emit(OpCodes.Ldarg_1);
                            break;
                        case 2:
                            gen.Emit(OpCodes.Ldarg_2);
                            break;
                        case 3:
                            gen.Emit(OpCodes.Ldarg_3);
                            break;
                    }
                }
                else
                {
                    gen.Emit(OpCodes.Ldarg_S, i);
                }
            }
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);

            // Return the delegate :)
            return method.CreateDelegate(delegateType);
        }

        #endregion
    }
}
