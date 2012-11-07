using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Nexus.Client.Util;

namespace Util.Test.Archive.Mocks
{
    static class ArchiveExtractorFactory
    {
        public class ArchiveExtractorTestContext
        {
			public IArchiveExtractor ArchiveExtractorObject { get { return MockExtractor.Object; } }
            public ArchiveAttributes ArchiveAttributes { get; private set; }
			public Mock<IArchiveExtractor> MockExtractor { get; private set; }

            public ArchiveExtractorTestContext(Mock<IArchiveExtractor> extractor, ArchiveAttributes attributes)
            {
	            MockExtractor = extractor;
                ArchiveAttributes = attributes;
            }
        }

        public static IEnumerable<ArchiveExtractorTestContext> AllExtractors
        {
            get
            {
                foreach (var extractor in MockExtractors)
                {
                    yield return extractor;
                }
            }
        }

        public static IEnumerable<ArchiveExtractorTestContext> MockExtractors
        {
            get
            {
                foreach (var archiveAttributes in ArchiveAttributeFactory.ArchiveAttributes)
                {
                    yield return new ArchiveExtractorTestContext(
                        GetArchiveExtractorMock(archiveAttributes),
                        archiveAttributes);
                }
            }
        }

        private static Mock<IArchiveExtractor> GetArchiveExtractorMock(ArchiveAttributes p_aratAttributes)
        {
            var mock = new Mock<IArchiveExtractor>();
            mock.Setup(aexExtractor => aexExtractor.ReadFolderStructure()).Returns(p_aratAttributes.DirectoryStructure);
            mock.Setup(
                aexExtractor => aexExtractor.ExtractFileToStream(It.IsAny<IArchiveFile>(), It.IsAny<System.IO.Stream>()))
                .Callback(
                    delegate(IArchiveFile file, System.IO.Stream stream)
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(p_aratAttributes.ExtractedPath, file.FullPath));
                            stream.Write(fileBytes, 0, fileBytes.Length);
                        });
            return mock;
        }

        public static IArchiveExtractor GetPreExtractedFileReader(ArchiveAttributes p_aratAttributes)
        {
            Mock<IArchiveExtractor> mock = new Mock<IArchiveExtractor>();
            mock.Setup(
                aexExtractor => aexExtractor.ExtractFileToStream(It.IsAny<IArchiveFile>(), It.IsAny<System.IO.Stream>()))
                .Callback(
                    delegate(IArchiveFile file, System.IO.Stream stream)
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(p_aratAttributes.ExtractedPath, file.FullPath));
                        stream.Write(fileBytes, 0, fileBytes.Length);
                    });
            return mock.Object;
        }
    }
}
