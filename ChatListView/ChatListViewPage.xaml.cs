using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace ChatListView
{
	public partial class ChatListViewPage : ContentPage
	{
		public ChatListViewPage()
		{
			InitializeComponent();
		}

		public Command<String> SendMessage
		{
			get
			{
				return new Command<String>((message) =>
			   {
			   });

			}
		}


	}
}
