using System.ComponentModel;

namespace Engine
{
    public class LivingCreature : INotifyPropertyChanged
    {
        private int _currentHitPoints;
        public LivingCreature( int currentHitPoints, int maximumHitPoints )
        {
            CurrentHitPoints = currentHitPoints;
            MaximumHitPoints = maximumHitPoints;
        }

        public int CurrentHitPoints
        {
            get => _currentHitPoints;
            set
            {
                _currentHitPoints = value;
                OnPropertyChanged("CurrentHitPoints");
            }
        }

        public int MaximumHitPoints { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs(name));
        }
    }
}
