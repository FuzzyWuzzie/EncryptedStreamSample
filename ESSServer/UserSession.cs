using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSServer
{
    class UserSession : INotifyPropertyChanged, IComparable<UserSession>, IEquatable<UserSession>
    {
        private string _Name = "";
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (value != this._Name)
                {
                    this._Name = value;
                    this.NotifyPropertyChanged("Name");
                }
            }
        }

        public UserSession(string name)
        {
            this.Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(UserSession other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(UserSession other)
        {
            throw new NotImplementedException();
        }
    }
}
