namespace XcodeProjectEditor.Tests
{
    using NUnit.Framework;

    internal static class XcodeModLoaderTests
    {
        private const string FakeFilesPath = @"/fakepath/";

        private const string EmptyXcodeMod = @"{
}
";

        private const string XcodeModWithGroup = @"{
  ""group"": ""Test Group""
}
";

        private const string XcodeModWithEmptyLibs = @"{
  ""libs"": []
}
";

        private const string XcodeModWithOneLib = @"{
  ""libs"": [
    ""libsqlite3.dylib""
  ]
}
";

        private const string XcodeModWithOneWeakLib = @"{
  ""libs"": [
    ""libsqlite3.0.dylib:weak""
  ]
}
";

        private const string XcodeModWithTwoLibs = @"{
  ""libs"": [
    ""libz.dylib"",
    ""libsqlite3.dylib:weak""
  ]
}
";

        private const string XcodeModWithEmptyFrameworks = @"{
  ""frameworks"": []
}
";

        private const string XcodeModWithOneFramework = @"{
  ""frameworks"": [
    ""StoreKit.framework""
  ]
}
";

        private const string XcodeModWithThreeFrameworks = @"{
  ""frameworks"": [
    ""Security.framework"",
    ""Social.framework"",
    ""Accounts.framework""
  ]
}
";

        private const string XcodeModWithEmptyHeaders = @"{
  ""headerpaths"": []
}
";

        private const string XcodeModWithOneHeader = @"{
  ""headerpaths"": [
    ""iOS/GameCenter""
  ]
}
";

        [Test]
        public static void TestEmptyMod()
        {
            XcodeProjectMod modificator = XcodeModLoader.LoadFromJson(FakeFilesPath, EmptyXcodeMod);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(0));
            Assert.That(modificator.Frameworks, Has.Length.EqualTo(0));
            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }

        [Test]
        public static void TestModWithGroup()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithGroup);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.AreEqual(modificator.Group, "Test Group");
            Assert.That(modificator.Libraries, Has.Length.EqualTo(0));
            Assert.That(modificator.Frameworks, Has.Length.EqualTo(0));
            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }

        [Test]
        public static void TestModWithEmptyLibs()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithEmptyLibs);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(0));
            Assert.That(modificator.Frameworks, Has.Length.EqualTo(0));
            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }

        [Test]
        public static void TestModWithOneLib()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithOneLib);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(1));

            Assert.AreEqual(modificator.Libraries[0].FilePath, "libsqlite3.dylib");
            Assert.False(modificator.Libraries[0].IsWeak);

            Assert.That(modificator.Frameworks, Has.Length.EqualTo(0));
            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }

        [Test]
        public static void TestModWithOneWeakLib()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithOneWeakLib);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(1));

            Assert.AreEqual(modificator.Libraries[0].FilePath, "libsqlite3.0.dylib");
            Assert.True(modificator.Libraries[0].IsWeak);

            Assert.That(modificator.Frameworks, Has.Length.EqualTo(0));
            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }

        [Test]
        public static void TestModWithTwoLibs()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithTwoLibs);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(2));

            Assert.AreEqual(modificator.Libraries[0].FilePath, "libz.dylib");
            Assert.False(modificator.Libraries[0].IsWeak);

            Assert.AreEqual(modificator.Libraries[1].FilePath, "libsqlite3.dylib");
            Assert.True(modificator.Libraries[1].IsWeak);

            Assert.That(modificator.Frameworks, Has.Length.EqualTo(0));
            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }

        [Test]
        public static void TestModWithEmptyFrameworks()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithEmptyFrameworks);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(0));
            Assert.That(modificator.Frameworks, Has.Length.EqualTo(0));
            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }

        [Test]
        public static void TestModWithOneFramework()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithOneFramework);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(0));
            Assert.That(modificator.Frameworks, Has.Length.EqualTo(1));

            Assert.AreEqual(modificator.Frameworks[0], "StoreKit.framework");

            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }

        [Test]
        public static void TestModWithThreeFrameworks()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithThreeFrameworks);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(0));
            Assert.That(modificator.Frameworks, Has.Length.EqualTo(3));

            Assert.AreEqual(modificator.Frameworks[0], "Security.framework");
            Assert.AreEqual(modificator.Frameworks[1], "Social.framework");
            Assert.AreEqual(modificator.Frameworks[2], "Accounts.framework");

            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }


        [Test]
        public static void TestModWithEmptyHeaders()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithEmptyHeaders);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(0));
            Assert.That(modificator.Frameworks, Has.Length.EqualTo(0));
            Assert.That(modificator.Headers, Has.Length.EqualTo(0));
            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }

        [Test]
        public static void TestModWithOneHeader()
        {
            XcodeProjectMod modificator =
                XcodeModLoader.LoadFromJson(FakeFilesPath, XcodeModWithOneHeader);

            Assert.AreEqual(modificator.FilesPath, FakeFilesPath);
            Assert.IsEmpty(modificator.Group);
            Assert.That(modificator.Libraries, Has.Length.EqualTo(0));
            Assert.That(modificator.Frameworks, Has.Length.EqualTo(0));
            Assert.That(modificator.Headers, Has.Length.EqualTo(1));

            Assert.AreEqual(modificator.Headers[0], @"iOS/GameCenter");

            Assert.That(modificator.Files, Has.Length.EqualTo(0));
            Assert.That(modificator.Folders, Has.Length.EqualTo(0));
            Assert.That(modificator.Excludes, Has.Length.EqualTo(0));
            Assert.NotNull(modificator.BuildSettings);

            Assert.That(modificator.BuildSettings.OtherLinkerFlags, Has.Length.EqualTo(0));
            Assert.IsEmpty(modificator.BuildSettings.GccEnableCppExceptions);
            Assert.IsEmpty(modificator.BuildSettings.GccEnableObjcExceptions);
        }
    }
}