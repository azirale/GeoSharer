using System.Windows;
using net.azirale.geosharer.core;
using System.ComponentModel;

namespace net.azirale.geosharer.wpf
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window, INotifyPropertyChanged
    {
        public ReportWindow(WorldBuilder builder)
        {
            this.myBuilder = builder;
            InitializeComponent();
        }

        private WorldBuilder myBuilder;

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        // properties and property change events
        private int _chunksAdded;
        public int ChunksAdded
        {
            get { return this._chunksAdded; }
            private set
            {
                int old = this._chunksAdded;
                this._chunksAdded = value;
                if (old != value) OnPropertyChanged("ChunksAdded");
            }
        }
        private int _chunksUpdated;
        public int ChunksUpdated
        {
            get { return this._chunksUpdated; }
            private set
            {
                int old = this._chunksUpdated;
                this._chunksUpdated = value;
                if (old != value) OnPropertyChanged("ChunksUpdated");
            }
        }
        private int _chunksIgnored;
        public int ChunksIgnored
        {
            get { return this._chunksIgnored; }
            private set
            {
                int old = this._chunksIgnored;
                this._chunksIgnored = value;
                if (old != value) OnPropertyChanged("ChunksIgnored");
            }
        }
        private int _chunksFailed;
        public int ChunksFailed
        {
            get { return this._chunksFailed; }
            private set
            {
                int old = this._chunksFailed;
                this._chunksFailed = value;
                if (old != value) OnPropertyChanged("ChunksFailed");
            }
        }

        private string _timeTaken;
        public string TimeTaken
        {
            get { return _timeTaken; }
            private set
            {
                this._timeTaken = value;
                OnPropertyChanged("TimeTaken");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handle = this.PropertyChanged;
            if (handle != null) handle(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ReportWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            this.ChunksAdded = myBuilder.ChunksAdded;
            this.ChunksUpdated = myBuilder.ChunksUpdated;
            this.ChunksIgnored = myBuilder.ChunksIgnored;
            this.ChunksFailed = myBuilder.ChunksFailed;
            this.TimeTaken = myBuilder.TimeTaken.ToString(@"hh\:mm\:ss");
        }
    }
}
