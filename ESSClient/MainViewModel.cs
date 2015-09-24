using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ESSClient
{
    class MainViewModel : INotifyPropertyChanged, IDisposable
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

        private string _Message = "";
        public string Message
        {
            get { return this._Message; }
            set
            {
                if (value != this._Message)
                {
                    this._Message = value;
                    this.NotifyPropertyChanged("Message");
                }
            }
        }

        public ICommand SendTextCommand
        {
            get
            {
                return new DelegateCommand<object>(context =>
                {
                    // don't send empty strings
                    if (string.IsNullOrWhiteSpace(this.Message)) return;

                    // send the message off
                    client.SendMessage(this.Message);

                    // and clear it
                    this.Message = "";
                });
            }
        }

        private TaskFactory UITaskFactory;
        private Client client;
        private string name;

        public MainViewModel()
        {
            Status = "Initializing";
            name = NameGenerator.Generate();
            UITaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            client = new Client(this, name);
            client.Start();
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Close();
                    client = null;
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
