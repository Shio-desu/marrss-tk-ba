/**
* ----------------------------------------------------------------
* Theo Kaminsky
*
*/
using System;
using System.Collections.Generic;
using System.Linq;
using MARRSS.Global;

using MARRSS.Definition;

namespace MARRSS.Scheduler
{
    /**
    * \brief Objective Function Interface
    *
    * This Interface contains the declaration of the important functions for the original ObjectiveFunction class (weighted sum)
    * and the new EpsilonConstraint class
    */
    interface ObjectiveFunctionInterface
    {
        void calculateValues(ContactWindowsVector currentSolution, ContactWindowsVector completeContacts, int numberOfAllContacts,
            ContactWindow contactToAdd);
        void calculateValues(ContactWindowsVector contactWindows);
        void calculateValues(ContactWindowsVector contactWindows, int[] population);
        double getObjectiveResults();
        string ToString();
        double getDurationValue();
        double getSatelliteFairnessValue();
        double getStationFairnessValue();
        double getScheduledContactsValue();
        double getPriorityValue();
    }
}
