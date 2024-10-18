/**
* ----------------------------------------------------------------
* Theo Kaminsky
*
* 
*
* 
*
* 
* 
*/
using MARRSS.Interface2;
using MARRSS.Definition;
using MARRSS.Global;
using System.Windows.Documents;
using System.Collections.Generic;
using System;

namespace MARRSS.Scheduler
{
    /**
    * \brief Tabu Search Scheduler
    *
    * This class is an Example of how to implement another Scheduler into
    * the software it has all the callbacks and funktions that need to be defined
    * To Add the Scheduler into the main Programm it has to be added into the
    * Main.cs Class under startScheduleButton_Click()
    * There it can replace either a existing schedule or complemente the others
    * To let the user be able to select whitch scheduler he wants to use a new 
    * Radio Button has to be added to the other ones in the Main form.
    * It has to be added into the same GroupBox as the others.
    * In the Main.cs file add 
    * if (nameOfTheRadioButtonForNewScheduler.Checked)
    */
    class TabuSearchScheduler : SchedulerInterface, SchedulerSolutionInterface
    {

        private ObjectiveFunction objective;
        private ContactWindowsVector result;
        private bool cancel = false;
        private double currentFitness = 0.0;
        private double oldFitness = 0.0;

        private Main mainform = null;

        private int iterations = 0;
        private int maxNumberOfIteration = 2000;
        private int tabuListSize = 100;
        bool adaptiveMaxIterations = false;

        bool randomStart = false;

        public TabuSearchScheduler()
        {

        }

        //!TabuSearch constructor.
        public TabuSearchScheduler(bool randomizeOnStart, bool useAdaptiveMaxIterations = false, int setMaxIterations = 1000)
        {
            randomStart = randomizeOnStart;
            adaptiveMaxIterations = useAdaptiveMaxIterations;
            maxNumberOfIteration = setMaxIterations;
        }

        //! get The Objective Funktion to solve the scheduling problem
        /*!
            \param ObjectiveFunction problem set to solve
        */
        public void setObjectiveFunktion(ObjectiveFunction objectiveFunction)
        {
            objective = objectiveFunction;
        }
        //! returns The Objective Funktion to solve the scheduling problem
        /*!
            \rreturn ObjectiveFunction problem set to solve
        */
        public ObjectiveFunction getObjectiveFunction()
        {
            return objective;
        }

        //! Calculates a schedule from the defined problem
        /*!
            \pram ScheduleProblemInterface defined problem with contactwindows
            This Function will calculate the solution to the problem defined in
            Schedule Problem Interface
        */
        public void CalculateSchedule(ScheduleProblemInterface problem)
        {
            //retrive all the contactwindows that need to be scheduled
            //ContactWindowsVector set = problem.getContactWindows();
            //Scheduler Magic until is Complete returns true
            //No Element of the ContactWindowsVector set should be deleted
            //To Schedule a item call set.getAt(index).setSheduled()
            //To Unschedule a item call set.getAt(index).unShedule()

            objective = problem.getObjectiveFunction();
            result = problem.getContactWindows();

            currentFitness = 0.0;

            if (randomStart)
            {
                result.randomize();
                fillContacts(result);
            }

            if (adaptiveMaxIterations)
            {
                maxNumberOfIteration = result.Count() * 4;
                
            }
     
            if (mainform != null)
                mainform.setProgressBar(maxNumberOfIteration);

            ContactWindowsVector currentSolution = new ContactWindowsVector(result);
            List<ContactWindowsVector> tabuList = new List<ContactWindowsVector>();

            currentFitness = getFitness(result);

            while (!isComplete())
            {
                List<ContactWindowsVector> neighbors = GetNeightbors(currentSolution);
                ContactWindowsVector bestNeighbor = new ContactWindowsVector();
                double bestNeighborFitness = 0;
                fillContacts(result);


                // finding the best neighbor
                foreach (ContactWindowsVector neighbor in neighbors)
                {
                    if (!tabuList.Contains(neighbor))
                    {
                        double neighborFitness = getFitness(neighbor);
                        if (neighborFitness > bestNeighborFitness)
                        {
                            bestNeighbor = new ContactWindowsVector(neighbor);
                            bestNeighborFitness = neighborFitness;
                        }
                    }
                }

                if (bestNeighbor.getNumberOfScheduledContacts() == 0)
                    // no non-tabu neighbor found
                    break;

                currentSolution = new ContactWindowsVector(bestNeighbor);
                tabuList.Add(bestNeighbor);

                if (tabuList.Count > tabuListSize)
                {
                    tabuList.RemoveAt(0);
                }

                Console.WriteLine($"best neighbor: {bestNeighborFitness} | currentFitness: {currentFitness} | actual fitness: {getFitness(result)}");
                if (bestNeighborFitness > currentFitness)
                {
                    
                    result = new ContactWindowsVector(bestNeighbor);
                    currentFitness = bestNeighborFitness;
                }

                if (Properties.Settings.Default.global_MaxPerf == false)
                    System.Windows.Forms.Application.DoEvents();

                if (mainform != null)
                    mainform.updateProgressBar(iterations);
            }
        }

        public List<ContactWindowsVector> GetNeightbors(ContactWindowsVector solution)
        {
            List<ContactWindowsVector> neighbors = new List<ContactWindowsVector>();

            // Create all possible neighbors of the current solution by making one change in the schedule
            for (int i = 0; i < solution.Count(); i++)
            {
                iterations++;
                for (int j = i + 1; j < solution.Count(); j++)
                {

                    if (!solution.getAt(i).checkConflict(solution.getAt(j)))
                        continue;

                    if (solution.getAt(i).getStationName() != solution.getAt(j).getStationName() &&
                        solution.getAt(i).getSatName() != solution.getAt(j).getSatName())
                        continue;
                    // collision detected

                    ContactWindowsVector neighbor = new ContactWindowsVector(solution);

                    // swapping the scheduled windows

                    if (solution.getAt(i).getSheduledInfo())
                    {
                        neighbor.getAt(i).unShedule();
                        neighbor.getAt(j).setSheduled();
                        neighbors.Add(neighbor);
                    }

                    if (solution.getAt(j).getSheduledInfo())
                    {
                        neighbor.getAt(i).setSheduled();
                        neighbor.getAt(j).unShedule();
                        neighbors.Add(neighbor);
                    }

                    // if none of the contactwindows are scheduled, nothing is happening because there has to be a third (or more) overlapping which should be scheduled and then swapped

                }
            }

            return neighbors;
        }

        // schedules one contact for every window if there is no conflicting scheduled already (to fill the easy gaps and go from there)
        private void fillContacts(ContactWindowsVector contacts)
        {
            for (int i = 0; i < contacts.Count(); i++)
            {
                bool conflicts = false;
                if (!contacts.getAt(i).getSheduledInfo())
                {
                    for (int j = 0; j < contacts.Count(); j++)
                    {
                        if (contacts.getAt(j).getSheduledInfo() && i != j && contacts.getAt(i).checkConflict(contacts.getAt(j)))
                        {
                            if (contacts.getAt(i).getStationName() == contacts.getAt(j).getStationName() ||
                                contacts.getAt(i).getSatName() == contacts.getAt(j).getSatName())
                            {
                                conflicts = true;
                                break;
                            }
                        }
                    }
                }
                if (!conflicts)
                {
                    contacts.getAt(i).setSheduled();
                }
            }
        }

        //! Checks if a solution has been found
        /*!
            \return bool true if complete
            This function will tell the scheduler if a solution has been found
            evaluation function
        */
        public bool isComplete()
        {
            if (cancel)
                return true;
            if (currentFitness > oldFitness)
            {
                oldFitness = currentFitness;
                iterations = 0;
            }
            else
            {
                //iterations++;
                //Console.WriteLine("iterations: " + iterations);
            }
            if (iterations > maxNumberOfIteration)
            {
                return true;
            }
            
            return false;
        }

        //! returns the finisched Schedule
        /*!
            \return ContactWindowsVector solution
            This Function returns the finisched schedule as a ContactWindowsVector
        */
        public ContactWindowsVector getFinischedSchedule()
        {
            return result;
        }

        //! cancel function
        /*!
            set internal value to halt/stop current calculation
        */
        public void cancelCalculation()
        {
            cancel = true;
        }

        //! ToString method
        /*!
           \return string 
            returns the Name of the Schedule and used Settings as String
        */
        override public string ToString()
        {
            return "Example Scheduler";
        }

        public void setFormToUpdate(Main form)
        {
            mainform = form;
        }

        public void setMaxNumberOfIterations(int val)
        {
            maxNumberOfIteration = val;
        }

        public void setRandomStart(bool val)
        {
            randomStart = val;
        }

        public void setAdaptiveMaxIterationbs(bool val)
        {
            adaptiveMaxIterations = val;
        }

        //! returns the fitness value of current set
        /*!
            /param Contact Windows Vector
            /return double fitnessValue
        */
        private double getFitness(ContactWindowsVector contacts)
        {
            objective.calculateValues(contacts);
            return objective.getObjectiveResults();
        }
    }
}
