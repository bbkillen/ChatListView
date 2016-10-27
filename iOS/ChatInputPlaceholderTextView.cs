using System;
using UIKit;

namespace ChatListView.iOS.Controls
{
	public partial class ChatInputPlaceholderTextView : UITextView
	{
		public ChatInputPlaceholderTextView(IntPtr handle)
			: base(handle)
		{
			Placeholder = string.Empty;
		}

		public ChatInputPlaceholderTextView(UIButton SendButton)
			: base()
		{
			Placeholder = string.Empty;
			_sendButton = SendButton;
		}

		public string Placeholder { get; set; }

		private UILabel PlaceholderLabel { get; set; }

		private UIButton _sendButton;

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (PlaceholderLabel == null)
			{
				PlaceholderLabel = new UILabel();
				PlaceholderLabel.TranslatesAutoresizingMaskIntoConstraints = false;
				PlaceholderLabel.TextColor = UIColor.LightGray;
				PlaceholderLabel.Font = Font;
				PlaceholderLabel.Text = Placeholder;

				AddSubview(PlaceholderLabel);
				AddSubview(_sendButton);

				AddConstraint(NSLayoutConstraint.Create(PlaceholderLabel, NSLayoutAttribute.CenterY,  NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY,  1,  0));
				AddConstraint(NSLayoutConstraint.Create(PlaceholderLabel, NSLayoutAttribute.Leading,  NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading,  1, 10));
				AddConstraint(NSLayoutConstraint.Create(PlaceholderLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.Trailing, 1,  0));

				AddConstraint(NSLayoutConstraint.Create(_sendButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0));
				AddConstraint(NSLayoutConstraint.Create(_sendButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 2, -16));

				Changed += (sender, e) =>
				{
					UpdatePlaceholder();
				};

				PlaceholderLabel.Hidden = !string.IsNullOrEmpty(Text);
			}
		}

		public void Reset()
		{
			Text = string.Empty;
			UpdatePlaceholder();
		}

		private void UpdatePlaceholder()
		{
			if (PlaceholderLabel != null)
			{
				PlaceholderLabel.Hidden = !string.IsNullOrEmpty(Text);
			}
		}
	}
}

