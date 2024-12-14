/**
* ----------------------------------------------------------------
* Nikolai Jonathan Reed 
*
* 
* Copyright (c) 2017, Nikolai Reed, 1manprojects.de
* All rights reserved.
*
* Licensed under
* Creative Commons Attribution NonCommercial (CC-BY-NC)
*/
using MARRSS.Interface2;
using MARRSS.Definition;
using System;
using System.Collections.Generic;
using MARRSS.Performance;

namespace MARRSS.Scheduler
{
    class HillClimberScheduler : SchedulerInterface, SchedulerSolutionInterface
    {

        private ObjectiveFunctionInterface objective;
        private ContactWindowsVector result;
        private bool cancel = false;
        private double currentFitness = 0.0;
        private double oldFitness = 0.0;

        private Main mainform = null;

        private int iterations = 0;
        private int maxNumberOfIteration = 1000;
        bool adaptiveMaxIterations = false;

        bool randomStart = false;


        public HillClimberScheduler()
        {

        }

        public HillClimberScheduler(bool randomizeOnStart, bool useAdaptiveMaxIterations = false, int setMaxIterations = 1000)
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
            objective = problem.getObjectiveFunction();
            result = problem.getContactWindows();

            if (adaptiveMaxIterations)
            {
                maxNumberOfIteration = result.Count() * 4;
            }

            // should always be correct to only search through the contactwindwos once without change. The iterations reset once a positive change was found, so if the local optimum was found and every possible change from this
            // point doesnt change, it stops
            maxNumberOfIteration = result.Count();

            if (randomStart)
            {
                result.randomize();
            }
            
            if (mainform != null)
                mainform.setProgressBar(maxNumberOfIteration);

            fillContacts(result);
            currentFitness = getFitness(result);
            while (!isComplete())
            {
                for (int i = 0; i < result.Count(); i++)
                {
                    iterations++;

                    ContactWindowsVector currenSolution = new ContactWindowsVector(result);
                    currenSolution.getAt(i).setSheduled();
                    for (int j = 0; j < result.Count(); j++)
                    {
                        if (i != j && result.getAt(i).checkConflict(result.getAt(j)))
                        {
                            if (result.getAt(i).getStationName() == result.getAt(j).getStationName() ||
                                result.getAt(i).getSatName() == result.getAt(j).getSatName())
                            {

                                currenSolution.getAt(j).unShedule();

                                //collision detected
                                //result.getAt(i).unShedule();
                                //result.getAt(j).setSheduled();

                                //double newFitness = getFitness(result);
                                //if (newFitness > currentFitness)
                                //{
                                //    currentFitness = newFitness;
                                //    break;
                                //}
                                //else
                                //{
                                //    result.getAt(i).setSheduled();
                                //    result.getAt(j).unShedule();
                                //}

                            }
                        }
                    }

                    double newFitness = getFitness(currenSolution);
                    if (newFitness > currentFitness)
                    {
                        result = new ContactWindowsVector(currenSolution);
                        currentFitness = newFitness;
                    }
                }
                if (Properties.Settings.Default.global_MaxPerf == false)
                    System.Windows.Forms.Application.DoEvents();
                if (mainform != null)
                    mainform.updateProgressBar(iterations);

                fillContacts(result);
                currentFitness = getFitness(result);
            }

        }

        //! Checks if a solution has been found
        /*!
            \return bool true if complete
            This function will tell the scheduler if a solutin has been found
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
                return true;
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
            return "Hill Climber Scheduler";
        }

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

        //! set the main Form wich should be updated
        /*!
            \Main form
            sets the Form wich holds the elements that display progress bar etc.
        */
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

    }
}
