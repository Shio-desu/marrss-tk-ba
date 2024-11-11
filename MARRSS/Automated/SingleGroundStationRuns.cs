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
        private SchedulerInterface scheduler = null;
        private Main updateForm;


        public SingleGroundStationRuns(SchedulerInterface scheduler, ObjectiveFunctionInterface objective, EpochTime start, EpochTime stop, List<One_Sgp4.Tle> satellites,
            List<Ground.Station> stations, int selectedScenario, conflictResolutionOptions conflictResolution, Main updateForm = null)
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
            this.updateForm = updateForm;
        }

        public void runThisRun()
        {
            List<ContactWindowsVector> resultSchedules = new List<ContactWindowsVector>();

            if (updateForm != null)
                updateForm.setProgressBar(stations.Count + 1);
            if (Properties.Settings.Default.global_MaxPerf == false)
                System.Windows.Forms.Application.DoEvents();
            int iterations = 0;

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
                RunScheduler.setScheduler(scheduler);
                RunScheduler.startScheduler(scheduler, problem);
                resultSchedules.Add(scheduler.getFinischedSchedule());

                iterations++;
                if (updateForm != null)
                    updateForm.updateProgressBar(iterations);
                if (Properties.Settings.Default.global_MaxPerf == false)
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

                    foreach (ContactWindowsVector schedule in resultSchedules)
                    {
                        combined.add(schedule.getAllContacts());
                    }
                    combined.sort(Global.Structs.sortByField.TIME);
                    combined = fillContacts(GreedyConflictResolution(combined));
                    break;

                default:
                    break;
            }

            if (updateForm != null)
                updateForm.updateProgressBar(iterations + 1);

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

        private ContactWindowsVector GreedyConflictResolution(ContactWindowsVector contacts)
        {
            ContactWindowsVector solution = new ContactWindowsVector();
            solution.setStartTime(start);
            solution.setStopTime(stop);

            // goes over each contact from the sirrs solution, to check if we should add it to the final solution
            for (int i = 0; i < contacts.Count(); i++)
            {

                ContactWindow contactToCheck = contacts.getAt(i);

                // if it wasnt scheduled in the sirrs solution, skip over the checks, just add it so we might schedule it later if the collisions free themselves up
                if (!contactToCheck.getSheduledInfo())
                {
                    solution.add(contactToCheck);
                    continue;
                }
                    

                bool conflictFound = false;

                // goes over each contact already in the solution, if it would raise a collision
                for (int j = 0; j < solution.Count(); j++)
                {
                    if (solution.getAt(j).checkConflict(contactToCheck) && solution.getAt(j).getSheduledInfo())
                    {
                        if (solution.getAt(j).getSatName() == contactToCheck.getSatName()
                            || solution.getAt(j).getStationName() == contactToCheck.getStationName())
                        {

                            conflictFound = true;

                            // check if the contactToCheck would improve the current solution
                            objectiveFunction.calculateValues(solution);
                            double curFitness = objectiveFunction.getObjectiveResults();

                            solution.getAt(j).unShedule();
                            solution.add(contactToCheck);

                            objectiveFunction.calculateValues(solution);
                            double newFitness = objectiveFunction.getObjectiveResults();

                            // if the new contact would improve the fitness, revert the change
                            if (curFitness > newFitness)
                            {
                                solution.getAt(j).setSheduled();
                                solution.getLast().unShedule();
                            }
                        }
                    }
                }

                // if there was no conflict with this contact, add it
                if (!conflictFound)
                {
                    solution.add(contactToCheck);
                }
            }

            return solution;
        }

        // fill some free contacts
        private ContactWindowsVector fillContacts(ContactWindowsVector contacts)
        {
            ContactWindowsVector change = new ContactWindowsVector(contacts);

            for (int i = 0; i < change.Count(); i++)
            {
                bool conflicts = false;
                if (!change.getAt(i).getSheduledInfo())
                {
                    for (int j = 0; j < change.Count(); j++)
                    {
                        if (change.getAt(j).getSheduledInfo() && i != j && change.getAt(i).checkConflict(change.getAt(j)))
                        {
                            if (change.getAt(i).getStationName() == change.getAt(j).getStationName() ||
                                change.getAt(i).getSatName() == change.getAt(j).getSatName())
                            {
                                conflicts = true;
                                break;
                            }
                        }
                    }
                    if (!conflicts)
                    {
                        change.getAt(i).setSheduled();
                    }
                }
            }
            return change;
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
