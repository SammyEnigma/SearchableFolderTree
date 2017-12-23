using System.IO;
using System.Windows.Forms;

namespace SearchableFolderTree
{
    public class FolderNode : TreeNode
    {
        private readonly string _path;

        public FolderNode(string path) : base(Path.GetFileName(path))
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"Path not found: {path}");
            _path = path;
        }
        
        public new string FullPath
        {
            get { return _path; }
        }
    }
}