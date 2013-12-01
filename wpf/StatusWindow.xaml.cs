using System.Windows;
using net.azirale.geosharer.core;
using System.ComponentModel;
using System.Threading;
using System;

namespace net.azirale.geosharer.wpf
{
    /// <summary>
    /// Interaction logic for StatusWindow.xaml
    /// </summary>
    public partial class StatusWindow : Window, INotifyPropertyChanged
    {
        public StatusWindow(Thread thread, IProgressSender sender)
        {
            InitializeComponent();
            this.worker = thread;
            this.sender = sender;
            this._current = 0;
            this._max = long.MaxValue;
            this.hasCompleted = false;
        }

        public void RecieveProgress(object sender, ProgressPacket progress)
        {
            // Assign the values before updating the UI
            this._current = progress.Current;
            this._max = progress.Maximum;
            this._labeltext = progress.Text;
            if (progress.Current == progress.Maximum)
            {
                lock (this.worker)
                {
                    if (!this.hasCompleted)
                    {
                        this.hasCompleted = true;
                        new Thread(() => { Thread.Sleep(1000); this.Dispatcher.Invoke(new Action(() => { this.Close(); })); }).Start();
                    }
                }
            }
            OnPropertyChanged("Max");
            OnPropertyChanged("Current");
            OnPropertyChanged("LabelText");
        }

        private bool hasCompleted;
        private long _current;
        public long Current { get { return _current; } }
        private long _max;
        public long Max { get { return _max; } }
        private string _labeltext;
        public string LabelText { get { return this._labeltext; } }

        private Thread worker;
        private IProgressSender sender;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void StatusForm_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            this.sender.Progressing += this.RecieveProgress;
            worker.Start();
        }
    }
}
