using EconomicWPF;
using EconomicWPF.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EconomicWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private DateTime _currentDate;

        public DateTime CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            CurrentDate = DateTime.Now;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}