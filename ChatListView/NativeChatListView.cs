using System;
using System.Windows.Input;
using Xamarin.Forms;


namespace CC
{
	public class NativeChatListView : ListView
	{
		
		public EventHandler<SendMessageEventArgs>   SendMessage;
		public EventHandler<ImageSelectedEventArgs> ImageSelected;

	} // NativeChatListView

	public class NativeChatListViewSendBehavior : Behavior<NativeChatListView>
	{
		
		public static readonly BindableProperty CommandProperty =
			BindableProperty.Create("Command", typeof(ICommand), typeof(NativeChatListViewSendBehavior), null);
		
		public static readonly BindableProperty InputConverterProperty =
				BindableProperty.Create("Converter", typeof(IValueConverter), typeof(NativeChatListViewSendBehavior), null);

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public IValueConverter Converter
		{
			get { return (IValueConverter)GetValue(InputConverterProperty); }
			set { SetValue(InputConverterProperty, value); }
		}

		public ListView AssociatedObject { get; private set; }

		protected override void OnAttachedTo(NativeChatListView bindable)
		{
			base.OnAttachedTo(bindable);
			AssociatedObject = bindable;
			if (bindable != null)
			{
				((ListView)bindable).BindingContextChanged += OnBindingContextChanged;
			}
			bindable.SendMessage += OnSendMessage;
		}

		protected override void OnDetachingFrom(NativeChatListView bindable)
		{
			base.OnDetachingFrom(bindable);
			bindable.BindingContextChanged -= OnBindingContextChanged;
			bindable.SendMessage -= OnSendMessage;
			AssociatedObject = null;
		}

		void OnBindingContextChanged(object sender, EventArgs e)
		{
			OnBindingContextChanged();
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			BindingContext = AssociatedObject.BindingContext;
		}

		void OnSendMessage(object sender, SendMessageEventArgs e)
		{
			if (Command == null)
			{
				return;
			}

			//object parameter = Converter.Convert(e, typeof(object), null, null);
			if (Command.CanExecute(e.Message))
			{
				Command.Execute(e.Message);
			}
		}

	} // NativeChatListViewSendBehavior


	public class SendMessageEventArgs : EventArgs
	{
		public SendMessageEventArgs(string s)
		{
			_message = s;
		}

		private string _message;

		public string Message
		{
			get { return _message; }
			set { _message = value; }
		}

	} // SendMessageEventArgs

	public class ImageSelectedEventArgs : EventArgs
	{
		public ImageSelectedEventArgs()
		{
		}
		
	}
}
