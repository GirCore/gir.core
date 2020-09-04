using System;
using System.Collections.Generic;

namespace GObject
{
    public class SignalArgs : EventArgs
    {
        public object[] Args { get; }

        public SignalArgs()
        {
            Args = Array.Empty<object>();
        }

        internal SignalArgs(params object[] args)
        {
            Args = args;
        }

        internal SignalArgs(params Sys.Value[] args)
        {
            Args = new object[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                // TODO: Args[i] = args[i].Value;
            }
        }
    }

    /// <summary>
    /// Describes a GSignal.
    /// </summary>
    public sealed class Signal
    {
        #region Fields

        private static readonly Dictionary<EventHandler<SignalArgs>, ActionRefValues> Handlers = new Dictionary<EventHandler<SignalArgs>, ActionRefValues>();

        #endregion

        #region Properties

        /// <summary>
        /// The name of the GSignal.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The signal flags.
        /// </summary>
        public Sys.SignalFlags Flags { get; }

        /// <summary>
        /// The return type of signal handlers.
        /// </summary>
        public Sys.Type ReturnType { get; }

        /// <summary>
        /// The type of parameters in signal handlers.
        /// </summary>
        public Sys.Type[] ParamTypes { get; }

        #endregion

        #region Constructors

        private Signal(string name, Sys.SignalFlags flags, Sys.Type returnType, Sys.Type[] paramTypes)
        {
            Name = name;
            Flags = flags;
            ReturnType = returnType;
            ParamTypes = paramTypes;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers a new GSignal into this type.
        /// </summary>
        /// <param name="name">The name of the GSignal to create.</param>
        /// <param name="flags">The GSignal flags.</param>
        /// <param name="returnType">The type of the value returned by the handlers of this GSignal.</param>
        /// <param name="paramTypes">
        /// The types list for each parameters given to handlers of this GSignal,
        /// in the order they appear.</param>
        /// <returns>
        /// An instance of <see cref="Signal"/> which describes the registered signal.
        /// </returns>
        public static Signal Register(string name, Sys.SignalFlags flags, Sys.Type returnType, params Sys.Type[] paramTypes)
        {
            return new Signal(name, flags, returnType, paramTypes);
        }

        /// <summary>
        /// Registers a new GSignal into this type.
        /// </summary>
        /// <param name="name">The name of the GSignal to create.</param>
        /// <param name="flags">The GSignal flags.</param>
        /// <returns>
        /// An instance of <see cref="Signal"/> which describes the registered signal.
        /// </returns>
        public static Signal Register(string name, Sys.SignalFlags flags = Sys.SignalFlags.run_last)
        {
            return new Signal(name, flags, Sys.Type.None, Array.Empty<Sys.Type>());
        }

        /// <summary>
        /// Wraps an existing GSignal.
        /// </summary>
        /// <param name="name">The name of the GSignal to wrap.</param>
        /// <returns>
        /// An instance of <see cref="Signal"/> which describes the signal to wrap.
        /// </returns>
        public static Signal Wrap(string name)
        {
            // Here only the signal name is relevant, other paramters are not used.
            return new Signal(name, Sys.SignalFlags.run_last, Sys.Type.None, Array.Empty<Sys.Type>());
        }

        /// <summary>
        /// Connects an <paramref name="action"/> to this signal.
        /// </summary>
        /// <param name="o">The object on which connect the handler.</param>
        /// <param name="action">The signal handler function.</param>
        /// <param name="after">
        /// Define if this action must be called before or after the default handler of this signal.
        /// </param>
        public void Connect(Object o, Action action, bool after = false)
        {
            if (action == null)
                return;

            o.RegisterEvent(Name, action, after);
        }

        /// <summary>
        /// Connects an <paramref name="action"/> to this signal.
        /// </summary>
        /// <param name="o">The object on which connect the handler.</param>
        /// <param name="action">The signal handler function.</param>
        /// <param name="after">
        /// Define if this action must be called before or after the default handler of this signal.
        /// </param>
        public void Connect(Object o, ActionRefValues action, bool after = false)
        {
            if (action == null)
                return;

            o.RegisterEvent(Name, action, after);
        }

        /// <summary>
        /// Connects an <paramref name="action"/> to this signal.
        /// </summary>
        /// <param name="o">The object on which connect the handler.</param>
        /// <param name="action">The signal handler function.</param>
        /// <param name="after">
        /// Define if this action must be called before or after the default handler of this signal.
        /// </param>
        public void Connect(Object o, EventHandler<SignalArgs> action, bool after = false)
        {
            if (action == null)
                return;

            if (!Handlers.TryGetValue(action, out ActionRefValues callback))
                callback = (ref Sys.Value[] values) => action(o, new SignalArgs(values));

            o.RegisterEvent(Name, callback, after);
            Handlers[action] = callback;
        }

        /// <summary>
        /// Disconnects an <paramref name="action"/> previously connected to this signal.
        /// </summary>
        /// <param name="o">The object from which disconnect the handler.</param>
        /// <param name="action">The signal handler function.</param>
        public void Disconnect(Object o, Action action)
        {
            if (action == null)
                return;

            o.UnregisterEvent(action);
        }

        /// <summary>
        /// Disconnects an <paramref name="action"/> previously connected to this signal.
        /// </summary>
        /// <param name="o">The object from which disconnect the handler.</param>
        /// <param name="action">The signal handler function.</param>
        public void Disconnect(Object o, ActionRefValues action)
        {
            if (action == null)
                return;

            o.UnregisterEvent(action);
        }

        /// <summary>
        /// Disconnects an <paramref name="action"/> previously connected to this signal.
        /// </summary>
        /// <param name="o">The object from which disconnect the handler.</param>
        /// <param name="action">The signal handler function.</param>
        public void Disconnect(Object o, EventHandler<SignalArgs> action)
        {
            if (action == null)
                return;

            if (!Handlers.TryGetValue(action, out ActionRefValues callback))
                return;

            o.UnregisterEvent(callback);
            Handlers.Remove(action);
        }

        #endregion
    }
}