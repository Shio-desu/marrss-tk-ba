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

using MARRSS.Interface2;
using MARRSS.Interface1;
using MARRSS.Definition;
using MARRSS.Global;

namespace MARRSS.Scheduler
{
    /**
    * \brief Scheduling Problem
    *
    * This class defines the problem to solve for the scheduler. Herby the Contact
    * windows are checked if they meet the necesary requirements. For example
    * long enough contact time.
    */
    class SchedulingProblem : ScheduleProblemInterface
    {
        private ContactWindowsVector schedulerContacts; //!< ContactWindowsVector all contact windows
        private List<RequestInterface> schedulerRequests; //!< List<RequestInterface> list of all Requests
        private ObjectiveFunctionInterface objectives; //!< Objectives to solve Problem with

        //! SchedulingProblem constructor.
        /*!
            empty constructor
        */
        public SchedulingProblem()
        {

        }

        //! Get the Objective-Function of the current Problem
        /*!
            \return ObjectiveFunction
        */
        public ObjectiveFunctionInterface getObjectiveFunction()
        {
            return objectives;
        }

        //! Set Objective-Function to current Problem
        /*!
            \pram ObjectiveFunction objectives for the problem
        */
        public void setObjectiveFunction(ObjectiveFunctionInterface objectiveFunction)
        {
            objectives = objectiveFunction;
        }

        //! Set ContactWindows to current Problem
        /*!
            \pram ContactWindowsVector all contacts
        */
        public void setContactWindows(ContactWindowsVector contacts)
        {
            schedulerContacts = contacts;
        }

        //! Sets the Request List
        /*!
            \pram List<RequestInterface> all Requestst to consider
        */
        public void setRequests(List<RequestInterface> requestsList)
        {
            schedulerRequests = requestsList;
        }

        //! Sets the Requests to ContactWindows
        /*!
            
        */
        public void setRequestToContact()
        {
            //toDo asign requests to Contacts
            //remove all Contacts that are not asigned a request
        }

        //! Returns the ContactWindows 
        /*!
            \return ContactWindowsVector
        */
        public ContactWindowsVector getContactWindows()
        {
            return schedulerContacts;
        }

        //! Remove unwanted contact windows 
        /*!
            \param int Min. Duration window
            Deletes all contact windows whitch contact times are lower then min duration
        */
        public void removeUnwantedContacts(int minDuration)
        {
            for (int i = 0; i < schedulerContacts.Count(); i++ )
            {
                if (schedulerContacts.getAt(i).getDuration() < minDuration)
                {
                    schedulerContacts.deleteAt(i);
                    i--;
                }
            }
        }

        //------------------------Create Scenarios-----------------------------
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        public void GenerateSzenarioA()
        {
            for (int i = 0; i < schedulerContacts.Count(); i++)
            {
                schedulerContacts.getAt(i).setPriority(Global.Structs.priority.NONE);
            }
        }
        
        public void GenerateSzenarioC(int seed = 0)
        { 
            Random rnd;
            if (seed == 0)
                rnd = new Random();
            else
                rnd = new Random(seed);

            for (int i= 0; i < schedulerContacts.Count(); i++)
            {
                if (schedulerContacts.getAt(i).getSatName() == "UWE-3")
                {
                    schedulerContacts.getAt(i).setPriority(0);
                }
                else
                {
                    int p = rnd.Next(0, 5); 
                    Global.Structs.priority pr;
                    pr = (Structs.priority)p;
                    schedulerContacts.getAt(i).setPriority(pr);
                }
            }
        }

        public void GenerateSzenarioB(int seed = 0)
        {
            Random rnd;
            if (seed == 0)
                rnd = new Random();
            else
                rnd = new Random(seed);

            for (int i = 0; i < schedulerContacts.Count(); i++)
            {
                int p = rnd.Next(0, 5);
                Global.Structs.priority pr;
                pr = (Structs.priority)p;
                schedulerContacts.getAt(i).setPriority(pr);
            }
        }

        public void GenerateSzenarioD(int seed = 0)
        {
            Random rnd;
            if (seed == 0)
                rnd = new Random();
            else
                rnd = new Random(seed);

            for (int i = 0; i < schedulerContacts.Count(); i++)
            {
                bool found = false;
                //------------1
                if (schedulerContacts.getAt(i).getSatName() == "QUBE"
                    && schedulerContacts.getAt(i).getStationName() == 
                    "Würzburg (Zentrum für Telematik)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }
                //------------2
                if (schedulerContacts.getAt(i).getSatName().StartsWith("CloudCT_Sat")
                    && schedulerContacts.getAt(i).getStationName() ==
                    "Würzburg (Zentrum für Telematik)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }
                //------------3
                if (schedulerContacts.getAt(i).getSatName() == "Random 2 (KAFASAT)"
                   && schedulerContacts.getAt(i).getStationName() ==
                   "Santa Maria (INPE)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }
                //------------4
                if (schedulerContacts.getAt(i).getSatName() == "Random 1 (MCUBED-2)"
                   && schedulerContacts.getAt(i).getStationName() ==
                   "Shandong (SISET 2)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }
                //------------5
                if (schedulerContacts.getAt(i).getSatName() == "Random 3 (Iridium 113)"
                   && schedulerContacts.getAt(i).getStationName() ==
                   "Montreal (Polytechnique Montreal)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }
                //------------6
                if (schedulerContacts.getAt(i).getSatName() == "Random 4 (PIXL-1)"
                   && schedulerContacts.getAt(i).getStationName() ==
                   "Natal (INPE)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }
                //------------7
                if (schedulerContacts.getAt(i).getSatName() == "Random 5 (UNISAT-7)"
                  && schedulerContacts.getAt(i).getStationName() ==
                  "Athens (University of Georgia)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }
                //------------8
                if (schedulerContacts.getAt(i).getSatName() == "Random 6 (TIGRISAT)"
                  && schedulerContacts.getAt(i).getStationName() ==
                  "Stellenbosch (Stellenbosch University)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }
                //------------9
                if (schedulerContacts.getAt(i).getSatName() == "Random 7 (Taurus-1)"
                  && schedulerContacts.getAt(i).getStationName() ==
                  "München (TUM 1)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }
                //------------10
                if (schedulerContacts.getAt(i).getSatName() == "Random 8 (KSM1-B)"
                  && schedulerContacts.getAt(i).getStationName() ==
                  "Würzburg (Computer Science Institute)")
                {
                    schedulerContacts.getAt(i).setPriority(Structs.priority.CRITICAL);
                    found = true;
                }

                if (!found)
                {
                    int p = rnd.Next(1, 5);
                    Global.Structs.priority pr;
                    pr = (Structs.priority)p;
                    schedulerContacts.getAt(i).setPriority(pr);
                }

            }
        }
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
    }
}
