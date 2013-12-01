using System.ComponentModel;
using System.Windows;
using net.azirale.geosharer.core;
using System.Threading;

namespace net.azirale.geosharer.wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.files = new BindingList<FilePath>();
            this.InputGrid.ItemsSource = this.files;
        }

        private BindingList<FilePath> files;

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private class FilePath
        {
            public FilePath() { this.Path = string.Empty; }
            public FilePath(string path) { this.Path = path; }
            public string Path { get; set; }
        }

        private void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) this.OutputText.Text = dialog.SelectedPath;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "GeoSharer file (*.geosharer)|*.geosharer";
            dialog.Multiselect = true;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            foreach (string s in dialog.FileNames)
            {
                this.files.Add(new FilePath(s));
            }
        }

        private void InputGrid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string s in paths)
            {
                if (s.EndsWith(".geosharer"))
                {
                    this.files.Add(new FilePath(s));
                }
            }
        }

        private void InputGrid_PreviewDragOver(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            bool hasGeosharer = false;
            foreach (string s in paths)
            {
                hasGeosharer = hasGeosharer | s.EndsWith(".geosharer");
            }
            e.Effects = hasGeosharer ? DragDropEffects.Link : DragDropEffects.None;
            e.Handled = true;
        }

        private void OutputText_Drop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            this.OutputText.Text = paths[0];
        }

        private void OutputText_PreviewDragOver(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (paths == null || paths.Length > 1 || !System.IO.Directory.Exists(paths[0]))
            {
                e.Effects = DragDropEffects.None;
                return;
            }
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            // reader
            GeoReader reader = new GeoReader();
            foreach (FilePath path in this.files)
            {
                reader.AttachFile(path.Path);
            }
            // builder
            WorldBuilder builder = new WorldBuilder();
            string outpath = this.OutputText.Text;
            // worker thread
            Thread worker = new Thread(() => builder.UpdateWorld(outpath, reader));
            // status window
            StatusWindow status = new StatusWindow(worker, builder);
            status.Owner = this;
            status.ShowDialog();
        }
    }
}
