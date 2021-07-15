using System;

namespace GObject
{
    /// <summary>
    /// Describes a GProperty.
    /// </summary>
    /// <typeparam name="T">The type of the value this property will store.</typeparam>
    public sealed class Property<T>
    {
        #region Fields

        /// <summary>
        /// The property getter.
        /// </summary>
        private Func<IObject, T>? _get;

        /// <summary>
        /// The property setter.
        /// </summary>
        private Action<IObject, T>? _set;

        #endregion

        #region Properties

        /// <summary>
        /// The name of the GProperty.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The name of the C# property which serves as the
        /// proxy of this GProperty.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Checks if this GProperty is readable.
        /// </summary>
        public bool IsReadable => _get != null;

        /// <summary>
        /// Checks if this GProperty is writeable.
        /// </summary>
        public bool IsWriteable => _set != null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Property{T}"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is made private, since the end user need
        /// to register his properties using <see cref="Register{TObject}"/>.
        /// </remarks>
        /// <param name="name">The name of this GProperty</param>
        /// <param name="propertyName">The name of the C# property which serves as the proxy of this GProperty.</param>
        private Property(string name, string propertyName)
        {
            Name = name;
            PropertyName = propertyName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers this property descriptor and creates the correct GProperty
        /// into the GLib type of <typeparamref name="TObject"/>.
        /// </summary>
        /// <param name="nativeName">The name of the GProperty to create.</param>
        /// <param name="managedName">The name of the C# property which serves as the proxy of this GProperty</param>
        /// <param name="get">The function called when retrieving the value of this property in bindings.</param>
        /// <param name="set">The function called when defining the value of this property in bindings.</param>
        /// <typeparam name="TObject">The type of the object on which this property will be registered.</typeparam>
        /// <returns>
        /// An instance of <see cref="Property{T}"/> representing the GProperty description.
        /// </returns>
        public static Property<T> Register<TObject>(string nativeName, string managedName, Func<TObject, T>? get = null, Action<TObject, T>? set = null)
            where TObject : Object
        {
            return new Property<T>(nativeName, managedName)
            {
                _get = get is null ? null : new Func<IObject, T>((o) => get((TObject) o)),
                _set = set is null ? null : new Action<IObject, T>((o, v) => set((TObject) o, v)),
            };
        }

        /// <summary>
        /// Wrap this property descriptor on an existing GProperty
        /// from a GLib type of <typeparamref name="TObject"/>.
        /// </summary>
        /// <param name="nativeName">The name of the GProperty to wrap.</param>
        /// <param name="managedName">The name of the C# property which serves as the proxy of this GProperty</param>
        /// <param name="get">The function called when retrieving the value of this property in bindings.</param>
        /// <param name="set">The function called when defining the value of this property in bindings.</param>
        /// <typeparam name="TObject">The type of the object on which this property will be wrapped.</typeparam>
        /// <returns>
        /// An instance of <see cref="Property{T}"/> representing the GProperty description.
        /// </returns>
        public static Property<T> Wrap<TObject>(string nativeName, string managedName, Func<TObject, T>? get = null, Action<TObject, T>? set = null)
            where TObject : IObject
        {
            return new Property<T>(nativeName, managedName)
            {
                _get = get is null ? null : new Func<IObject, T>((o) => get((TObject) o)),
                _set = set is null ? null : new Action<IObject, T>((o, v) => set((TObject) o, v)),
            };
        }

        /// <summary>
        /// Get the value of this property in the given object.
        /// </summary>
        public T Get(IObject o) => _get is null
            ? throw new InvalidOperationException("Trying to read the value of a write-only property.")
            : _get(o);

        /// <summary>
        /// Set the value of this property in the given object
        /// using the given value.
        /// </summary>
        public void Set(IObject o, T v)
        {
            if (_set is null)
                throw new InvalidOperationException("Trying to write the value of a read-only property.");

            _set(o, v);
        }

        /// <summary>
        /// Registers this GProperty for notify events into the given object.
        /// </summary>
        /// <param name="o">The object in which register the property.</param>
        internal void RegisterNotifyEvent(Object o)
        {
            Object.ClosureRegistry closureRegistry = o.GetClosureRegistry($"notify::{Name}");
            closureRegistry.Connect<Action<string>>(
                action: o.OnPropertyChanged,
                after: false,
                mapping: arg => ((ref Value[] _) => arg(PropertyName))
            );
        }

        #endregion
    }
}
