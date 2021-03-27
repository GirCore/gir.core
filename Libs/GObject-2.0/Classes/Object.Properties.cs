﻿using System;
using System.Runtime.InteropServices;

namespace GObject
{
    public partial class Object
    {
        #region Methods

        /// <summary>
        /// Gets the value of the GProperty described by <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The property descriptor of the GProperty from which get the value.</param>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <returns>
        /// The value of the GProperty.
        /// </returns>
        protected T GetProperty<T>(Property<T> property)
            => GetProperty(property.Name).Extract<T>();

        /// <summary>
        /// Sets the <paramref name="value"/> of the GProperty described by <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The property descriptor of the GProperty on which set the value.</param>
        /// <param name="value">The value to set to the GProperty.</param>
        /// <typeparam name="T">The tye of the value to define.</typeparam>
        protected void SetProperty<T>(Property<T> property, T value)
        {
            if (value is Object o)
                SetProperty(property.Name, new Value(o.Handle));
            else
                SetProperty(property.Name, Value.From(value));
        }

        /// <summary>
        /// Assigns the value of a GObject's property given its <paramref name="name"/>
        /// </summary>
        /// <param name="value">The property name.</param>
        /// <param name="name">The property value.</param>
        protected void SetProperty(string name, Value value)
        {
            Native.Instance.Methods.SetProperty(Handle, name, value.Handle);
            value.Dispose();
        }

        /// <summary>
        /// Gets the value of a GObject's property given its <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>
        /// The native value of the property, wrapped as a <see cref="Value"/>.
        /// </returns>
        protected Value GetProperty(string name)
        {
            var handle = Value.Native.ManagedValueSafeHandle.Create();
            
            GetProperty(Handle, name, ref handle);

            return new Value(handle);
        }

        //TODO: Clarify this call use native one if possible
        [DllImport("GObject", EntryPoint = "g_object_get_property")]
        public static extern void GetProperty(IntPtr @object, string propertyName, ref Value.Native.ValueSafeHandle value);

        #endregion
    }
}