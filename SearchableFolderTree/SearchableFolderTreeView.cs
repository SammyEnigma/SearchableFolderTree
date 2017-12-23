using AdamOneilSoftware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SearchableFolderTree
{
    public partial class SearchableFolderTreeView : UserControl
    {
        public event EventHandler FilterChanged;

        private bool _inProgress = false;
        private bool _canceled = false;

        private delegate FolderNode AddFolderNodeHandler(FolderNode parent, string path);

        public string RootPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public SearchableFolderTreeView()
        {
            InitializeComponent();
            searchBox.TextChanged += OnFilterChanged;
            searchBox.KeyDown += OnFilterKeyDown;
            progressBar.Visible = false;
        }

        private async void OnFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (_inProgress) _canceled = true;
                e.Handled = true;

                await FillAsync();                
            }
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            FilterChanged?.Invoke(this, e);
        }

        public string Filter
        {
            get { return searchBox.Text; }
            set { searchBox.Text = value; }
        }        

        public async Task FillAsync()
        {
            _canceled = false;
            _inProgress = true;

            treeView.Nodes.Clear();
            
            FolderNode ndRoot = new FolderNode(RootPath);
            treeView.Nodes.Add(ndRoot);

            string[] queryWords = Filter?.Split(' ').Select(s => s.Trim().ToLower()).ToArray();

            progressBar.Visible = true;
            treeView.BeginUpdate();

            var results = await FindFolders(RootPath, queryWords);
            var treeResults = FileSystem.GetFolderTree(results, checkFileExistence: false);



            treeView.EndUpdate();
            progressBar.Visible = false;

            _inProgress = false;
        }

        private static async Task<IEnumerable<string>> FindFolders(string path, string[] queryWords)
        {
            List<string> results = new List<string>();

            await Task.Run(() =>
            {
                FindFoldersR(results, path, queryWords);
            });

            return results;
        }

        private static void FindFoldersR(List<string> results, string path, string[] queryWords)
        {
            string[] subFolders = Directory.GetDirectories(path);

            foreach (string subFolder in subFolders)
            {
                if (MeetsCriteria(subFolder, queryWords)) results.Add(subFolder);

                FindFoldersR(results, subFolder, queryWords);
            }
        }

        private void FillAsyncInnerR(FolderNode ndParent, string path, string[] queryWords)
        {
            string[] subFolders = Directory.GetDirectories(path);            

            foreach (string subFolder in subFolders)
            {
                if (_canceled) break;
                
                AddFolderNodeHandler callback = AddFolderNode;
                FolderNode ndChild = treeView.Invoke(callback, ndParent, subFolder) as FolderNode;

                FillAsyncInnerR(ndChild, subFolder, queryWords);
            }
        }

        private static bool MeetsCriteria(string path, string[] words)
        {
            if (words?.Length == 0) return true;            
            return words.All(w => path.ToLower().Contains(w));
        }

        private FolderNode AddFolderNode(FolderNode ndParent, string path)
        {
            FolderNode ndChild = new FolderNode(path);
            ndParent.Nodes.Add(ndChild);
            return ndChild;
        }
    }
}