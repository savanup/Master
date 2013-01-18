using System.Collections.Generic;
using O2.DotNetWrappers.ExtensionMethods;

namespace TeamMentor.CoreLib
{	
	public partial class TM_Xml_Database 
	{
        public static int FORCED_MILLISEC_DELAY_ON_LOGIN_ACTION = 500;

		public static void loadTmUserObjects(string xmlDatabasePath)
		{
			if(xmlDatabasePath.dirExists().isFalse())
			{
				"[TM_Xml_Database_ExtensionMethods_Users_Persistance] in loadTmUserObjects, provided xmlDatabasePath didn't exist: {0}".error(xmlDatabasePath);
				return;
			}
			var tmUsersXmlFile = xmlDatabasePath.getTmUsersXmlFile();
			//var tmUsersPasswordsXmlFile = xmlDatabasePath.getTmUsersPasswordsXmlFile();
			if(tmUsersXmlFile.fileExists().isFalse())			
				new List<TMUser>().saveAs(tmUsersXmlFile);
			
			//if(tmUsersPasswordsXmlFile.fileExists().isFalse())			
			//	new O2.DotNetWrappers.DotNet.Items().saveAs(tmUsersPasswordsXmlFile);
				
			TM_Xml_Database.Current.TMUsers = tmUsersXmlFile.load<List<TMUser>>();	
			//TM_Xml_Database.Current.TMUsersPasswordHashes = tmUsersPasswordsXmlFile.load<O2.DotNetWrappers.DotNet.Items>();				
		}

		public static void loadAndCheckUserDatabase(string xmlDatabasePath)
		{
			loadTmUserObjects(xmlDatabasePath);
			//check for invalid users
			foreach (var tmUser in TM_Xml_Database.Current.TMUsers.toList())
				if (tmUser.UserID == 0)
				{
					"[loadAndCheckUserDatabase] there was an account with userId=0, so removing it".error();
					TM_Xml_Database.Current.TMUsers.remove(tmUser);
				}
			saveTmUserObjects(xmlDatabasePath);
		}

		//[PrincipalPermission(SecurityAction.Demand, Role = "ManageUsers")] 
		public static void saveTmUserObjects(string xmlDatabasePath)
		{
			if (TM_Xml_Database.Current.UsingFileStorage)
			{
				"in saveTmUserObjects".info();
				lock (xmlDatabasePath)
				{
					var tmUsersXmlFile = xmlDatabasePath.getTmUsersXmlFile();
					//var tmUsersPasswordsXmlFile = xmlDatabasePath.getTmUsersPasswordsXmlFile();
					TM_Xml_Database.Current.TMUsers.saveAs(tmUsersXmlFile);
					//TM_Xml_Database.Current.TMUsersPasswordHashes.saveAs(tmUsersPasswordsXmlFile);
				}
			}
		}			
	}
}