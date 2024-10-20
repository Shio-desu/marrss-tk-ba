﻿/**
* ----------------------------------------------------------------
* Nikolai Jonathan Reed 
*
* 
* Copyright (c) 2015, Nikolai Reed, 1manprojects.de
* All rights reserved.
*
* Licensed under
* Creative Commons Attribution NonCommercial (CC-BY-NC)
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MARRSS.Interface2;
using MARRSS.Definition;
using MARRSS.Global;

namespace MARRSS.Scheduler
{
    /**
    * \brief Example Scheduler
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
    class CollisionSearchScheduler : SchedulerInterface, SchedulerSolutionInterface
    {

        private ContactWindowsVector schedule; //!< ContactWindowsVector to add scheduled items
        private ContactWindowsVector set; //!< ContactWindowsVector to start with
        private Main f = null;
        //!CollisionSearchScheduler constructor.
        /*!
            Class constructor is neede to create the object
        */
        public CollisionSearchScheduler()
        {

        }

        //! Calculates a schedule from the defined problem
        /*!
            \pram ScheduleProblemInterface defined problem with contactwindows
            This Function will calculate the solution to the problem defined in
            Schedule Problem Interface
        */
        public void CalculateSchedule(ScheduleProblemInterface problem)
        {
            set = problem.getContactWindows();
            schedule = new ContactWindowsVector();
            set.sort(Structs.sortByField.TIME);
            while (!isComplete())
            {
                
                for (int i = 0; i < set.Count(); i++)
                {
                    bool collisionFound = false;
                    List<int> collisionSet = new List<int>();
                    collisionSet.Add(i);
                    for (int j = 1; j < set.Count()-1; j++)
                    {
                        collisionFound = checkCollision(set.getAt(i), set.getAt(j));
                        collisionSet.Add(j);
                    }
                    if (collisionFound)
                    {
                        set.getAt(i).setSheduled();
                        for (int k = 0; k < collisionSet.Count-1; k++)
                        {
                            set.getAt(collisionSet[k]).unShedule();
                        }
                    }
                    else
                    {
                        schedule.add(set.getAt(i));
                        set.deleteAt(i);
                        i--;
                    }
                }
                
            }
            //retrive all the contactwindows that need to be scheduled
            //ContactWindowsVector set = problem.getContactWindows();
            //Scheduler Magic until is Complete returns true
            //No Element of the ContactWindowsVector set should be deleted
            //To Schedule a item call set.getAt(index).setSheduled()
            //To Unschedule a item call set.getAt(index).unShedule()
        }

        //! Checks if a solution has been found
        /*!
            \return bool true if complete
            This function will tell the scheduler if a solutin has been found
            evaluation function
        */
        public bool isComplete()
        {
            return set.isEmpty();
        }

        //! returns the finisched Schedule
        /*!
            \return ContactWindowsVector solution
            This Function returns the finisched schedule as a ContactWindowsVector
        */
        public ContactWindowsVector getFinischedSchedule()
        {
            return schedule;
        }

        //! ToString method
        /*!
           \return string 
            returns the Name of the Schedule and used Settings as String
        */
        override public string ToString()
        {
            return "Collision Search Scheduler";
        }

        private bool checkCollision(ContactWindow a, ContactWindow b)
        {
            bool res = false;
            
            res = a.checkConflikt(b) && a.getStationName() == b.getStationName() && 
                a.getSheduledInfo() && b.getSheduledInfo();
            return res;
        }


    }
}
