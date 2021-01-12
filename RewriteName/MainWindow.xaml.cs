using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Text.RegularExpressions;

namespace RewriteName
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand ExitCommand = new RoutedUICommand("Exit", "Exit", typeof(MainWindow));
        string selectedDirectory = "";
        BackgroundWorker worker;
        string saveFileFolderRoot = @"RenameApp\DataExport";
        string saveFileSrc = "";
        string newName = "";
        bool isRunning;
        bool isDeleting = false;
        string symbol;
        System.Windows.Forms.Timer timer;
        public MainWindow()
        {
            InitializeComponent();
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000;
            timer.Tick += timer_Tick;
            timer.Disposed += timer_Disposed;
         //   timer.Start();


            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            isRunning = false;

            InitCombobox();

        }

        void timer_Disposed(object sender, EventArgs e)
        {
            Thread thread = new Thread(playMusic);
            thread.Start();

        }

        private void play()
        {
            if (PlayMucsic.Play() == 0)
            {
                var question = System.Windows.MessageBox.Show("The file was deleted, do you want load new file ?", "You are a vandaler", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (question == MessageBoxResult.Yes)
                {
                    Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                    openFileDialog.Filter = "MP3 files (*.mp3,*.wav)|*.mp3;*.wav";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        PlayMucsic.Play(openFileDialog.FileName);
                    }

                }
            }
        }
        private void playMusic()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var rsult = System.Windows.MessageBox.Show("Do you want to listen some music ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (rsult == MessageBoxResult.Yes)
                {
                    play();
                }
                else
                {
                    var rs = System.Windows.MessageBox.Show("Are you sure ?", "Please think again", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (rs == MessageBoxResult.Yes)
                    {
                        play();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Oh shit, fuck you !!!", "Fuck !!!", MessageBoxButton.OK, MessageBoxImage.Hand);
                    }
                   
                }
            }));
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Dispose();
        }

        public void InitCombobox()
        {
            cbbSymbol.Items.Add("-");
            cbbSymbol.Items.Add("_");
            cbbSymbol.Items.Add("( )");
            symbol = cbbSymbol.SelectedItem as string;
        }

        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;

            if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtDirectory.Text = folderDlg.SelectedPath;
                selectedDirectory = folderDlg.SelectedPath;
            }
        }

        public void DoStuff()
        {
            isRunning = !isRunning;
            if (isRunning)
            {
                object countLock = new object();
                lock (countLock)
                {
                    worker.RunWorkerAsync();
                }
                btnStart.IsEnabled = false;
            }
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            newName = txtName.Text;
            if (string.IsNullOrEmpty(newName))
            {
                System.Windows.MessageBox.Show("Please enter new name !", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (!isValidName(newName))
            {
                System.Windows.MessageBox.Show("File name is invalid !", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!Directory.Exists(selectedDirectory))
            {
                System.Windows.MessageBox.Show("Directory does not exists !", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var files = Directory.GetFiles(selectedDirectory);
            if (files.Length == 0)
            {
                System.Windows.MessageBox.Show("Folder is empty !", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            saveFileSrc = System.IO.Path.Combine(selectedDirectory, saveFileFolderRoot);
            if (!Directory.Exists(saveFileSrc))
            {
                Directory.CreateDirectory(saveFileSrc);
            }
            else
            {
                isDeleting = true;
                // Array.ForEach(Directory.GetFiles(saveFileSrc), delegate(string path) { File.Delete(path); });
            }

            DoStuff();

        }

        public bool isValidName(string filename)
        {
            string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (regInvalidFileName.IsMatch(filename)) { return false; };
            return true;
        }
        public void DoTheWork(string file, string newName, string saveFileSrc, int i)
        {
            string filename;
            string _i = i < 10 ? string.Concat("0", i) : i.ToString();

            if (symbol == "( )")
            {
                filename = string.Format("{0}{1}{2}{3}{4}", newName, "(", _i, ")", System.IO.Path.GetExtension(file));
            }
            else
            {
                filename = string.Format("{0}{1}{2}{3}", newName, symbol, _i, System.IO.Path.GetExtension(file));
            }
            try
            {
                File.Copy(System.IO.Path.Combine(selectedDirectory, System.IO.Path.GetFileName(file)), System.IO.Path.Combine(saveFileSrc, filename));
            }
            catch
            {
                Directory.Delete(System.IO.Path.Combine(saveFileSrc, System.IO.Path.Combine(saveFileSrc, filename)));
                File.Copy(System.IO.Path.Combine(selectedDirectory, System.IO.Path.GetFileName(file)), System.IO.Path.Combine(saveFileSrc, filename));
            }

        }


        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtDirectory.Clear();
            txtName.Clear();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                var result = System.Windows.MessageBox.Show("Do you want to stop ?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DoStuff();
                    this.Close();
                }
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int _counter = 0;
            var countLock = new object();
            if (isDeleting)
            {
                var deleteFiles = Directory.GetFiles(saveFileSrc);
                Parallel.For(_counter, deleteFiles.Length, (i, state) =>
                {
                    if (!isRunning)
                    {
                        state.Break();
                    }

                    var currentFile = deleteFiles[i];
                    File.Delete(currentFile);
                    _counter++;
                    var percentage = Math.Ceiling((decimal)_counter / deleteFiles.Length * 100);
                    lock (countLock) { worker.ReportProgress((int)percentage); }
                });

                isDeleting = false;
            }

            var files = Directory.GetFiles(selectedDirectory);
            _counter = 0;

            Parallel.For(_counter, files.Length, (i, state) =>
            {
                if (!isRunning)
                {
                    state.Break();
                }

                var currentFile = files[i];
                DoTheWork(currentFile, newName, saveFileSrc, i);

                _counter++;
                var percentage = Math.Ceiling((decimal)_counter / files.Length * 100); //(double)_counter / files.Length * 100.0;
                lock (countLock) { worker.ReportProgress((int)percentage); }
            });

        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string action = !isDeleting ? "Moving..." : "Deleting...";
            tbProgress.Text = action + e.ProgressPercentage + " %";
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            tbProgress.Text = "";
            isRunning = false;
            btnStart.IsEnabled = true;
            System.Windows.MessageBox.Show("Rename Complete !", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            System.Diagnostics.Process.Start(saveFileSrc);
        }

        private void cbbSymbol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            symbol = cbbSymbol.SelectedItem as string;
        }

      
        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpFr wd = new HelpFr();
            wd.ShowDialog();
        }

        private void ExitBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ExitBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

      

    }
}
