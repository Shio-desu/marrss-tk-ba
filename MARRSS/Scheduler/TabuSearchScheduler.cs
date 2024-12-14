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
using System.Linq;

namespace MARRSS.Scheduler
{
    /**
    * \brief Tabu Search Scheduler
    *
    * This class defines the tabu search scheduler to find a solution to the problem.
    * This is done by finding the best neighbor (solutions with one change from the current solution) each iteration and go from there,
    * but allowing a step down in fitness and preventing going back to already visited solutions for a few steps
    */
    class TabuSearchScheduler : SchedulerInterface, SchedulerSolutionInterface
    {

        private ObjectiveFunctionInterface objective;
        private ContactWindowsVector result;
        private bool cancel = false;
        private double currentFitness = 0.0;
        private double oldFitness = 0.0;

        private Main mainform = null;

        private int iterations = 0;
        private int maxNumberOfIteration = 25;
        private int tabuListSize = 100;
        bool adaptiveMaxIterations = false;

        bool randomStart = false;

        public TabuSearchScheduler()
        {

        }

        //!TabuSearch constructor.
        public TabuSearchScheduler(bool randomizeOnStart, bool useAdaptiveMaxIterations = false, int setMaxIterations = 20)
        {
            randomStart = randomizeOnStart;
            adaptiveMaxIterations = useAdaptiveMaxIterations;
            maxNumberOfIteration = setMaxIterations;
        }

        //! get The Objective Funktion to solve the scheduling problem
        /*!
            \param ObjectiveFunction problem set to solve
        */
        public void setObjectiveFunktion(ObjectiveFunctionInterface objectiveFunction)
        {
            objective = objectiveFunction;
        }
        //! returns The Objective Funktion to solve the scheduling problem
        /*!
            \rreturn ObjectiveFunction problem set to solve
        */
        public ObjectiveFunctionInterface getObjectiveFunction()
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

            tabuListSize = result.Count() / 10;

            currentFitness = 0.0;

            if (randomStart)
            {
                result.randomize();
                fillContacts(result);
            }

            if (adaptiveMaxIterations)
            {
                //maxNumberOfIteration = result.Count() * 4;
                // adaptive number of iterations might not make sense here because one iteration is already including a comparions of all of the avaliable contactwindows
                // you could make an argument that more contact windows mean more iterations to get out of local optima
                // might make sense in that case to also increase tabuListSize 
            }
     
            if (mainform != null)
                mainform.setProgressBar(maxNumberOfIteration);

            fillContacts(result);
            currentFitness = getFitness(result);

            ContactWindowsVector currentSolution = new ContactWindowsVector(result);
            List<ContactWindowsVector> tabuList = new List<ContactWindowsVector>();

            while (!isComplete())
            {
                List<ContactWindowsVector> neighbors = GetNeightbors(currentSolution);
                ContactWindowsVector bestNeighbor = new ContactWindowsVector();
                double bestNeighborFitness = 0;

                iterations++;
                // finding the best neighbor
                foreach (ContactWindowsVector neighbor in neighbors)
                { 
                    double neighborFitness = getFitness(neighbor);
                    if (neighborFitness > bestNeighborFitness)
                    {
                        // if tabuList doesnt contain neighbor (searches the list for a schedule equaling (own implemented equals function) the neighbor)
                        if (!tabuList.Any(schedule => (schedule.Equals(neighbor))))
                        {
                            bestNeighbor = new ContactWindowsVector(neighbor);
                            bestNeighborFitness = neighborFitness;
                        }
                    }
                    
                }

                if (bestNeighbor.getNrOfScheduled() == 0)
                    // no non-tabu neighbor found
                    break;

                currentSolution = new ContactWindowsVector(bestNeighbor);
                tabuList.Add(bestNeighbor);

                if (tabuList.Count > tabuListSize)
                {
                    tabuList.RemoveAt(0);
                }

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

        private List<ContactWindowsVector> GetNeightbors(ContactWindowsVector solution)
        {
            List<ContactWindowsVector> neighbors = new List<ContactWindowsVector>();

            // Create all possible neighbors of the current solution by making one change in the schedule
            for (int i = 0; i < solution.Count(); i++)
            {
                // only changes unscheduled to scheduled, because that is the safer operation (we check every collision for this one, set them unscheduled und schedule this. Other way around could create collisions)
                if (solution.getAt(i).getSheduledInfo())
                    continue;

                ContactWindowsVector neighbor = new ContactWindowsVector(solution);
                for (int j = 0; j < solution.Count(); j++)
                {

                    if (!solution.getAt(j).getSheduledInfo())
                        continue;

                    if (!solution.getAt(i).checkConflict(solution.getAt(j)) && i != j)
                        continue;

                    if (solution.getAt(i).getStationName() != solution.getAt(j).getStationName() &&
                        solution.getAt(i).getSatName() != solution.getAt(j).getSatName())
                        continue;
                    // collision detected

                    // unscheduling every collision                 
                    neighbor.getAt(j).unShedule();

                }
                neighbor.getAt(i).setSheduled();
                neighbors.Add(neighbor);
            }

            return neighbors;
        }

        // schedules one contact for every window if there is no conflicting scheduled already (to fill the easy gaps and go from there)
        private void fillContacts(ContactWindowsVector contacts)
        {
            for (int i = 0; i < contacts.Count(); i++)
            {
                bool confilcts = false;
                if (!contacts.getAt(i).getSheduledInfo())
                {
                    for (int j = 0; j < contacts.Count(); j++)
                    {
                        if (contacts.getAt(j).getSheduledInfo() && i != j && contacts.getAt(i).checkConflict(contacts.getAt(j)))
                        {
                            if (contacts.getAt(i).getStationName() == contacts.getAt(j).getStationName() ||
                                contacts.getAt(i).getSatName() == contacts.getAt(j).getSatName())
                            {
                                confilcts = true;
                                break;
                            }
                        }
                    }
                }
                if (!confilcts)
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
