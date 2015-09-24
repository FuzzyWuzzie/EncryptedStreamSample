using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSServer
{
    class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _Log = new ObservableCollection<string>();
        public ObservableCollection<string> Log
        {
            get { return this._Log; }
            set
            {
                if (value != this._Log)
                {
                    this._Log = value;
                    this.NotifyPropertyChanged("Log");
                }
            }
        }

        private string _Status = "";
        public string Status
        {
            get { return this._Status; }
            set
            {
                if (value != this._Status)
                {
                    this._Status = value;
                    LogMessage(value);
                    this.NotifyPropertyChanged("Status");
                }
            }
        }

        private ObservableCollection<UserSession> _UserSessions = new ObservableCollection<UserSession>();
        public ObservableCollection<UserSession> UserSessions
        {
            get { return this._UserSessions; }
            set
            {
                if (value != this._UserSessions)
                {
                    this._UserSessions = value;
                    this.NotifyPropertyChanged("UserSessions");
                }
            }
        }

        private TaskFactory UITaskFactory;
        private Server server;

        public MainViewModel()
        {
            Status = "Initializing";
            UITaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            server = new Server(this);
            server.Start();
        }

        private void LogMessage(string message)
        {
            Log.Add("<" + DateTime.Now.ToString("HH:mm:ss") + "> " + message);
        }

        public void TS_SetStatus(string status)
        {
            if (UITaskFactory != null) UITaskFactory.StartNew(() => this.Status = status);
        }

        public void TS_LogMessage(string message)
        {
            if (UITaskFactory != null) UITaskFactory.StartNew(() => LogMessage(message));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
