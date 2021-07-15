using System;
using System.Threading.Tasks;
using GLib;

namespace Gio
{
    public partial class DBusConnection
    {
        #region Fields

        private AsyncReadyCallbackAsyncHandler? _callAsyncCallbackHandler;

        #endregion

        #region Static methods

        public static DBusConnection Get(BusType busType)
        {
            var error = new GLib.Native.Error.Handle(IntPtr.Zero);
            var handle = Native.Functions.BusGetSync(busType, IntPtr.Zero, error);
            Error.ThrowOnError(error);

            return GObject.Native.ObjectWrapper.WrapHandle<DBusConnection>(handle, true);
        }

        #endregion

        #region Methods

        public Task<Variant> CallAsync(string busName, string objectPath, string interfaceName, string methodName,
            Variant? parameters = null)
        {
            var tcs = new TaskCompletionSource<Variant>();

            void Callback(GObject.Object sourceObject, AsyncResult res)
            {
                // TODO: Make sure this is correct (can we assume res is a GObject?)
                var error = new GLib.Native.Error.Handle(IntPtr.Zero);
                var ret = Native.DBusConnection.Instance.Methods.CallFinish(sourceObject.Handle, (res as GObject.Object).Handle, error);
                Error.ThrowOnError(error);

                tcs.SetResult(Variant.__FactoryNew(ret));
            }

            //TODO: Use on time CallbackHandler
            _callAsyncCallbackHandler = new AsyncReadyCallbackAsyncHandler(Callback);

            Native.DBusConnection.Instance.Methods.Call(Handle, busName, objectPath, interfaceName, methodName, parameters.GetSafeHandle(), GLib.Native.VariantType.Handle.Null, DBusCallFlags.None, -1, IntPtr.Zero, _callAsyncCallbackHandler.NativeCallback, IntPtr.Zero);

            return tcs.Task;
        }

        public Variant Call(string busName, string objectPath, string interfaceName, string methodName, Variant? parameters = null)
        {
            var parameterHandle = parameters?.Handle ?? GLib.Native.Variant.Handle.Null;
            var error = new GLib.Native.Error.Handle(IntPtr.Zero);
            var ret = Native.DBusConnection.Instance.Methods.CallSync(Handle, busName, objectPath, interfaceName, methodName, parameterHandle, GLib.Native.VariantType.Handle.Null, DBusCallFlags.None, 9999, IntPtr.Zero, error);

            Error.ThrowOnError(error);

            return Variant.__FactoryNew(ret);
        }

        public override void Dispose()
        {
            _callAsyncCallbackHandler?.Dispose();
            base.Dispose();
        }

        #endregion
    }
}
