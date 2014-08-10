using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using net.azirale.geosharer.core;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GeoSharerWinForm
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.inputFilePaths = new HashSet<string>();
            ResizeProgressBar();
            this.busy = false;
            this.outputText = new StringBuilder();
            outputDirectoryPath = string.Empty;
            this.StatusProgressbar.Maximum = 10000;
        }

        private HashSet<string> inputFilePaths;
        private string outputDirectoryPath;
        private bool busy;
        StringBuilder outputText;

        #region Form upkeep
        private void ReceiveProgress(object sender, ProgressPacket progress)
        {
            this.Invoke((MethodInvoker)delegate 
            {
                this.StatusProgressbar.Value = (int)(((double)progress.Current / (double)progress.Maximum) * 10000);
                MessageOut(progress.Text);
            });
        }

        public void ReceiveMessage(object sender, MessagePacket msg)
        {
            MessageOut(msg.Text);
        }

        private const int maxMessages = 20;
        private string[] messages = new string[maxMessages];
        int lastMessageIndex = 0;
        private void MessageOut(string message)
        {
            this.Invoke((MethodInvoker)delegate
            {
                for (int i = 9; i > 0; i--)
                {
                    this.messages[i] = this.messages[i - 1];
                }
                this.messages[maxMessages - 1] = message;
                StringBuilder sb = new StringBuilder();
                for (int i = maxMessages - 1; i >= 0; i--)
                {
                    sb.AppendLine(this.messages[i]);
                }
                this.richTextBox1.Focus();
                this.richTextBox1.Text = sb.ToString();
            });
        }

        private void statusStrip1_Resize(object sender, EventArgs e)
        {
            ResizeProgressBar();
        }

        private void ResizeProgressBar()
        {
            this.StatusProgressbar.Width = this.statusStrip1.Width - 150;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitPlease();
        }

        private void ExitPlease()
        {
            if (this.busy) { MessageBox.Show("Please wait for process to complete"); return; }
            this.Close();
        }

        private void pickOutputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.busy) { MessageBox.Show("Please wait for process to complete"); return; }
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            this.OutputFolderText.Text = folderDialog.SelectedPath;
        }

        private void addInputFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.busy) { MessageBox.Show("Please wait for process to complete"); return; }
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.DefaultExt = "geosharer";
            dialog.Filter = "Geosharer files (*.geosharer)|*.geosharer";
            DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            foreach (string filePath in dialog.FileNames)
            {
                this.inputFilePaths.Add(filePath);
            }
            this.listBox1.DataSource = this.inputFilePaths.ToList<string>();
        }

        private void OutputFolderText_TextChanged(object sender, EventArgs e)
        {
            this.outputDirectoryPath = this.OutputFolderText.Text;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.busy)
            {
                MessageBox.Show("Please wait for process to complete");
                e.Cancel = true;
            }
        }
        #endregion

        #region Merge or Create World
        private void mergeToWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Thread workerThread = new Thread(new ThreadStart(MergeCreate));
                workerThread.Name = "UICallMergeCreateWorld";
                workerThread.Start();
            }
            catch (Exception ex)
            {
                ExceptionDump(ex);
            }
        }

        private void MergeCreate()
        {
            this.MergeCreate(true, true, true);
        }

        private void MergeCreateFast()
        {
            this.MergeCreate(false, false, false);
        }

        private void MergeCreate(bool doLighting, bool doFluid, bool doStitching)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.busy = true;
                    this.UseWaitCursor = true;
                    this.Cursor = Cursors.WaitCursor;
                    this.StatusMessagelabel.Text = "Merging World";
                });
                GeoMultifile gmf = new GeoMultifile();
                gmf.Messaging += ReceiveMessage;
                foreach (string s in inputFilePaths)
                {
                    bool isOk = gmf.AttachFile(s);
                    if (!isOk) MessageOut("Input file '" + s + "' failed. Wrong version or filetype?");
                }
                List<GeoChunkRaw> rawData = gmf.GetLatestChunkData(this.UpToDatePicker.Value);
                GeoWorldWriter gww = new GeoWorldWriter();
                gww.Progressing += ReceiveProgress;
                gww.UpdateWorld(outputDirectoryPath, rawData, doFluid, doLighting, doStitching);
                MessageOut("Added " + gww.Added + " chunks");
                MessageOut("Updated " + gww.Updated + " chunks");
                MessageOut("Run command complete.");
                this.Invoke((MethodInvoker)delegate
                {
                    this.busy = false;
                    this.UseWaitCursor = false;
                    this.Cursor = Cursors.Default;
                    this.StatusMessagelabel.Text = "Ready";
                });
            }
            catch (Exception ex)
            {
                ExceptionDump(ex);
            }
        }
        #endregion



        #region Trimming
        private void createTrimmedDatafileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread workerThread = new Thread(this.TrimFiles);
            workerThread.Name = "UITrimFilesCall";
            workerThread.Start();
        }

        public void TrimFiles()
        {
            try
            {
                this.busy = true;
                this.StatusMessagelabel.Text = "Trimming files";
                Stopwatch clock = new Stopwatch();
                clock.Start();
                List<FileInfo> files = new List<FileInfo>();
                foreach (string filePath in this.inputFilePaths)
                {
                    FileInfo file = new FileInfo(filePath);
                    files.Add(file);
                }
                GeoMultifile gmf = new GeoMultifile();
                gmf.Messaging += ReceiveMessage;
                StringBuilder report = new StringBuilder();
                MessageOut("Adding files...");
                foreach (FileInfo fi in files)
                {
                    if (!gmf.AttachFile(fi.FullName)) report.AppendLine("File \"" + fi.FullName + "\" not valid - different version?");
                    else report.AppendLine("    Added file \"" + fi.FullName + "\"");
                }
                if (files.Count == 0) { MessageOut("No files to trim."); return; }
                MessageOut(report.ToString());
                MessageOut("Getting latest data for each chunk");
                List<GeoChunkRaw> chunks = gmf.GetLatestChunkData();
                MessageOut("Got " + chunks.Count + " up-to-date chunks");
                TrimWriteChunks(this.outputDirectoryPath, chunks);
                clock.Stop();
                MessageOut("Trim Complete in " + clock.ElapsedMilliseconds.ToString("#,##0") + " ms");
                this.StatusMessagelabel.Text = "Ready";
            }
            catch (Exception ex)
            {
                ExceptionDump(ex);
            }
            finally
            {
                this.busy = false;
            }
        }

        public void TrimWriteChunks(string directoryPath, List<GeoChunkRaw> chunks)
        {
            string outPath = directoryPath + "/TrimmedFiles_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".geosharer";
            FileInfo outFile = new FileInfo(outPath);
            using (FileStream outStream = outFile.Create())
            using (GZipStream zipStream = new GZipStream(outStream, CompressionMode.Compress))
            using (BinaryWriter datStream = new BinaryWriter(zipStream))
            {
                MessageOut("Writing up-to-date chunks to \"" + outPath + "\"");
                int version = 4;
                int numChunks = chunks.Count;
                int[] x = new int[numChunks];
                int[] z = new int[numChunks];
                long[] timestamp = new long[numChunks];
                int[] chunkStarts = new int[numChunks];
                int thisStart = 4 + 4 + (numChunks * (4 + 4 + 8 + 4)); // VERSION + NUMCHUNKS + [numchunks]*X+Z+TIME+START
                for (int i = 0; i < numChunks; ++i)
                {
                    GeoChunkRaw chunk = chunks[i];
                    x[i] = Endian.Reverse(chunk.X);
                    z[i] = Endian.Reverse(chunk.Z);
                    timestamp[i] = Endian.Reverse(chunk.TimeStamp);
                    chunkStarts[i] = Endian.Reverse(thisStart);
                    thisStart += chunk.Data.Length;
                }
                datStream.Write(Endian.Reverse(version));
                datStream.Write(Endian.Reverse(numChunks));
                for (int i = 0; i < numChunks; ++i) datStream.Write(x[i]);
                for (int i = 0; i < numChunks; ++i) datStream.Write(z[i]);
                for (int i = 0; i < numChunks; ++i) datStream.Write(timestamp[i]);
                for (int i = 0; i < numChunks; ++i) datStream.Write(chunkStarts[i]);
                for (int i = 0; i < numChunks; ++i) datStream.Write(chunks[i].Data);
                MessageOut("Done writing");
            }
        }
        #endregion

        private void ExceptionDump(Exception ex)
        {
            try
            {
                MessageOut("UNHANDLED EXCEPTION! MergeCreate() aborted.");
                MessageOut(ex.ToString());
                FileInfo fi = new FileInfo("Crashlog_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".txt");
                using (StreamWriter sr = fi.CreateText())
                {
                    sr.Write(this.richTextBox1.Text);
                    sr.Dispose();
                }
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Woah, secondary exception total failure\n" + ex2.Message);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedPaths = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (droppedPaths.Length == 1 && Directory.Exists(droppedPaths[0]))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else foreach (string path in droppedPaths)
                    {
                        if (path.EndsWith(".geosharer"))
                        {
                            e.Effect = DragDropEffects.Copy;
                            break;
                        }
                    }
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] droppedPaths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (droppedPaths.Length == 1 && Directory.Exists(droppedPaths[0]))
            {
                DropDirectory(droppedPaths[0]);
            }
            else
            {
                DropFiles(droppedPaths);
            }
        }

        private void DropDirectory(string directoryPath)
        {
            this.OutputFolderText.Text = directoryPath;
        }

        private void DropFiles(string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                if (filePath.EndsWith(".geosharer")) this.inputFilePaths.Add(filePath);
            }
            this.listBox1.DataSource = this.inputFilePaths.ToList<string>();
        }

        private void mergeCreateFastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Thread workerThread = new Thread(new ThreadStart(MergeCreateFast));
                workerThread.Name = "UICallMergeCreateWorldFast";
                workerThread.Start();
            }
            catch (Exception ex)
            {
                ExceptionDump(ex);
            }
        }

        private void recalculateWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Thread workerThread = new Thread(new ThreadStart(Recalculation));
                workerThread.Name = "UICallRecalculation";
                workerThread.Start();
            }
            catch (Exception ex)
            {
                ExceptionDump(ex);
            }
        }

        private void Recalculation()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                    {
                        this.busy = true;
                        this.UseWaitCursor = true;
                        this.Cursor = Cursors.WaitCursor;
                        this.StatusMessagelabel.Text = "Recalculating World";
                    });
                GeoWorldWriter gww = new GeoWorldWriter();
                gww.Progressing += ReceiveProgress;
                gww.RecalculateWorld(outputDirectoryPath, true, true, true);
                this.Invoke((MethodInvoker)delegate
                {
                    this.busy = false;
                    this.UseWaitCursor = false;
                    this.Cursor = Cursors.Default;
                    this.StatusMessagelabel.Text = "Ready";
                });
            }
            catch (Exception ex)
            {
                ExceptionDump(ex);
            }
        }
    }
}
