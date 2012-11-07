using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Moq;
using Nexus.Client.Util;
using Util.Test.Archive.Mocks;

namespace Util.Test.Archive
{
    using NUnit.Framework;

    [TestFixture]
    class IArchiveTests
    {
        protected IArchive ConstructArchive(ArchiveExtractorFactory.ArchiveExtractorTestContext extractor, IArchiveCompressor compressor = null)
        {
            return new CArchive(extractor.ArchiveExtractorObject);
        }

        [Test]
        public void Constructor()
        {
            Assert.Throws(Is.InstanceOf(typeof (ArchiveException)), () => new CArchive(null));
        }

        [Test]
        public void ContainsDirectory(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor);

            CheckExistingDirectoriesInFolder(archiveUnderTest, archiveExtractor.ArchiveExtractorObject.ReadFolderStructure());
            Assert.IsFalse(archiveUnderTest.ContainsDirectory("thisdirectorycouldnt/possibly/exist"));
            Assert.Throws(Is.InstanceOf(typeof(ArchiveException)), () => archiveUnderTest.ContainsDirectory((string)null));
            Assert.Throws(Is.InstanceOf(typeof(ArchiveException)), () => archiveUnderTest.ContainsDirectory((IArchiveFolder)null));
        }

        [Test]
        public void ContainsFile(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor);

            IArchiveFolder rootFolder = archiveExtractor.ArchiveExtractorObject.ReadFolderStructure();

            CheckExistingFilesInFolder(archiveUnderTest, rootFolder);
            Assert.IsFalse(archiveUnderTest.ContainsFile("thisfilecouldnt/possibly/exist.nope"));

            Assert.Throws(Is.InstanceOf(typeof (ArchiveException)),
                          () => archiveUnderTest.ContainsFile((string) null));
            Assert.Throws(Is.InstanceOf(typeof (ArchiveException)),
                          () => archiveUnderTest.ContainsFile((IArchiveFile) null));
        }

        [Test]
        public void GetMatchingFilesInDirectoryRecursive(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor);

            IArchiveFolder rootFolder = archiveExtractor.ArchiveExtractorObject.ReadFolderStructure();

            IEnumerable<IArchiveFile> allFiles = Flatten(rootFolder).OfType<IArchiveFile>();
            IEnumerable<IArchiveFile> matches = 
                archiveUnderTest.GetMatchingFilesInDirectoryRecursive(rootFolder, (_) => true);
            
            ValidateMatch(allFiles, matches);

            Assert.Throws(Is.InstanceOf(typeof(ArchiveException)), () => archiveUnderTest.GetMatchingFilesInDirectoryRecursive(null, arf => true).ToList());
            Assert.Throws(Is.InstanceOf(typeof(ArchiveException)), () => archiveUnderTest.GetMatchingFilesInDirectoryRecursive(rootFolder, null).ToList());
            Assert.Throws(Is.InstanceOf(typeof(ArchiveException)), () => archiveUnderTest.GetMatchingFilesInDirectoryRecursive(null, null).ToList());
        }

        [Test]
        public void GetMatchingFilesInDirectory(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor);

            IArchiveFolder rootFolder = archiveExtractor.ArchiveExtractorObject.ReadFolderStructure();

            IEnumerable<IArchiveFile> allFilesInRoot = rootFolder.Files;
            IEnumerable<IArchiveFile> matches =
                archiveUnderTest.GetMatchingFilesInDirectory(rootFolder, (_) => true);

            ValidateMatch(allFilesInRoot, matches);

            Assert.Throws(Is.InstanceOf(typeof(ArchiveException)), () => archiveUnderTest.GetMatchingFilesInDirectory(null, arf => true).ToList());
            Assert.Throws(Is.InstanceOf(typeof(ArchiveException)), () => archiveUnderTest.GetMatchingFilesInDirectory(rootFolder, null).ToList());
            Assert.Throws(Is.InstanceOf(typeof(ArchiveException)), () => archiveUnderTest.GetMatchingFilesInDirectory(null, null).ToList());
        }

        private void ValidateMatch(IEnumerable<IArchiveItem> one, IEnumerable<IArchiveItem> two)
        {
            IArchiveItem[] oneArray = one.ToArray();
            IArchiveItem[] twoArray = two.ToArray();

            Assert.AreEqual(oneArray.Length, twoArray.Length);

            for (int i = 0; i < oneArray.Length; i++)
            {
                Assert.IsTrue(IsMatch(oneArray[i], twoArray[i]));
            }
        }

        private bool IsMatch(IArchiveItem one, IArchiveItem two)
        {
            bool equal = true;

            //Assert.AreEqual(one.FullPath, two.FullPath);
            equal &= one.FullPath == two.FullPath;

            IIndexedArchiveItem<int> intIndexedOne = one as IIndexedArchiveItem<int>;
            IIndexedArchiveItem<int> intIndexedTwo = two as IIndexedArchiveItem<int>;

            if (intIndexedOne != null && intIndexedTwo != null)
            {
                //Assert.AreEqual(intIndexedOne.Index, intIndexedTwo.Index);
                equal &= intIndexedOne.Index == intIndexedTwo.Index;
            }

            IIndexedArchiveItem<string> strIndexedOne = one as IIndexedArchiveItem<string>;
            IIndexedArchiveItem<string> strIndexedTwo = two as IIndexedArchiveItem<string>;

            if(strIndexedOne != null && strIndexedTwo != null)
            {
                //Assert.AreEqual(strIndexedOne.Index, strIndexedTwo.Index);
                equal &= strIndexedOne.Index == strIndexedTwo.Index;
            }

            return equal;
        }

        [Test]
        public void GetFileContentsAsBytes(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor);

            IArchiveFolder rootFolder = archiveExtractor.ArchiveAttributes.DirectoryStructure;
            
            MemoryStream stream = new MemoryStream();
            archiveExtractor.ArchiveExtractorObject.ExtractFileToStream(rootFolder.Files.First(), stream);

            byte[] archiveBytes = archiveUnderTest.GetFileContentsAsBytes(rootFolder.Files.First());
            Assert.AreEqual(stream.ToArray(), archiveBytes);
        }

        [Test]
        public void DeleteFile(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor, GetMockCompressor());

            Folder arfldRoot = archiveExtractor.ArchiveAttributes.DirectoryStructure as Folder;
            IArchiveFile arflFile = arfldRoot.Files.First();

            //archiveExtractor.Setup(ae => ae.ReadFolderStructure()).Returns(
            //    ReflectDelete(arfldRoot, arflFile));
            archiveUnderTest.DeleteFile(arflFile);
            Assert.IsFalse(archiveUnderTest.ContainsFile(arflFile));
            Assert.IsTrue(archiveUnderTest.ContainsFile(arfldRoot.Files.Skip(1).First()));
        }

        [Test]
        public void DeleteFiles(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor, GetMockCompressor());

            Folder arfldRoot = archiveExtractor.ArchiveAttributes.DirectoryStructure as Folder;
            IEnumerable<IArchiveFile> arflFiles = arfldRoot.Files;
            //archiveExtractor.Setup(ae => ae.ReadFolderStructure()).Returns(
            //    ReflectDelete(arfldRoot, arflFiles));
            archiveUnderTest.DeleteFiles(arflFiles);
            Assert.IsFalse(archiveUnderTest.ContainsFile(arflFiles.First()));
        }

        private Folder ReflectDelete(Folder p_arfRootFolder, IArchiveFile p_arfDeletedFile)
        {
            return ReflectDelete(p_arfRootFolder, new IArchiveFile[] {p_arfDeletedFile});
        }

        private Folder ReflectDelete(Folder p_arfRootFolder, IEnumerable<IArchiveFile> p_earfDeletedFiles)
        {
            var subItems = new List<ITestArchiveItem>();

            foreach (var file in p_arfRootFolder.Files.OfType<ITestArchiveItem>())
            {
                if (p_earfDeletedFiles.Any(df => IsMatch(file, df))) continue;
                subItems.Add(file);
            }

            foreach (var folder in p_arfRootFolder.SubFolders.OfType<Folder>())
            {
                subItems.Add(ReflectDelete(folder, p_earfDeletedFiles));
            }

            return new Folder(p_arfRootFolder.Name,
                              subItems.ToArray());

        }

        [Test]
        public void AddFile(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor, GetMockCompressor());

            Folder arfldRoot = archiveExtractor.ArchiveAttributes.DirectoryStructure as Folder;
            IEnumerable<IArchiveFile> arfFiles
                = new IArchiveFile[]
                      {
                          new Mocks.File("file.txt"),
                          new Mocks.File("test.txt"),
                          new Mocks.File("noextension2"),
                          new Mocks.File("folder/otherfile.bat")
                      }; 
            //archiveExtractor.Setup(ae => ae.ReadFolderStructure()).Returns(
            //    ReflectAdd(arfldRoot, arfFiles));
            foreach (var file in arfFiles)
            {
                archiveUnderTest.AddFile(file.FullPath, string.Empty);
                Assert.IsTrue(archiveUnderTest.ContainsFile(file.FullPath));
            }
        }

        private Folder ReflectAdd(Folder p_arfRootFolder, IArchiveFile p_arfAddedFile)
        {
            return ReflectAdd(p_arfRootFolder, new IArchiveFile[] {p_arfAddedFile});
        }

        private Folder ReflectAdd(Folder p_arfRootFolder, IEnumerable<IArchiveFile> p_earfAddedFiles)
        {
            var subItems = new List<ITestArchiveItem>();

            if (p_earfAddedFiles.Any(file => BelongsInDirectory(p_arfRootFolder, file)))
            {
                subItems.AddRange(p_earfAddedFiles.Where(file=>BelongsInDirectory(p_arfRootFolder, file)).Select(file=>file as ITestArchiveItem));
            }

            foreach (var file in p_arfRootFolder.Files.OfType<ITestArchiveItem>())
            {
                subItems.Add(file);
            }

            foreach (var folder in p_arfRootFolder.SubFolders.OfType<Folder>())
            {
                subItems.Add(ReflectAdd(folder, p_earfAddedFiles));
            }

            return new Folder(p_arfRootFolder.Name,
                              subItems.ToArray());
        }

        private bool BelongsInDirectory(IArchiveFolder p_arfFolder, IArchiveFile p_arfFile)
        {
            string p_strFilePath = p_arfFile.FullPath;
            if (!p_strFilePath.StartsWith(p_arfFolder.FullPath)) return false;

            if (!string.IsNullOrEmpty(p_arfFolder.FullPath))
            {
                p_strFilePath = p_strFilePath.Replace(p_arfFolder.FullPath, string.Empty)
                    .Trim(Path.DirectorySeparatorChar)
                    .Trim(Path.AltDirectorySeparatorChar);
            }

            //self
            if (string.IsNullOrEmpty(p_strFilePath)) return false;

            return !p_strFilePath.Contains(Path.DirectorySeparatorChar)
                   && !p_strFilePath.Contains(Path.AltDirectorySeparatorChar);
        }

        [Test]
        public void GetSubDirectories(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor);

            IArchiveFolder rootFolder = archiveExtractor.ArchiveAttributes.DirectoryStructure;

            IEnumerable<IArchiveFolder> allFoldersInRoot = rootFolder.SubFolders;
            IEnumerable<IArchiveFolder> matches =
                archiveUnderTest.GetSubDirectories(rootFolder);

            ValidateMatch(allFoldersInRoot, matches);
        }

        [Test]
        public void GetSubDirectoriesRecursive(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchive archiveUnderTest = ConstructArchive(archiveExtractor);

            IArchiveFolder rootFolder = archiveExtractor.ArchiveAttributes.DirectoryStructure;

            IEnumerable<IArchiveFolder> allFolders = Flatten(rootFolder).OfType<IArchiveFolder>();
            IEnumerable<IArchiveFolder> matches =
                archiveUnderTest.GetSubDirectoriesRecursive(rootFolder);

            ValidateMatch(allFolders, matches);
        }

        private void CheckExistingFilesInFolder(IArchive archiveUnderTest, IArchiveFolder folder)
        {
            foreach (var file in folder.Files)
            {
                CheckFile(archiveUnderTest, file);
            }
            foreach (var subfolder in folder.SubFolders)
            {
                Assert.IsFalse(archiveUnderTest.ContainsFile(folder.FullPath));
                CheckExistingFilesInFolder(archiveUnderTest, subfolder);
            }
        }

        private void CheckExistingDirectoriesInFolder(IArchive archiveUnderTest, IArchiveFolder folder)
        {
            foreach (var file in folder.Files)
            {
                Assert.IsFalse(archiveUnderTest.ContainsDirectory(file.FullPath));
            }

            foreach (var subfolder in folder.SubFolders)
            {
                CheckDirectory(archiveUnderTest, subfolder);
                CheckExistingDirectoriesInFolder(archiveUnderTest, subfolder);
            }
        }

        private void CheckFile(IArchive archiveUnderTest, IArchiveFile file)
        {
            Assert.IsTrue(archiveUnderTest.ContainsFile(file));
            Assert.IsTrue(archiveUnderTest.ContainsFile(file.FullPath));
            Assert.IsTrue(archiveUnderTest.ContainsFile(MixPathSeperators(file.FullPath)));
        }

        private void CheckDirectory(IArchive archiveUnderTest, IArchiveFolder folder)
        {
            Assert.IsTrue(archiveUnderTest.ContainsDirectory(folder));
            Assert.IsTrue(archiveUnderTest.ContainsDirectory(folder.FullPath));
            Assert.IsTrue(archiveUnderTest.ContainsDirectory(MixPathSeperators(folder.FullPath)));
        }

        private string MixPathSeperators(string path)
        {
            string mangledPath = path;

            Random rand = new Random();
            int seperatorIndex = mangledPath.IndexOf(Path.DirectorySeparatorChar);
            while (seperatorIndex != -1)
            {
                if (rand.Next() % 2 == 0)
                {
                    mangledPath = path.Remove(seperatorIndex, 1).Insert(seperatorIndex, Path.AltDirectorySeparatorChar.ToString());
                }
                seperatorIndex = mangledPath.IndexOf(Path.DirectorySeparatorChar, seperatorIndex + 1);
            }

            return mangledPath;
        }

        [Test]
        public void GetFilesInDirectory(
            [ValueSource(typeof(ArchiveExtractorFactory), "AllExtractors")] ArchiveExtractorFactory.ArchiveExtractorTestContext archiveExtractor)
        {
            IArchiveFile[] emptyList = new IArchiveFile[] { };

            IArchive archiveUnderTest = ConstructArchive(archiveExtractor);

            IArchiveFolder emptyFolder = GetMockFolderFromPath("emptyfolder");
            IArchiveFolder fullFolder = GetMockFolderFromPath("folder");

            IEnumerable<IArchiveFile> emptyFolderTest = archiveUnderTest.GetFilesInDirectory(emptyFolder);
            Assert.AreEqual(emptyList, emptyFolderTest);

            IEnumerable<IArchiveFile> fullFolderTest = archiveUnderTest.GetFilesInDirectory(fullFolder);
            Assert.AreNotEqual(emptyList, fullFolderTest);
        }

        private IArchiveFolder GetMockFolderFromPath(string p_strFolderPath)
        {
            DirectoryInfo diDirInfo = new DirectoryInfo(p_strFolderPath);

            var mock = new Mock<IArchiveFolder>(MockBehavior.Strict);
            mock.Setup(folder => folder.FullPath).Returns(p_strFolderPath);
            mock.Setup(folder => folder.Name).Returns(diDirInfo.Name);
            mock.Setup(folder => folder.Files).Returns(new IArchiveFile[] {});
            mock.Setup(folder => folder.SubFolders).Returns(new IArchiveFolder[] {});

            return mock.Object;
        }
        
        private IArchiveCompressor GetMockCompressor()
        {
            var mock = new Mock<IArchiveCompressor>(MockBehavior.Strict);
            mock.Setup(ac => ac.AddFile(It.IsAny<string>(), It.IsAny<byte[]>()));
            mock.Setup(ac => ac.AddFiles(It.IsAny<IDictionary<string, byte[]>>()));
            mock.Setup(ac => ac.ModifyFile(It.IsAny<IArchiveFile>(), It.IsAny<byte[]>()));
            mock.Setup(ac => ac.ModifyFiles(It.IsAny<IDictionary<IArchiveFile, byte[]>>()));
            mock.Setup(ac => ac.DeleteFile(It.IsAny<IArchiveFile>()));
            mock.Setup(ac => ac.DeleteFiles(It.IsAny<IEnumerable<IArchiveFile>>()));

            return mock.Object;
        }
        
        private IEnumerable<IArchiveItem> Flatten(IArchiveFolder folder)
        {
            yield return folder;

            foreach (var file in folder.Files)
            {
                yield return file;
            }

            foreach (var subFolder in folder.SubFolders)
            {
                foreach (var item in Flatten(subFolder))
                {
                    yield return item;
                }
            }
        }

        private static IArchiveCompressor m_arcUselessCompressor;

        private static IArchiveCompressor StrictlyUselessMockCompressor
        {
            get
            {
                return m_arcUselessCompressor ??
                       (m_arcUselessCompressor = new Mock<IArchiveCompressor>(MockBehavior.Strict).Object);
            }
        }
    }
}
