using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nexus.Client.Util;

namespace Util.Test.Archive.Mocks
{
    interface ITestArchiveItem : IArchiveItem
    {
        Folder Parent { set; }
    }

    class Folder : ITestArchiveItem, IArchiveFolder
    {
        public Folder(params ITestArchiveItem[] children) : this("", children)
        {
        }

        public Folder(string name, params ITestArchiveItem[] children)
        {
            Name = name;
            SubFolders = children.OfType<IArchiveFolder>();
            Files = children.OfType<IArchiveFile>();
            foreach (var child in children)
            {
                child.Parent = this;
            }
        }

        public string Name { get; private set; }

        public IEnumerable<IArchiveFolder> SubFolders { get; private set; }
        public IEnumerable<IArchiveFile> Files { get; private set; }

        public string FullPath
        {
            get
            {
                if (Parent == null) return Name;
                return Path.Combine(Parent.FullPath, Name);
            }
        }

        public Folder Parent { private get; set; }
    }

    class File : ITestArchiveItem, IArchiveFile
    {
        public File(string name)
        {
            Name = name;
            int dotIndex = name.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
            if (dotIndex != -1)
            {
                Extension = name.Substring(dotIndex + 1);
            }
            else
            {
                Extension = string.Empty;
            }

        }

        public string Name { get; private set; }

        public Folder Parent { private get; set; }
        public string Extension { get; private set; }

        public string FullPath
        {
            get
            {
                if (Parent == null) return Name;
                return Path.Combine(Parent.FullPath, Name);
            }
        }
    }
}
