﻿using FluentSharp.CassiniDev;
using FluentSharp.CoreLib;
using FluentSharp.NUnit;
using FluentSharp.Web35;
using NUnit.Framework;

namespace TeamMentor.UnitTests.Cassini
{
    [TestFixture]
    public class Test_TeamMentor_Static_Content : NUnitTests_Cassini_TeamMentor
    {
        //WorkFlows
        [Test] public void Check_That_Site_Is_Up()
        {                        
            apiCassini.url().GET().assert_Not_Null()
                                  .assert_Not_Contains("TeamMentor is current unavailable") 
                                  .assert_Contains("<html>","<body>","</body>","</html>");

            apiCassini.url().uri()                            .HEAD().assert_True();
            apiCassini.url().uri().append("default.htm")      .HEAD().assert_True();
            apiCassini.url().uri().append("default.htm").str().HEAD_StatusCode().assert_Http_OK();
    
            
        }

        [Test] public void Expected_Pages_Html()
        {
            checkFile("default.htm");            
            checkFile("Html_Pages/errorPage.html");
            checkFile("Html_Pages/GuidanceItemViewer/GuidanceItemViewer.html");
            checkFile("Html_Pages/Gui/Pages/login.html");            
        }

        [Test] public void Expected_Pages_Javascript()
        {
            checkFiles("javascript/IE_Fixes.js",
                       "javascript/TM_WS_Methods.js",                       
                       "javascript/TM/Settings.js",
                       "javascript/TM/Events.js", 
                       "javascript/TM.Gui/TM.Gui.Main.js",
                       "javascript/TM.Gui/AppliedFilters.js");
        }

        //Util methods

        public void checkFiles(params string[] files)
        {
            foreach(var file in files)
                checkFile(file);
        }
        public void checkFile(string file)
        {
            var file_Path = apiCassini.mapPath(file);
            var file_Url  = apiCassini.url    (file);

            file_Path.fileContents().assert_Not_Empty()
                                    .assert_Equal_To(file_Url.GET());
                    
        }
    }
}
