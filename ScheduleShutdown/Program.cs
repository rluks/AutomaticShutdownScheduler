using Microsoft.Win32.TaskScheduler;
using System.Configuration;
using System;

namespace ScheduleShutdown
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime shutdownBoundary;

            try
            {
                int hour = Convert.ToInt32(ConfigurationManager.AppSettings["hour"]);
                int minute = Convert.ToInt32(ConfigurationManager.AppSettings["minute"]);
                int second = Convert.ToInt32(ConfigurationManager.AppSettings["second"]);
                shutdownBoundary = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, hour, minute, second);
            }
            catch (Exception)
            {
                //throw new Exception("Invalid shutdown time arguments!");
                Console.WriteLine("Invalid shutdown time arguments! 0-23 hours, 0-59 minutes/seconds");
                Console.ReadKey();
                return;
            }

            // Get the service on the local machine
            using (TaskService ts = new TaskService())
            {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Automatic shutdown every day to ensure going to bed at reasonable time.";

                // Create a trigger that will fire the task at this time every other day
                var MyTrigger = new DailyTrigger { DaysInterval = 1 };
                MyTrigger.StartBoundary = shutdownBoundary;
                td.Triggers.Add(MyTrigger);

                // Create an action that will launch Notepad whenever the trigger fires
                td.Actions.Add(new ExecAction("shutdown.exe", "/s /f /t 90 /c \"Automatic shutdown in 90s\"", null));

                // Register the task in the root folder
                ts.RootFolder.RegisterTaskDefinition(@"Automatic shutdown", td);

                // Remove the task?
                bool removeShutdown;
                try
                {
                    removeShutdown = Convert.ToBoolean(ConfigurationManager.AppSettings["removeShutdown"]);
                }
                catch (Exception)
                {
                    //throw new Exception("Invalid boolean arguments! true or false");
                    Console.WriteLine("Invalid boolean arguments! true or false");
                    Console.ReadKey();
                    return;
                }

                if (removeShutdown)
                {
                    ts.RootFolder.DeleteTask(@"Automatic shutdown");
                    Console.WriteLine("Automatic shutdown removed.");
                }
                else
                {
                    Console.WriteLine("Automatic shutdown scheduled: ");
                    Console.WriteLine(shutdownBoundary);
                }

                Console.ReadKey();
            }
        }
    }
}
