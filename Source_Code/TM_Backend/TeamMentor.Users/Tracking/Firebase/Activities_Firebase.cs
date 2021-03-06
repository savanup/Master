﻿using System;
using FluentSharp.CoreLib;

namespace TeamMentor.CoreLib
{
    public class Activities_Firebase 
    {        
        public static Activities_Firebase Current = new Activities_Firebase();
        public static API_Firebase apiFirebase = new API_Firebase(TMConsts.FIREBASE_AREA_ACTIVITIES);
    	
        public UserActivity logActivity(UserActivity userActivity)
        {            
            if(TM_UserData.Current.firebase_Log_Activities())
            {
                var submitData = new API_Firebase.SubmitData(userActivity, API_Firebase.Submit_Type.ADD);
                apiFirebase.submit(submitData);
            }            
            return userActivity;
        }
    }
    public static class Activities_Firebase_ExtensionMethod
    {
        public static UserActivity firebase_Log(this UserActivity userActivity)
        {
            try
            {
                Activities_Firebase.Current.logActivity(userActivity);
            }
            catch(Exception ex)
            {
                ex.log("[Activities_Firebase] [firebase_Log] with activity: {0}",userActivity);
            }
            return userActivity;        
        }
    }
}