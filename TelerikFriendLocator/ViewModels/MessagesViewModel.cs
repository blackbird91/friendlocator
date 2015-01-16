
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using Microsoft.Phone.Reactive;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using TelerikFriendLocator.Managers;
using TelerikFriendLocator.Wrappers;

namespace TelerikFriendLocator.ViewModels
{
    public class MessagesViewModel : ViewModelBase
    {
        private const int DefaultUserId = 4;
        private ObservableCollection<CustomMessage> messages;
        private ObservableCollection<Person> people;
        private Person you;
        private Person conversationBuddy;
        private int currentGroup = 0;
        private CustomMessage previousMessage;

        public async void SendMessage(CustomMessage message)
        {
            string sender = GeneralManager.Instance.LoggedInUser.FacebookId;
            string receiver = GeneralManager.Instance.Person2.FacebookId;

            var messageTable = App.serviceClient.GetTable<Message>();

            await messageTable.InsertAsync(new Message()
            {
                Content = message.Text,
                Receiver = receiver,
                Sender = sender,
                Timestamp = message.TimeStamp
            });
        }

        private async void GetMessages()
        {
            string sender = GeneralManager.Instance.LoggedInUser.FacebookId;
            string receiver = GeneralManager.Instance.Person2.FacebookId;

            var messageTable = App.serviceClient.GetTable<Message>();
            var currentMessages = await messageTable.Where(u => (u.Sender == sender && u.Receiver == receiver) || (u.Sender == receiver && u.Receiver == sender)).ToListAsync();

            this.messages.Clear();

            foreach (var message in currentMessages)
            {
                this.messages.Add(new CustomMessage(
               message.Content, message.Timestamp,
               message.Sender == sender ? ConversationViewMessageType.Outgoing : ConversationViewMessageType.Incoming,
               message.Sender));
            }
        }

        private void InitializeMessages()
        {
            this.messages = new ObservableCollection<CustomMessage>();
            this.messages.CollectionChanged += OnMessagesCollectionChanged;

            DispatcherTimer timer = new DispatcherTimer();

            timer.Tick += timer_Tick;

            timer.Interval = new TimeSpan(0, 0, 15);
            timer.Start();

            GetMessages();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            GetMessages();
        }

        private void OnMessagesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                return;
            }
            CustomMessage message = e.NewItems[0] as CustomMessage;
            if (previousMessage != null)
            {
                if (previousMessage.SenderId != message.SenderId)
                {
                    this.currentGroup++;
                }
            }
            if (message.Group == null)
            {
                message.Group = this.currentGroup;
            }
            previousMessage = message;

        }

        private void InitializePeople()
        {
            this.people = new ObservableCollection<Person>();

            User first = GeneralManager.Instance.LoggedInUser;
            User second = GeneralManager.Instance.Person2;


            Person person1 = new Person() { PersonId = first.FacebookId, Name = string.Join(" ", first.Firstname, first.Lastname), Picture = new Uri(first.PictureUrl) };

            Person person2 = new Person() { PersonId = second.FacebookId, Name = string.Join(" ", second.Firstname, second.Lastname), Picture = new Uri(second.PictureUrl) };

            this.people.Add(person2);
            this.people.Add(person1);
        }

        public Person You
        {
            get
            {
                return this.you;
            }
            set
            {
                this.you = value;
                this.OnPropertyChanged("You");
            }
        }

        public Person ConversationBuddy
        {
            get
            {
                return this.conversationBuddy;
            }
            set
            {
                this.conversationBuddy = value;
                this.OnPropertyChanged("ConversationBuddy");
            }
        }

        public ObservableCollection<CustomMessage> Messages
        {
            get
            {
                if (this.messages == null)
                {
                    this.InitializeMessages();
                }
                return this.messages;
            }
            private set
            {
                this.messages = value;
            }
        }

        public ObservableCollection<Person> People
        {
            get
            {
                if (this.people == null)
                {
                    this.InitializePeople();
                }
                return this.people;
            }
            private set
            {
                this.people = value;
            }
        }
    }

    public class CustomMessage : ConversationViewMessage, IComparable
    {
        public CustomMessage(string text, DateTime timeStamp, ConversationViewMessageType type, string senderId, int? group = null)
            : base(text, timeStamp, type)
        {
            this.SenderId = senderId;
            this.Group = group;
        }

        public string SenderId
        {
            get;
            private set;
        }

        public int? Group
        {
            get;
            set;
        }

        public SolidColorBrush MessageBackground
        {
            get
            {
                int id = int.Parse(this.SenderId[0].ToString()) % 6;
                switch (id)
                {
                    case 0: return new SolidColorBrush(Color.FromArgb(255, 51, 153, 51));
                    case 1: return new SolidColorBrush(Color.FromArgb(255, 27, 161, 226));
                    case 2: return new SolidColorBrush(Color.FromArgb(255, 255, 0, 151));
                    case 3: return new SolidColorBrush(Color.FromArgb(255, 240, 150, 9));
                    case 4: return new SolidColorBrush(Color.FromArgb(255, 0, 171, 169));
                    case 5: return new SolidColorBrush(Color.FromArgb(255, 140, 191, 38));
                }
                return App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
        }

        public string FormattedTimeStamp
        {
            get
            {
                return this.TimeStamp.ToShortTimeString();
            }
        }

        public override bool Equals(object obj)
        {
            CustomMessage secondMessage = obj as CustomMessage;

            if (obj is DataGroup)
            {
                secondMessage = (obj as DataGroup).Key as CustomMessage;
            }

            return this.Group == secondMessage.Group;
        }

        public override int GetHashCode()
        {
            return this.Group.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            CustomMessage targetMessage = obj as CustomMessage;

            if (targetMessage != null)
            {
                return this.Group > targetMessage.Group ? 1 : this.Group == targetMessage.Group ? 0 : -1;
            }

            if (obj is DataGroup)
            {
                targetMessage = (obj as DataGroup).Key as CustomMessage;

                return this.Group > targetMessage.Group ? 1 : this.Group == targetMessage.Group ? 0 : -1;
            }

            return 0;
        }
    }

    public class Person : ViewModelBase
    {
        private string personId;
        private string name;
        private Uri picture;

        public string PersonId
        {
            get
            {
                return this.personId;
            }
            set
            {
                if (this.personId != value)
                {
                    this.personId = value;
                    this.OnPropertyChanged("PersonId");
                }
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }

        public Uri Picture
        {
            get
            {
                return this.picture;
            }
            set
            {
                if (this.picture != value)
                {
                    this.picture = value;
                    this.OnPropertyChanged("Picture");
                }
            }
        }
    }
}
