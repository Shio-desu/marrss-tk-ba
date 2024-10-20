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

using MARRSS.Definition;
using MARRSS.Global;

namespace MARRSS.Ground
{
    /**
    * \brief Station Class definition.
    *
    * This class defines the object ground station. Each station as a coordinate
    * and a name. Currently each ground station will be seen has having only one
    * antenna with now furtur information.
    * Can be expanded with the Antenna Class in the future. 
    */
    class Station
    {
        private string name; //!< string name of the Station
        private Definition.GeoCoordinate geoCoordinate; //!< GeoCoordinate position of the stations
        private List<Antenna> antennaList; //!< List<Antenna> list of available antennas
        private double minElevation = 0.000000; //!< double min elevation of groundstation

        //! Stations constructor.
        /*!
            \param string name of station
            \param GeoCoordinate position of the Station
            constructs a basic Groundstation with a single basic antenna at the given coordinates
        */
        public Station(string _name, Definition.GeoCoordinate _geoCord)
        {
            antennaList = new List<Antenna>();
            geoCoordinate = _geoCord;
            name = _name;
        }

        //! Stations constructor.
        /*!
            \param string name of station
            \param GeoCoordinate position of the Station
            \param List<Antennas> list of available antennas 
            constructs a Groundstation at the given coordinates
        */
        public Station(string _name, Definition.GeoCoordinate _geoCord,
            List<Antenna> antennas)
        {
            antennaList = new List<Antenna>();
            geoCoordinate = _geoCord;
            name = _name;
            antennaList = antennas;

        }

        //! Stations constructor.
        /*!
            \param string name of station
            \param GeoCoordinate position of the Station
            \param Antenna antenna available at the stations
            constructs a Groundstationat with a advanced antenna the given coordinates
        */
        public Station(string _name, Definition.GeoCoordinate _geoCord,
            Antenna _antenna)
        {
            antennaList = new List<Antenna>();
            geoCoordinate = _geoCord;
            name = _name;
            antennaList.Add(_antenna);
        }

        //! Stations constructor.
        /*!
            \param string name of station
            \param double latetude of the station
            \param double longitude of the station
            \param double height of the stations
            constructs a Groundstationat with one antenna the given coordinates
        */
        public Station(string _name, double latetude, double longetude,
                        double height = 0.0)
        {
            geoCoordinate = new Definition.GeoCoordinate(latetude, longetude, height);
            name = _name;
        }

        //! Set the Coordinates of the Stations
        /*!
            \param GeoCoordinate
        */
        public void setGeoCoordinate(Definition.GeoCoordinate _geoCoord)
        {
            geoCoordinate = _geoCoord;
        }

        //! returns the Coordinates of the stations
        /*!
            \return GeoCoordinate
        */
        public Definition.GeoCoordinate getGeoCoordinate()
        {
            return geoCoordinate;
        }

        //! returns the Name of the stations
        /*!
            \return string
        */
        public string getName()
        {
            return name;
        }

        //! Add a Antenna to the stations
        /*!
            \param Antenna
        */
        public void addAntenna(Antenna AntennaToAdd)
        {
            antennaList.Add(AntennaToAdd);
        }

        //! returns the nr of Antennas available
        /*!
            \return int
        */
        public int getNrOfAntennas()
        {
            int count = 1;
            try
            {
                count = antennaList.Count();
            }
            catch
            {
                count = 1;
            }
            return count;
        }

        //! returns local sidreal time at any given time
        /*!
            \pram TimeDate current time
            \return double LocalSidrealTime
        */
        public double getLocalSidrealTime(One_Sgp4.EpochTime time)
        {
            return time.getLocalSiderealTime(geoCoordinate.getLongitude());
        }

        //! returns the longitude
        /*!
            \return double longitude
        */
        public double getLongitude()
        {
            return geoCoordinate.getLongitude();
        }

        //! returns the latitude
        /*!
            \return double latitude
        */
        public double getLatitude()
        {
            return geoCoordinate.getLatetude();
        }

        //! returns the min elevation
        /*!
            \return double minElevation
        */
        public double getMinElevation()
        {
            return minElevation;
        }

        //! returns the Position of the Station in ECI-vector
        /*!
            \param double localSidrealTime at station
            \return point3D ECI coordinates as x,y,z-vector
        */
        public Structs.point3D getEci(double localSidrealTime)
        {
            return geoCoordinate.toECI(localSidrealTime);
        }

        //! returns the hight of the station
        /*!
            \return double height
        */
        public double getHeight()
        {
            return geoCoordinate.getHeight();
        }

    }
}
