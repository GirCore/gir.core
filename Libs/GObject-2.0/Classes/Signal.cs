using System;

namespace GObject
{
    /// <summary>
    /// SignalHandler for signals without any extra data.
    /// </summary>
    /// <param name="sender">The sender of this signal.</param>
    /// <param name="args">Event args. Will always have the value of <see cref="EventArgs.Empty"/>.</param>
    public delegate void SignalHandler<in TSender>(TSender sender, EventArgs args)
        where TSender : Object;

    /// <summary>
    /// SignalHandler for signals with extra data.
    /// </summary>
    /// <param name="sender">The sender of this signal.</param>
    /// <param name="args"><see cref="SignalArgs"/> with additional data.</param>
    public delegate void SignalHandler<in TSender, in TSignalArgs>(TSender sender, TSignalArgs args)
        where TSender : Object
        where TSignalArgs : SignalArgs;

    /// <summary>
    /// Base class for signal based events.
    /// </summary>
    public class SignalArgs : EventArgs
    {
        #region Properties

        protected Value[] Args { get; set; }

        #endregion

        #region Constructors

        public SignalArgs()
        {
            Args = Array.Empty<Value>();
        }

        #endregion

        #region Methods

        internal void SetArgs(Value[] args)
        {
            Args = args;
        }

        #endregion
    }

    public class Signal
    {
        #region Properties

        /// <summary>
        /// The name of the GSignal.
        /// </summary>
        public string Name { get; }

        #endregion

        #region Constructors

        internal Signal(string name)
        {
            Name = name;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disconnects an <paramref name="action"/> previously connected to this signal.
        /// </summary>
        /// <param name="o">The object from which disconnect the handler.</param>
        /// <param name="action">The signal handler function.</param>
        public void Disconnect(Object o, object action)
        {
            Object.ClosureRegistry closureRegistry = o.GetClosureRegistry(Name);
            closureRegistry.Disconnect(action);
        }

        #endregion
    }

    /// <summary>
    /// Describes a GSignal.
    /// </summary>
    public class Signal<TSender, TSignalArgs> : Signal
        where TSender : Object
        where TSignalArgs : SignalArgs, new()
    {
        #region Constructors

        private Signal(string name) : base(name)
        { }

        #endregion

        #region Methods

        /// <summary>
        /// Wraps an existing GSignal.
        /// </summary>
        /// <param name="name">The name of the GSignal to wrap.</param>
        /// <returns>
        /// An instance of <see cref="Signal"/> which describes the signal to wrap.
        /// </returns>
        public static Signal<TSender, TSignalArgs> Wrap(string name)
        {
            return new Signal<TSender, TSignalArgs>(name);
        }

        /// <summary>
        /// Connects an <paramref name="action"/> to this signal.
        /// </summary>
        /// <param name="o">The object on which connect the handler.</param>
        /// <param name="action">The signal handler function.</param>
        /// <param name="after">
        /// Define if this action must be called before or after the default handler of this signal.
        /// </param>
        public void Connect(TSender o, SignalHandler<TSender, TSignalArgs> action, bool after = false)
        {
            Object.ClosureRegistry closureRegistry = o.GetClosureRegistry(Name);
            closureRegistry.Connect(
                action: action,
                after: after,
                mapping: callback => (ref Value[] items) =>
                {
                    var args = new TSignalArgs();
                    args.SetArgs(items);
                    callback(o, args);
                }
            );
        }

        #endregion

    }

    /// <summary>
    /// Describes a GSignal.
    /// </summary>
    public class Signal<TSender> : Signal
        where TSender : Object
    {
        #region Constructors

        private Signal(string name) : base(name)
        { }

        #endregion

        #region Methods

        /// <summary>
        /// Wraps an existing GSignal.
        /// </summary>
        /// <param name="name">The name of the GSignal to wrap.</param>
        /// <returns>
        /// An instance of <see cref="Signal"/> which describes the signal to wrap.
        /// </returns>
        public static Signal<TSender> Wrap(string name)
        {
            return new Signal<TSender>(name);
        }

        /// <summary>
        /// Connects an <paramref name="action"/> to this signal.
        /// </summary>
        /// <param name="o">The object on which connect the handler.</param>
        /// <param name="action">The signal handler function.</param>
        /// <param name="after">
        /// Define if this action must be called before or after the default handler of this signal.
        /// </param>
        public void Connect(TSender o, SignalHandler<TSender> action, bool after = false)
        {
            //TODO: Use SignalMapper to avoid lossing the signalhelper instance? Remove
            //"GetSignalHelper method from Object class
            Object.ClosureRegistry closureRegistry = o.GetClosureRegistry(Name);
            closureRegistry.Connect(
                action: action,
                after: after,
                mapping: callback => (ref Value[] _) => callback(o, EventArgs.Empty)
            );
        }

        #endregion
    }
}
