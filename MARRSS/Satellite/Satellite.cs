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

namespace MARRSS.Satellite
{
    /**
    * \brief Satellite Class
    *
    * This class defines the satellite object containing of a name the NORAD-ID
    * and its two line element.
    */
    class Satellite
    {
        private string name; //!< string satellite name
        private One_Sgp4.Tle tleData; //!< TLE two line element data for the satellite

        //! Satellite constructor.
        /*!
            \param string name of the satellite
        */
        public Satellite(string _name)
        {
            name = _name;
        }

        //! Satellite constructor.
        /*!
            \param string name of the satellite
            \param Tle Two Line Element data
        */
        public Satellite(string _name, One_Sgp4.Tle _tleData)
        {
            name = _name;
            tleData = _tleData;
        }

        //! Sets the TLE-Data for this object
        /*!
            \pram tle TwoLineElement data 
            \return bool if successful
        */
        public bool setTleData(One_Sgp4.Tle _tleData)
        {
            //toDo
            //Check Tle Data if its newer
            tleData = _tleData;
            return true;
        }

        //! Returns the name of the satellite
        /*!
            \return string satellite name
        */
        public string getName()
        {
            return name;
        }

        //! Returns the TLE data of the satellite
        /*!
            \return Tle two line element data
        */
        public One_Sgp4.Tle getTleData()
        {
            return tleData;
        }
    }
}
