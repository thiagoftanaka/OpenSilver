namespace System.Windows.Controls.Internals
{
    internal interface IEvent<TEventArgs>
    {
        TEventArgs EventArgs { get; }
        object Sender { get; }
    }
}
