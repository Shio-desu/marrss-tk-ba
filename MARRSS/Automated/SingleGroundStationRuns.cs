using System;
using System.Collections.Generic;

using MARRSS.Definition;
using MARRSS.Interface2;
using MARRSS.Scheduler;
using MARRSS.Performance;
using One_Sgp4;

namespace MARRSS.Automated
{
    class SingleGroundStationRuns
    {

        public enum conflictResolutionOptions
        {
            Nothing,
            Greedy
        }

        private int scenario;
        private ObjectiveFunctionInterface objectiveFunction;
        private List<Ground.Station> stations;
        private List<One_Sgp4.Tle> satellites;
        private ContactWindowsVector result;
        private EpochTime start;
        private EpochTime stop;
        private conflictResolutionOptions conflictResolution;
        SchedulerInterface scheduler = null;

        public SingleGroundStationRuns(SchedulerInterface scheduler, ObjectiveFunctionInterface objective, EpochTime start, EpochTime stop, List<One_Sgp4.Tle> satellites,
            List<Ground.Station> stations, int selectedScenario, conflictResolutionOptions conflictResolution)
        {
            this.scheduler = scheduler;
            scenario = selectedScenario;
            objectiveFunction = objective;
            scheduler = null;
            this.start = start;
            this.stop = stop;
            this.conflictResolution = conflictResolution;
            this.stations = stations;
            this.satellites = satellites;
        }

        public void runThisRun()
        {
            List<ContactWindowsVector> resultSchedules = new List<ContactWindowsVector>();

            System.Windows.Forms.Application.DoEvents();

            // runs a scheduler for each single station and adds the result to the list
            foreach (Ground.Station gs in stations)
            {
                ContactWindowsVector contacts = MainFunctions2.calculateContactWindows(satellites, new List<Ground.Station> {gs}, start, stop);
                
                SchedulingProblem problem = new SchedulingProblem();
                problem.setContactWindows(contacts);
                problem.removeUnwantedContacts(Properties.Settings.Default.orbit_Minimum_Contact_Duration_sec);
                problem.setObjectiveFunction(objectiveFunction);
                problem.getContactWindows().randomize(Properties.Settings.Default.global_Random_Seed);
                getScenario(problem, scenario);
                System.Windows.Forms.Application.DoEvents();
                TimeMeasurement tm = new TimeMeasurement();
                tm.activate();
                RunScheduler.setScheduler(scheduler);
                RunScheduler.startScheduler(scheduler, problem);
                string time = tm.getValueAndDeactivate();
                System.Windows.Forms.Application.DoEvents();
                resultSchedules.Add(scheduler.getFinischedSchedule());
                System.Windows.Forms.Application.DoEvents();

            }
           
            ContactWindowsVector combined = new ContactWindowsVector();
            combined.setStartTime(start);
            combined.setStopTime(stop);

            // add all the Single Resource schedules to one combined solution, dependent on the selected conflictResolution option
            switch (conflictResolution)
            {
                // add them all together and dont resolve the collisions
                case conflictResolutionOptions.Nothing:

                    foreach (ContactWindowsVector schedule in resultSchedules)
                    {
                        combined.add(schedule.getAllContacts());
                    }
                    //for (int i = 0; i < combined.Count(); i++)
                    //{
                    //    for (int k = 0; k < combined.Count(); k++)
                    //    {
                    //        if (i != k && combined.getAt(i).getSheduledInfo() &&
                    //            combined.getAt(k).getSheduledInfo() &&
                    //            combined.getAt(i).checkConflict(combined.getAt(k)))
                    //        {
                    //            if (combined.getAt(k).getSatName() == combined.getAt(i).getSatName()
                    //                || combined.getAt(k).getStationName() == combined.getAt(i).getStationName())
                    //            {
                    //                combined.getAt(k).unShedule();
                    //            }
                    //        }
                    //    }
                    //}

                    break;
                   
                case conflictResolutionOptions.Greedy:
                    break;

                default:
                    break;
            }

            result = new ContactWindowsVector(combined);
            objectiveFunction.calculateValues(result);
        }

        public ContactWindowsVector getResult()
        {
            return result;
        }

        //! get Selected Scenario
        /*! 
         * Generates the Scenario selected
        */
        private void getScenario(SchedulingProblem problem, int selectedScenario)
        {
            /*
                * Generate the selected Scenarios
                * These are defined in the SchedulingProblem Class
                * Other Scenarios can be selected here if they are added
                */
            if (selectedScenario == 0)
            {
                problem.GenerateSzenarioA();
            }
            if (selectedScenario == 1)
            {
                problem.GenerateSzenarioB(Properties.Settings.Default.global_Random_Seed);
            }
            if (selectedScenario == 2)
            {
                problem.GenerateSzenarioC(Properties.Settings.Default.global_Random_Seed);
            }
            if (selectedScenario == 3)
            {
                problem.GenerateSzenarioD(Properties.Settings.Default.global_Random_Seed);
            }
        }

        public ObjectiveFunctionInterface getObjectiveFunction()
        {
            return objectiveFunction;
        }

        public int getScenario()
        {
            return scenario;
        }

        public EpochTime getStartTime()
        {
            return result.getStartTime();
        }

        public EpochTime getStopTime()
        {
            return result.getStopTime();
        }

        public List<string> getSatellites()
        {
            return result.getSatelliteNames();
        }

        public List<string> getStation()
        {
            return result.getStationNames();
        }

    }
}
