using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using ChatListView.iOS.Controls;

namespace ChatListView.iOS
{
	internal class ChatInputAccessoryView : UIView
	{
		private const float StartingHeight = 44;

		private const float TextViewMaxHeightPortrait = 130;

		private const float TextViewMaxHeightLandscape = 90;

		internal ChatInputAccessoryView(string unsentMessage, EventHandler<SendMessageEventArgs> sendMessageHandler)
			: base(new CGRect(0, 0, nfloat.MaxValue, StartingHeight))
		{
			UnsentMessage = unsentMessage;
			SendMessageHandler = sendMessageHandler;
		}

		internal static bool IsPortrait
		{
			get
			{
				return UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.Portrait ||
					   UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.PortraitUpsideDown;
			}
		}

		internal MessageAreaView MessageArea { get; private set; }

		private Action UploadButtonAction { get; set; }

		private EventHandler<SendMessageEventArgs> SendMessageHandler { get; set; }

		private Func<string, bool, Task> SaveUnsentMessageAsync { get; set; }

		private string UnsentMessage { get; set; }

		private UIViewController PresentingViewController { get; set; }

		private NSLayoutConstraint HeightConstraint { get; set; }

		private NSObject TextDidChangeObservable { get; set; }

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (HeightConstraint == null)
			{
				// This is a height constraint on the accessory view.  
				// We need to update this along with the frame height.
				HeightConstraint = Constraints.First(c => c.GetIdentifier() == "_UIKBAutolayoutHeightConstraint");

				BackgroundColor = UIKit.UIColor.White;

				MessageArea = new MessageAreaView(UnsentMessage, UploadButtonAction, SendMessageHandler, SaveUnsentMessageAsync, () =>
				{
					AdjustViewHeight();
				});

				AddSubview(MessageArea);
				MessageArea.SetupConstraints(this);
			}
		}

		internal void WillAppear()
		{
			RegisterForNotifications();
		}

		internal void WillDisappear()
		{
			UnregisterForNotifications();

			if (MessageArea != null && MessageArea.TextInputView != null)
			{
				SaveUnsentMessageAsync(MessageArea.TextInputView.Text, true);
			}
		}

		internal void UpdateHeight()
		{
			if (MessageArea != null)
			{
				MessageArea.UpdateTextViewHeight();
			}
		}

		internal bool HideKeyboard()
		{
			if (MessageArea != null)
			{
				return MessageArea.HideKeyboard();
			}

			return false;
		}

		private void RegisterForNotifications()
		{
			TextDidChangeObservable = UITextView.Notifications.ObserveTextDidChange((sender, args) =>
			{
				if (MessageArea != null)
				{
					MessageArea.HandleTextChange(args);
				}
			});
		}

		private void UnregisterForNotifications()
		{
			if (TextDidChangeObservable != null)
			{
				TextDidChangeObservable.Dispose();
				TextDidChangeObservable = null;
			}
		}

		private void AdjustViewHeight()
		{
			var height = MessageArea.HeightConstraint.Constant;

			HeightConstraint.Constant = height;
			Frame = new CGRect(Frame.X, Frame.Y, Frame.Width, height);
		}

		internal class MessageAreaView : UIView
		{

			internal MessageAreaView(
				string unsentMessage,
				Action uploadButtonAction,
				EventHandler<SendMessageEventArgs> sendMessageHandler,
				Func<string, bool, Task> saveUnsentMessageAsync,
				Action heightChangeAction)
			{
				UnsentMessage = unsentMessage;
				UploadButtonAction = uploadButtonAction;
				SendMessageHandler = sendMessageHandler;
				SaveUnsentMessageAsync = saveUnsentMessageAsync;
				HeightChangeAction = heightChangeAction;

				TranslatesAutoresizingMaskIntoConstraints = false;
			}

			private Func<string, bool, Task> SaveUnsentMessageAsync { get; set; }

			private string UnsentMessage { get; set; }

			internal NSLayoutConstraint HeightConstraint { get; set; }

			internal UIButton UploadButton { get; private set; }

			internal ChatInputPlaceholderTextView TextInputView { get; private set; }

			internal UIButton SendButton { get; private set; }

			private UIView SeparatorLine { get; set; }

			private Action UploadButtonAction { get; set; }

			private EventHandler<SendMessageEventArgs> SendMessageHandler { get; set; }

			private Action HeightChangeAction { get; set; }

			internal void SetupConstraints(UIView parent)
			{
				parent.AddConstraint(NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, parent, NSLayoutAttribute.Leading, 1, 0));
				parent.AddConstraint(NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, parent, NSLayoutAttribute.Bottom, 1, 0));
				parent.AddConstraint(NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, parent, NSLayoutAttribute.Trailing, 1, 0));

				HeightConstraint = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, StartingHeight);
				AddConstraint(HeightConstraint);

				UploadButton = new UIButton();
				UploadButton.TranslatesAutoresizingMaskIntoConstraints = false;
				UploadButton.SetImage(UIImage.FromBundle("t_camera"), UIControlState.Normal);

				SendButton = new UIButton();
				SendButton.SetImage(UIImage.FromBundle("t_send"), UIControlState.Normal);
				SendButton.TranslatesAutoresizingMaskIntoConstraints = false;
				//SendButton.SetTitle("Send", UIControlState.Normal);
				SendButton.Enabled = false;
				//SendButton.Font = UIFont.BoldSystemFontOfSize(16);
				//SendButton.SetTitleColor(UIKit.UIColor.DarkTextColor, UIControlState.Normal);
				//SendButton.SetTitleColor(UIKit.UIColor.LightGray, UIControlState.Disabled);

				TextInputView = new ChatInputPlaceholderTextView(SendButton);
				TextInputView.TranslatesAutoresizingMaskIntoConstraints = false;
				TextInputView.Font = UIFont.SystemFontOfSize(14);
				TextInputView.ClipsToBounds = true;
				TextInputView.Layer.CornerRadius = 16;
				TextInputView.Layer.BorderColor = UIKit.UIColor.FromRGB(0, 0, 0).CGColor;
				TextInputView.Layer.BorderWidth = .25f;
				TextInputView.Placeholder = "Message";
				TextInputView.BackgroundColor = UIKit.UIColor.FromRGB(245, 245, 245);
				TextInputView.Text = UnsentMessage;

				var insets = TextInputView.TextContainerInset;
				insets.Left += 4;
				TextInputView.TextContainerInset = insets;

				SeparatorLine = new UIView();
				SeparatorLine.TranslatesAutoresizingMaskIntoConstraints = false;
				SeparatorLine.BackgroundColor = UIKit.UIColor.White;

				AddSubview(UploadButton);
				AddSubview(TextInputView);
				//AddSubview(SendButton);
				AddSubview(SeparatorLine);

				AddConstraint(NSLayoutConstraint.Create(UploadButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 9));
				AddConstraint(NSLayoutConstraint.Create(UploadButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, -9));
				AddConstraint(NSLayoutConstraint.Create(UploadButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, TextInputView, NSLayoutAttribute.Leading, 1, -8));
				//UploadButton.AddConstraint(NSLayoutConstraint.Create(UploadButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 24));
				//UploadButton.AddConstraint(NSLayoutConstraint.Create(UploadButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 24));

				AddConstraint(NSLayoutConstraint.Create(TextInputView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 6));
				AddConstraint(NSLayoutConstraint.Create(TextInputView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, -5));
				AddConstraint(NSLayoutConstraint.Create(TextInputView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.Trailing, 1, -5));

				AddConstraint(NSLayoutConstraint.Create(SeparatorLine, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0));
				AddConstraint(NSLayoutConstraint.Create(SeparatorLine, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 0));
				AddConstraint(NSLayoutConstraint.Create(SeparatorLine, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.Trailing, 1, 0));
				SeparatorLine.AddConstraint(NSLayoutConstraint.Create(SeparatorLine, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 1));

				SendButton.TouchUpInside += (sender, args) =>
				{
					if (!string.IsNullOrEmpty(TextInputView.Text))
					{
						SendButton.Enabled = false;
						try
						{
							var text = TextInputView.Text;
							if (SendMessageHandler != null)
							{
								OnRaiseSendMessageEvent(new SendMessageEventArgs(text));
							}

							TextInputView.Reset();
							UpdateTextViewHeight();
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine(ex);
						}
						finally
						{
							SendButton.Enabled = TextInputView.HasText;
						}
					}
				};

				UploadButton.TouchUpInside += (sender, args) =>
				{
					if (UploadButtonAction != null)
					{
						UploadButtonAction();
					}
				};
			}

			internal void UpdateTextViewHeight()
			{
				if (TextInputView == null)
				{
					return;
				}

				SendButton.Enabled = TextInputView.HasText;

				var oldTextMessageHeight = TextInputView.Frame.Height;
				var diffHeight = HeightConstraint.Constant - oldTextMessageHeight;

				var newSize = TextInputView.SizeThatFits(new CGSize(TextInputView.Bounds.Width, nfloat.MaxValue));
				var newSizeHeight = newSize.Height + diffHeight;

				var maxHeight = IsPortrait ? TextViewMaxHeightPortrait : TextViewMaxHeightLandscape;

				if (newSizeHeight > maxHeight)
				{
					newSizeHeight = maxHeight;
				}

				if (HeightConstraint.Constant != newSizeHeight)
				{
					HeightConstraint.Constant = newSizeHeight;

					if (HeightChangeAction != null)
					{
						HeightChangeAction();
					}

					TextInputView.ReloadInputViews();

					TextInputView.SetContentOffset(new CGPoint(0, 0), true);
				}

				if (SaveUnsentMessageAsync != null)
				{
					SaveUnsentMessageAsync(TextInputView.Text, false);
				}
			}

			internal void HandleTextChange(NSNotificationEventArgs args)
			{
				var field = args.Notification.Object as UITextView;
				if (field == TextInputView)
				{
					UpdateTextViewHeight();
					SendButton.Enabled = TextInputView.HasText;
				}
			}

			internal bool HideKeyboard()
			{
				if (TextInputView.IsFirstResponder)
				{
					TextInputView.ResignFirstResponder();
					return true;
				}

				return false;
			}

			// Wrap event invocations inside a protected virtual method
			// to allow derived classes to override the event invocation behavior
			protected virtual void OnRaiseSendMessageEvent(SendMessageEventArgs e)
			{
				// Make a temporary copy of the event to avoid possibility of
				// a race condition if the last subscriber unsubscribes
				// immediately after the null check and before the event is raised.
				EventHandler<SendMessageEventArgs> handler = SendMessageHandler;

				// Event will be null if there are no subscribers
				if (handler != null)
				{
					handler(this, e);
				}
			}

		}

	}
}