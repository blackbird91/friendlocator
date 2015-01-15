using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using TelerikFriendLocator.Wrappers;

namespace TelerikFriendLocator.ViewModels
{
    class SettingsViewModel : ViewModelBase,INotifyPropertyChanged
    {
        private User _loggedInUser;
        public SettingsViewModel(User user)
        {
            _loggedInUser = user;

            if (user != null)
            {
                IsUserVisible = user.Visible;
                RangeValue = user.Range < 200 ? 200 : (double)user.Range;
            }
        }

        private bool _isUserVisible = false;
        public bool IsUserVisible
        {
            get { return _isUserVisible; }
            set
            {
                _isUserVisible = (bool)value;
                OnPropertyChanged("IsUserVisible");
                OnPropertyChanged("IsUserVisibleText");
            }
        }

        private string _isUserVisibleText = "Off";
        public string IsUserVisibleText
        {
            get { return _isUserVisible ? "On" : "Off"; }
        }

        private double _rangeValue = 200;
        public double RangeValue {
            get { return _rangeValue; }
            set
            {
                _rangeValue = value;
                OnPropertyChanged("RangeValue");
                OnPropertyChanged("RangeValueText");

            }
        }

        private string _rangeValueText = "200 m";
        public string RangeValueText
        {
            get { return string.Format("{0} m", (int)_rangeValue); }
        }
    }
}
