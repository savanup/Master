﻿using System;
using FluentSharp.CoreLib.API;
using NUnit.Framework;
using FluentSharp.CoreLib;
using TeamMentor.CoreLib;
using TeamMentor.FileStorage;
using TeamMentor.FileStorage.XmlDatabase;
using urn.microsoft.guidanceexplorer;

namespace TeamMentor.UnitTests.TM_XmlDatabase
{
    [TestFixture]//[Assert_Admin]
    public class Test_Library_SaveToDisk : TM_XmlDatabase_FileStorage
    {        
                
        [TearDown]
        public void TearDown()
        {
            Assert.IsTrue     (tmFileStorage.path_XmlDatabase().dirExists());
            Assert.IsTrue     (tmFileStorage.path_XmlLibraries().dirExists());
            Assert.IsTrue     (tmFileStorage.WebRoot.dirExists());   
            
            //delete temp WebRoot
            //Files.deleteFolder(TM_Server.WebRoot, true);
            //Assert.IsFalse    (TM_Server.WebRoot.dirExists());

            //delete temp Library data            
            tmFileStorage.path_XmlDatabase().files(true).files_Attribute_ReadOnly_Remove();  // required due to locks on .git files
            Files.deleteFolder(tmFileStorage.path_XmlDatabase(),true);
            Assert.IsFalse    (tmFileStorage.path_XmlDatabase().dirExists());
            Assert.IsFalse    (tmFileStorage.path_XmlLibraries().dirExists());
        }

        [Test]//[Ignore("This fails in TeamCity, since the Temp folder is inside the Websites AppData folder")]
        public void TestUseOfTempFolders()
        {
            var databaseFolder      = tmFileStorage.path_XmlDatabase();
            var libraryFolder       = tmFileStorage.path_XmlLibraries();
            var appDomainFolder     = AppDomain.CurrentDomain.BaseDirectory;
            
            Assert.IsTrue(TM_Status.Current.TM_Database_Location_Using_AppData  , "in UnitTests, should be inside App_Data folder");
            Assert.IsTrue(libraryFolder.contains(appDomainFolder)               , "in UnitTests, libraryFolder should be inside appDomainFolder");
            Assert.IsTrue(databaseFolder.contains(appDomainFolder)              , "in UnitTests, databaseFolder should be inside appDomainFolder");
            
        }

        [Test][Assert_Editor]
        public void Test_xmlDB_LibraryPath()
        {
            UserGroup.Editor.assert();
            var newLibraryName1 = "Test_new_Library";
            var testLibrary1    = tmXmlDatabase.new_TmLibrary(newLibraryName1);

            var libraryPath_Null_GuidanceExplorer = tmXmlDatabase.xmlDB_Path_Library_XmlFile(null as guidanceExplorer);
            var libraryPath_Null_TM_Library       = tmXmlDatabase.xmlDB_Path_Library_XmlFile(null as TM_Library);
            var libraryPath_Null_EmptyGuid        = tmXmlDatabase.xmlDB_Path_Library_XmlFile(Guid.Empty);
            var libraryPath_TM_Library            = tmXmlDatabase.xmlDB_Path_Library_XmlFile(testLibrary1);
            var libraryPath_Library_Id            = tmXmlDatabase.xmlDB_Path_Library_XmlFile(testLibrary1.Id);
            var libraryPath_GuidanceExplorer      = tmXmlDatabase.xmlDB_Path_Library_XmlFile(testLibrary1.guidanceExplorer(tmXmlDatabase));            

            Assert.IsNull(libraryPath_Null_GuidanceExplorer , "libraryPath_NullValue");
            Assert.IsNull(libraryPath_Null_TM_Library       , "libraryPath_NullValue");
            Assert.IsNull(libraryPath_Null_EmptyGuid        , "libraryPath_Null_EmptyGuid");
            
            Assert.NotNull(libraryPath_TM_Library           , "libraryPath_TM_Library");
            Assert.NotNull(libraryPath_Library_Id           , "libraryPath_Library_Id");
            Assert.NotNull(libraryPath_GuidanceExplorer     , "libraryPath_GuidanceExplorer");

            Assert.IsTrue(libraryPath_TM_Library.valid() && libraryPath_Library_Id.valid() && libraryPath_GuidanceExplorer.valid());
            Assert.AreEqual(libraryPath_TM_Library, libraryPath_Library_Id);
            Assert.AreEqual(libraryPath_TM_Library, libraryPath_GuidanceExplorer);
        }


        [Test][Assert_Editor]
        public void CreateLibrary_OnDisk()
        {
            UserGroup.Editor.assert();
            var newLibraryName1  = "Test_new_Library";
            var newLibraryName2  = "C++";

            var testLibrary1      = tmXmlDatabase.new_TmLibrary     (newLibraryName1);
            var libraryPath1      = tmXmlDatabase.xmlDB_Path_Library_XmlFile (testLibrary1);
            var testLibrary2      = tmXmlDatabase.new_TmLibrary     (newLibraryName2);
            var libraryPath2      = tmXmlDatabase.xmlDB_Path_Library_XmlFile (testLibrary2);

            Assert.IsTrue(libraryPath1.fileExists());
            Assert.IsTrue(libraryPath2.fileExists());
        }

        [Test][Assert_Admin]
        public void CreateArticle_OnLibraryWithDiferentNameThanFolder()
        {
            UserGroup.Admin.assert();
            var libraryId                   = Guid.NewGuid();
            var libraryName                 = "library_Name".add_RandomLetters(4);
            var libraryFolderAndXmlFile     = "FolderXml_Name".add_RandomLetters(4);
            var newGuidanceExplorer         = new guidanceExplorer { library = { name = libraryId.str(), caption = libraryName } };
            var newGuidanceExplorerXmlFile  = tmXmlDatabase.path_XmlLibraries().pathCombine(@"{0}\{0}.xml".format(libraryFolderAndXmlFile));

            //manually add the new newGuidanceExplorer to the database
            tmXmlDatabase.GuidanceExplorers_XmlFormat.add(libraryId, newGuidanceExplorer);
            //manually add the new guidanceExplorer Path
            tmXmlDatabase.guidanceExplorers_Paths().add(newGuidanceExplorer, newGuidanceExplorerXmlFile);
            //save guidanceExplorer (which should save in the new path)
            tmXmlDatabase.xmlDB_Save_GuidanceExplorer(libraryId);

            //get new values
            var libraryXmlFile = tmXmlDatabase.xmlDB_Path_Library_XmlFile      (libraryId);
            var libraryRootFolder = tmXmlDatabase.xmlDB_Path_Library_RootFolder(newGuidanceExplorer);

            Assert.IsTrue     (newGuidanceExplorerXmlFile.fileExists(), "newGuidanceExplorerXmlFile.fileExists()");
            Assert.IsTrue     (libraryRootFolder.dirExists()          , "libraryRootFolder.dirExists()");
            Assert.AreEqual   (libraryXmlFile, newGuidanceExplorerXmlFile, "libraryXmlFile and libraryFolderAndXmlFile");
            Assert.AreNotEqual(libraryXmlFile.fileName_WithoutExtension(), libraryName);

            //Now that we have confirmed that the library Name is different from the folder name, we can add an article
            //which was not working ok in 3.3. ( https://github.com/TeamMentor/Master/issues/482 )
            var newArticle         = tmXmlDatabase.xmlDB_RandomGuidanceItem(libraryId);            
            var articlesInLibrary  = tmXmlDatabase.getGuidanceItems_from_LibraryFolderOrView(libraryId);
            var articlePath        = tmXmlDatabase.xmlDB_guidanceItemPath(newArticle.Metadata.Id);
            var articlePath_Manual = libraryRootFolder.pathCombine(TMConsts.DEFAULT_ARTICLE_FOLDER_NAME).pathCombine("{0}.xml".format(newArticle.Metadata.Id));

            Assert.NotNull  (newArticle                  , "newArticle was null");
            Assert.AreEqual (1, articlesInLibrary.size() , "There should be one article in this Library");
            Assert.IsTrue   (articlePath.fileExists()    , "articlePath.fileExists()");
            Assert.IsTrue   (articlePath_Manual.fileExists(), "articlePath_Manual.fileExists()");
            Assert.AreEqual (articlePath, articlePath_Manual, "articlePath vs articlePath_Manual");

            articlePath.info();
            articlePath_Manual.info();
        }

        [Test]
        public void CreateLibrary_DirectlyOnDatabase_CheckExpectedPaths()
        {
            var libraryId   = Guid.NewGuid();
            var libraryName =  "test_Library".add_RandomLetters(4);
            var newGuidanceExplorer = new guidanceExplorer {library = {name = libraryId.str(), caption = libraryName}};
            
            //manually add the new newGuidanceExplorer to the database
            tmXmlDatabase.GuidanceExplorers_XmlFormat.add(libraryId, newGuidanceExplorer);
            newGuidanceExplorer.xmlDB_Save_GuidanceExplorer(tmXmlDatabase);

            var tmLibrary = tmXmlDatabase.tmLibrary(libraryId);

            Assert.IsNotNull(tmLibrary, "tmLibrary was null for libraryId: {0}".format(libraryId));
            Assert.AreEqual(tmLibrary.Id, libraryId, "tmLibrary.Id");
            Assert.AreEqual(tmLibrary.Caption, libraryName, "tmLibrary.Caption");

            var libraryXml_Via_LibraryId        = tmXmlDatabase.xmlDB_Path_Library_XmlFile(libraryId);
            var libraryXml_Via_GuidanceExplorer = tmXmlDatabase.xmlDB_Path_Library_XmlFile(newGuidanceExplorer);
            var libraryRootFolder               = tmXmlDatabase.xmlDB_Path_Library_RootFolder(newGuidanceExplorer);

            Assert.IsTrue(libraryXml_Via_LibraryId.valid(), "libraryXml_Via_LibraryId");
            Assert.AreEqual(libraryXml_Via_LibraryId, libraryXml_Via_GuidanceExplorer, "libraryXml_Via_LibraryId and libraryXml_Via_GuidanceExplorer");

            Assert.AreEqual(libraryName, libraryXml_Via_LibraryId.fileName_WithoutExtension(),
                            libraryXml_Via_LibraryId.fileName_WithoutExtension());
            Assert.AreEqual(libraryName, libraryRootFolder.folderName(), libraryRootFolder.folderName());
        }
    }
}