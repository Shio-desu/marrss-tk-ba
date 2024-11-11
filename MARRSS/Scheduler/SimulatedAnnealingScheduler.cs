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
using MARRSS.Performance;

namespace MARRSS.Scheduler
{
    /**
    * \brief Simulated Annealing
    *
    * This class defines the simulated annealing scheduler to find a solution to the problem.
    */
    class SimulatedAnnealingScheduler : SchedulerInterface, SchedulerSolutionInterface
    {

        const double DEFAULT_START_TEMP = 1000.0;
        const double DEFAULT_COOLDOWN = 0.05;
        const double DEFAULT_EPSILON = 0.001;
        const int DEFAULT_STEP_SIZE = 4;
        const int DEFAULT_COOLDOWNINTERVAL = 10;

        private ObjectiveFunctionInterface objective;
        private ContactWindowsVector result;
        private bool cancel = false;
        private double currentFitness = 0.0;
        private double oldFitness = 0.0;
        private double temperature = DEFAULT_START_TEMP;
        private double cooldown = DEFAULT_COOLDOWN;
        private double cooldownInterval = DEFAULT_COOLDOWNINTERVAL;
        private double epsilon = DEFAULT_EPSILON;
        private int stepSize = DEFAULT_STEP_SIZE;
        private Random rnd;

        private Main mainform = null;

        private int iterations = 0;
        private int maxNumberOfIteration = 1000;
        bool adaptiveMaxIterations = false;

        bool randomStart = false;

        public SimulatedAnnealingScheduler()
        {
            rnd = new Random();
        }

        //!SimulatedAnnealing constructor.
        public SimulatedAnnealingScheduler(bool randomizeOnStart, bool useAdaptiveMaxIterations = false, int setMaxIterations = 1000,
            double setCooldown = DEFAULT_COOLDOWN, double setStartTemperature = DEFAULT_START_TEMP, double setEpsilon = DEFAULT_EPSILON,
            int setStepSize = DEFAULT_STEP_SIZE, int setCooldownInterval = DEFAULT_COOLDOWNINTERVAL, int seed = -1)
        {
            randomStart = randomizeOnStart;
            adaptiveMaxIterations = useAdaptiveMaxIterations;
            maxNumberOfIteration = setMaxIterations;
            cooldown = setCooldown;
            cooldownInterval = setCooldownInterval;
            temperature = setStartTemperature;
            epsilon = setEpsilon;
            stepSize = setStepSize;
            if (seed == -1)
                rnd = new Random();
            else
                rnd = new Random(seed);
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

            int progressStart = (int)temperature;
            if (mainform != null)
                mainform.setProgressBar(progressStart);

            fillContacts(result);
            currentFitness = getFitness(result);

            ContactWindowsVector currentSolution = new ContactWindowsVector(result);
            double currentSolutionFitness = currentFitness;

            while (!isComplete())
            {
                ContactWindowsVector neighbor = GenerateNeighbor(currentSolution);
                double neighborFitness = getFitness(neighbor);

                double deltaFitness = neighborFitness - currentSolutionFitness;

                // check if neighbor is accepted as current solution
                if (AcceptNeighbor(deltaFitness))
                {
                    currentSolution = neighbor;
                    currentSolutionFitness = neighborFitness;
                }
              
                // check if currentSolution is the best solution
                if (currentFitness < currentSolutionFitness)
                {
                    result = currentSolution;
                    currentFitness = currentSolutionFitness;
                }

                if (mainform != null)
                    mainform.updateProgressBar(progressStart - (int)temperature);

                if (Properties.Settings.Default.global_MaxPerf == false)
                    System.Windows.Forms.Application.DoEvents();
            }
        }

        private bool AcceptNeighbor(double deltaFit)
        {
            // accept neighbor if better
            if (deltaFit > 0)
                return true;

            // accept neighbor with slight chance influenced by difference and temperature
            else {
                double r = rnd.NextDouble();
                if (r < Math.Exp(deltaFit / temperature))
                    return true;
            }
            // reject solution
            return false;
        }

        // generates one neighbor by making a defined number of changes
        private ContactWindowsVector GenerateNeighbor(ContactWindowsVector solution)
        {
            ContactWindowsVector neighbor = new ContactWindowsVector(solution);

            Random rndNeighbor = new Random();
            for (int i = 0; i < stepSize; i++)
            {
                List<ContactWindow> conflictList = new List<ContactWindow>();

                bool nothingChanged = true;
                List<int> checkedIndex = new List<int>();
                do
                {

                    // get a non-repeating index for the conflict where a random reschedule happens
                    int windowIndex = rndNeighbor.Next(0, neighbor.Count());
                    if (checkedIndex.Contains(windowIndex))
                    {
                        // breaks infinite cycle if nothing CAN be changed here (should realistically never happen, for that every single contact window should be scheduled?)
                        if (checkedIndex.Count >= solution.Count())
                            nothingChanged = false;
                        continue;
                    }

                    checkedIndex.Add(windowIndex);

                    if (neighbor.getAt(windowIndex).getSheduledInfo())
                    {
                        continue;
                    }

                    for (int j = 0; j < neighbor.Count(); j++)
                    {

                        if (!neighbor.getAt(windowIndex).checkConflict(neighbor.getAt(j)) || j == windowIndex)
                            continue;

                        if (neighbor.getAt(windowIndex).getStationName() != neighbor.getAt(j).getStationName() &&
                            neighbor.getAt(windowIndex).getSatName() != neighbor.getAt(j).getSatName())
                            continue;
                        // collision detected

                        conflictList.Add(neighbor.getAt(j));
                    }
                    if (conflictList.Count == 0)
                    {
                        continue;
                    }

                    // unschedules all conflicting windows, so we can safely schedule this one              
                    foreach (ContactWindow window in conflictList)
                    {
                        window.unShedule();
                    }

                    // changes the schedule randomly by unscheduling the already scheduled, and scheduling another window
                    neighbor.getAt(windowIndex).setSheduled();
                    nothingChanged = false;
                } while (nothingChanged);

            }

            // try to fill neighbor if some space has free'd up by randomly changing a collision
            fillContacts(neighbor);
            return neighbor;
        }

        private List<ContactWindowsVector> GetNeightbors(ContactWindowsVector solution)
        {
            List<ContactWindowsVector> neighbors = new List<ContactWindowsVector>();

            // Create all possible neighbors of the current solution by making one change in the schedule
            for (int i = 0; i < solution.Count(); i++)
            {
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

            // increases iteration / decreases temperature only every few steps without fitness increase
            if (currentFitness > oldFitness)
            {
                oldFitness = currentFitness;
                iterations = 0;
            }
            else
            {
                if (iterations % cooldownInterval == 0)
                    temperature -= temperature * cooldown;

                iterations++;
                //Console.WriteLine("iterations: " + iterations);
            }
            if (temperature < epsilon)
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
