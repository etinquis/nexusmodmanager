using System.Collections.Generic;
using System.IO;

namespace Util.Test.Archive.Mocks
{
    class ArchiveAttributes
    {
        public ArchiveAttributes(string path, Folder rootDirectory)
        {
            System.IO.FileInfo info = new FileInfo(path);
            Path = path;
            ExtractedPath = System.IO.Path.Combine(info.DirectoryName, System.IO.Path.GetFileNameWithoutExtension(info.FullName));
            DirectoryStructure = rootDirectory;
        }

        public string Path { get; set; }
        public string ExtractedPath { get; private set; }
        public Folder DirectoryStructure { get; set; }
    }

    class ArchiveAttributeFactory
    {
        private const string BASIC_7Z_PATH = "../../../objects/basic.7z";

        public static IEnumerable<ArchiveAttributes> ArchiveAttributes
        {
            get
            {
                yield return new ArchiveAttributes(
                        BASIC_7Z_PATH,
                        new Folder(
                                new Folder("emptyfolder"),
                                new Folder("folder",
                                        new File("text.txt"),
                                        new File("noextension")
                                    ),
                                new File("text.txt"),
                                new File("noextension")
                        )
                    );
            }
        }
    }
}
