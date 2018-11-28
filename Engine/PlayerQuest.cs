using System.ComponentModel;

namespace Engine
{
    public class PlayerQuest : INotifyPropertyChanged
    {
        private Quest _details;
        private bool _isCompleted;

        public Quest Details
        {
            get => _details;
            set
            {
                _details = value;
                OnPropertyChanged("Details");
            }
        }
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged("IsCompleted");
                OnPropertyChanged("Name");
            }
        }

        public string Name => Details.Name;

        public PlayerQuest( Quest details )
        {
            Details = details;
            IsCompleted = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
