
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;
using TelerikFriendLocator.ViewModels;

namespace TelerikFriendLocator
{
    public partial class Messages : PhoneApplicationPage
    {
        public Messages()
        {
            InitializeComponent();
            this.SetConversationParticipants();
        }

        private void SetConversationParticipants()
        {
            MessagesViewModel viewModel = this.DataContext as MessagesViewModel;
            viewModel.ConversationBuddy = viewModel.People[0];
            viewModel.You = viewModel.People[1];
        }

        private void OnSendingMessage(object sender, ConversationViewMessageEventArgs e)
        {
            if (string.IsNullOrEmpty((e.Message as ConversationViewMessage).Text))
            {
                return;
            }
            ConversationViewMessage originalMessage = e.Message as ConversationViewMessage;
            MessagesViewModel viewModel = this.DataContext as MessagesViewModel;
            CustomMessage customMessage = new CustomMessage(originalMessage.Text, originalMessage.TimeStamp, originalMessage.Type, viewModel.You.PersonId);
            viewModel.SendMessage(customMessage);
            
            viewModel.Messages.Add(customMessage);
        }
    }
}
