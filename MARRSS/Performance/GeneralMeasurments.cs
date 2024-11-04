/**
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

using MARRSS.Scheduler;
using MARRSS.Definition;
using MARRSS.Interface1;
using MARRSS.Global;

namespace MARRSS.Performance
{
    /**
* \brief General Measurement Class
*
* This class calculates values to compare diffrent results to each other
*/
    class GeneralMeasurments
    {

        //! Calculate number of Conflicts in the schedule
        /*!
            \param ContactWindowVector schedule to calculate fairness
            \return int number of overall conflicting contact windows
        */
        public static int getNrOfConflicts(ContactWindowsVector contacts)
        {
            int nrOfConflicts = 0;

            // goes over each contactwindow and checks their colliding windows if they are scheduled as well
            for (int i = 0; i < contacts.Count(); i++)
            {
                ContactWindow window = contacts.getAt(i);
                if (!window.getSheduledInfo())
                    continue;

                bool conflictFound = false;
                List<ContactWindow> conflictList = window.getConflictWindows();
                for (int j = 0; j < conflictList.Count; j++)
                {
                    if (conflictList[j].getSheduledInfo())
                        conflictFound = true;
                }

                if (conflictFound)
                    nrOfConflicts++;
            }

            return nrOfConflicts;
        }

        public static double getDurationOfScheduledContacts(ContactWindowsVector contacts)
        {
            double duration = 0.0;
            for (int i = 0; i < contacts.Count(); i++)
            {
                if (contacts.getAt(i).getSheduledInfo())
                {
                    duration += contacts.getAt(i).getDuration();
                }
            }
            return duration;
        }

        //Test Function
        public static string getNrOfUweContacts(ContactWindowsVector contacts)
        {
            int count = 0;
            for (int i = 0; i < contacts.Count(); i++)
            {
                if (contacts.getAt(i).getSheduledInfo()
                    && contacts.getAt(i).getSatName() == "UWE-3")
                {
                    count++;
                }
            }
            return "UWE-3: " + count;
        }

        //! returns the number of contacts for each priority
        /*!
            \param ContactWindowVector schedule to calculate fairness
            \return string containing number of contacts schedule for each priorty
        */
        public static string getNrOfPrioritysScheduled(ContactWindowsVector contacts)
        {
            int p0 = 0;
            int p1 = 0;
            int p2 = 0;
            int p3 = 0;
            int p4 = 0;
            int sp0 = 0;
            int sp1 = 0;
            int sp2 = 0;
            int sp3 = 0;
            int sp4 = 0;
            for (int i = 0; i < contacts.Count(); i++)
            {
                int p = (int)contacts.getAt(i).getPriority();
                switch (p)
                {
                    case 0:
                        p0++;
                        break;
                    case 1:
                        p1++;
                        break;
                    case 2:
                        p2++;
                        break;
                    case 3:
                        p3++;
                        break;
                    case 4:
                        p4++;
                        break;
                }
                if (contacts.getAt(i).getSheduledInfo())
                {
                    switch (p)
                    {
                        case 0:
                            sp0++;
                            break;
                        case 1:
                            sp1++;
                            break;
                        case 2:
                            sp2++;
                            break;
                        case 3:
                            sp3++;
                            break;
                        case 4:
                            sp4++;
                            break;
                    }
                }
                
            }
            return sp0 + "/" + p0 + " - " + sp1 + "/" + p1 + " - " + sp2 + "/" + p2 + " - " + sp3 + "/" + p3 + " - " + sp4 + "/" + p4;
        }
    }
}
