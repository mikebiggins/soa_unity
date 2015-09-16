﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class MCTrialGenerator
    {
        /******************** ENVIRONMENT ********************/
        // Environment Configuration
         const string envConfigFile = "EnvConfig.xml";

        /********************** NETWORK **********************/
         const string networkRedRoomHeader = "soa-mc-red_";
         const string networkBlueRoomHeader = "soa-mc-blue_";

        /******************** MONTE CARLO ********************/
         const int numMCTrials = 10;
         const string soaConfigFileHeader = "MCConfig_";
         const string soaLoggerFileHeader = "MCOutput_";
         const bool logEvents = true;
         const bool logToUnityConsole = true;

        /******************** REMOTE ID **********************/
         const int remoteStartID = 200;
        private int availableRemoteID;
        
        private class PlatformLaydown
        {
            public float numMean;
            public float numStdDev;
            public int numMin;
            public int numMax;
            public float fromAnchorStdDev_km; // Along x and z dimensions separately 
            public float fromAnchorMax_km; // Along x and z dimensions separately
            public float altitudeMean_km;
            public float altitudeStdDev_km;
            public float altitudeMin_km;
            public float altitudeMax_km;
            public List<PrimitivePair<int, int>> anchors;
            public HashSet<PrimitivePair<int, int>> allowedCells;
        }

        /****************** LOCAL PLATFORMS ******************/
        // Red dismounts 
        private PlatformLaydown redDismountLaydown;
        private float probRedDismountWeaponized;

        // Red trucks
        private PlatformLaydown redTruckLaydown;
        private float probRedTruckWeaponized;
   
        // Neutral dismounts
        private PlatformLaydown neutralDismountLaydown;
       
        // Neutral trucks
        private PlatformLaydown neutralTruckLaydown;
    
        // Blue police
        private PlatformLaydown bluePoliceLaydown;

        /***************** REMOTE PLATFORMS ******************/
        // Heavy UAVs
        private PlatformLaydown heavyUAVLaydown;

        // Small UAVs
        private PlatformLaydown smallUAVLaydown;

        // Balloons
        private PlatformLaydown balloonLaydown;

        /***************** CLASS DEFINITION ******************/
        private GridMath gridMath;
        private HashSet<PrimitivePair<int, int>> landCells;
        private HashSet<PrimitivePair<int, int>> allCells;
        private Random rand;
        private EnvConfig envConfig;
        public MCTrialGenerator()
        {
            // Read in envConfig
            envConfig = EnvConfigXMLReader.Parse(envConfigFile);
            if (envConfig == null)
            {
                Console.WriteLine("MCTrialGenerator::Main(): Parsed envConfig is null, exiting");
                return;
            }

            // Initialize GridMath object
            gridMath = new GridMath(envConfig.gridOrigin_x, envConfig.gridOrigin_z, envConfig.gridToWorldScale);

            // Populate hashsets of cells for easy lookup
            landCells = new HashSet<PrimitivePair<int, int>>(envConfig.landCells);
            allCells = new HashSet<PrimitivePair<int, int>>(envConfig.landCells);
            allCells.UnionWith(envConfig.waterCells);
            allCells.UnionWith(envConfig.mountainCells);

            // Initialize remote platform ID assignment
            availableRemoteID = remoteStartID;

            // Initialize laydown objects
            InitializeLocalLaydown();
            InitializeRemoteLaydown();

            // Initialize random number generator
            rand = new Random(); //reuse this if you are generating many
        }

        public void InitializeLocalLaydown()
        {
            // Pointer
            PlatformLaydown laydownPtr;

            // Red dismounts
            redDismountLaydown = new PlatformLaydown();
            laydownPtr = redDismountLaydown;
            laydownPtr.numMean = 3;
            laydownPtr.numStdDev = 2;
            laydownPtr.numMin = 1;
            laydownPtr.numMax = 10;
            laydownPtr.fromAnchorStdDev_km = 1;
            laydownPtr.fromAnchorMax_km = 2;
            laydownPtr.altitudeMean_km = 0.6f;
            laydownPtr.altitudeStdDev_km = 0.0f;
            laydownPtr.altitudeMin_km = 0.6f;
            laydownPtr.altitudeMax_km = 0.6f;
            laydownPtr.anchors = envConfig.redBaseCells;
            laydownPtr.allowedCells = landCells;
            probRedDismountWeaponized = 0.5f;

            // Red trucks
            redTruckLaydown = new PlatformLaydown();
            laydownPtr = redTruckLaydown;
            laydownPtr.numMean = 3;
            laydownPtr.numStdDev = 2;
            laydownPtr.numMin = 1;
            laydownPtr.numMax = 10;
            laydownPtr.fromAnchorStdDev_km = 1;
            laydownPtr.fromAnchorMax_km = 2;
            laydownPtr.altitudeMean_km = 0.6f;
            laydownPtr.altitudeStdDev_km = 0.0f;
            laydownPtr.altitudeMin_km = 0.6f;
            laydownPtr.altitudeMax_km = 0.6f;
            laydownPtr.anchors = envConfig.redBaseCells;
            laydownPtr.allowedCells = landCells;
            probRedTruckWeaponized = 0.5f;

            // Merge list of NGO and village cells
            List<PrimitivePair<int, int>> neutralSiteCells = new List<PrimitivePair<int, int>>();
            neutralSiteCells.AddRange(envConfig.ngoSiteCells);
            neutralSiteCells.AddRange(envConfig.villageCells);

            // Neutral dismounts
            neutralDismountLaydown = new PlatformLaydown();
            laydownPtr = neutralDismountLaydown;
            laydownPtr.numMean = 2;
            laydownPtr.numStdDev = 2;
            laydownPtr.numMin = 0;
            laydownPtr.numMax = 4;
            laydownPtr.fromAnchorStdDev_km = 2;
            laydownPtr.fromAnchorMax_km = 5;
            laydownPtr.altitudeMean_km = 0.6f;
            laydownPtr.altitudeStdDev_km = 0.0f;
            laydownPtr.altitudeMin_km = 0.6f;
            laydownPtr.altitudeMax_km = 0.6f;
            laydownPtr.anchors = neutralSiteCells;
            laydownPtr.allowedCells = landCells;

            // Neutral trucks
            neutralTruckLaydown = new PlatformLaydown();
            laydownPtr = neutralTruckLaydown;
            laydownPtr.numMean = 2;
            laydownPtr.numStdDev = 2;
            laydownPtr.numMin = 0;
            laydownPtr.numMax = 4;
            laydownPtr.fromAnchorStdDev_km = 2;
            laydownPtr.fromAnchorMax_km = 5;
            laydownPtr.altitudeMean_km = 0.6f;
            laydownPtr.altitudeStdDev_km = 0.0f;
            laydownPtr.altitudeMin_km = 0.6f;
            laydownPtr.altitudeMax_km = 0.6f;
            laydownPtr.anchors = neutralSiteCells;
            laydownPtr.allowedCells = landCells;

            // Blue police
            bluePoliceLaydown = new PlatformLaydown();
            laydownPtr = bluePoliceLaydown;
            laydownPtr.numMean = 1;
            laydownPtr.numStdDev = 0.5f;
            laydownPtr.numMin = 1;
            laydownPtr.numMax = 1;
            laydownPtr.fromAnchorStdDev_km = 2;
            laydownPtr.fromAnchorMax_km = 5;
            laydownPtr.altitudeMean_km = 0.6f;
            laydownPtr.altitudeStdDev_km = 0.0f;
            laydownPtr.altitudeMin_km = 0.6f;
            laydownPtr.altitudeMax_km = 0.6f;
            laydownPtr.anchors = envConfig.blueBaseCells;
            laydownPtr.allowedCells = landCells;
        }

        public void InitializeRemoteLaydown()
        {
            // Pointer
            PlatformLaydown laydownPtr;

            // Heavy UAV
            heavyUAVLaydown = new PlatformLaydown();
            laydownPtr = heavyUAVLaydown;
            laydownPtr.numMean = 3.5f;
            laydownPtr.numStdDev = 0.5f;
            laydownPtr.numMin = 2;
            laydownPtr.numMax = 5;
            laydownPtr.fromAnchorStdDev_km = 2;
            laydownPtr.fromAnchorMax_km = 5;
            laydownPtr.altitudeMean_km = 0.6f;
            laydownPtr.altitudeStdDev_km = 0.0f;
            laydownPtr.altitudeMin_km = 0.6f;
            laydownPtr.altitudeMax_km = 0.6f;
            laydownPtr.anchors = envConfig.blueBaseCells;
            laydownPtr.allowedCells = allCells;

            // Small UAV
            smallUAVLaydown = new PlatformLaydown();
            laydownPtr = smallUAVLaydown;
            laydownPtr.numMean = 3.5f;
            laydownPtr.numStdDev = 0.5f;
            laydownPtr.numMin = 2;
            laydownPtr.numMax = 5;
            laydownPtr.fromAnchorStdDev_km = 2;
            laydownPtr.fromAnchorMax_km = 5;
            laydownPtr.altitudeMean_km = 0.6f;
            laydownPtr.altitudeStdDev_km = 0.0f;
            laydownPtr.altitudeMin_km = 0.6f;
            laydownPtr.altitudeMax_km = 0.6f;
            laydownPtr.anchors = envConfig.blueBaseCells;
            laydownPtr.allowedCells = allCells;

            // Balloon
            balloonLaydown = new PlatformLaydown();
            laydownPtr = balloonLaydown;
            laydownPtr.numMean = 1;
            laydownPtr.numStdDev = 0.0f;
            laydownPtr.numMin = 1;
            laydownPtr.numMax = 1;
            laydownPtr.fromAnchorStdDev_km = 2;
            laydownPtr.fromAnchorMax_km = 5;
            laydownPtr.altitudeMean_km = 0.6f;
            laydownPtr.altitudeStdDev_km = 0.0f;
            laydownPtr.altitudeMin_km = 0.6f;
            laydownPtr.altitudeMax_km = 0.6f;
            laydownPtr.anchors = envConfig.blueBaseCells;
            laydownPtr.allowedCells = allCells;
        }

        #region Config File Generator
        public void GenerateConfigFiles()
        {
            // Find out trial number formatting
            int numTrialDigits = numMCTrials.ToString().Length;
            string toStringFormat = "D" + numTrialDigits.ToString();

            // Generate each trial
            SoaConfig soaConfig;
            List<PrimitiveTriple<float, float, float>> randomizedPositions;
            for (int trial = 1; trial <= numMCTrials; trial++)
            {
                // Create a new SoaConfig object
                soaConfig = new SoaConfig();

                // Populate network
                soaConfig.networkRedRoom = networkRedRoomHeader + trial.ToString(toStringFormat);
                soaConfig.networkBlueRoom = networkBlueRoomHeader + trial.ToString(toStringFormat);

                // Simulation configuration
                soaConfig.probRedDismountWeaponized = probRedDismountWeaponized;
                soaConfig.probRedTruckWeaponized = probRedTruckWeaponized;

                // Logger configuration
                soaConfig.loggerOutputFile = soaLoggerFileHeader + trial.ToString(toStringFormat);
                soaConfig.enableLogToFile = true;
                soaConfig.enableLogEventsToFile = logEvents;
                soaConfig.enableLogToUnityConsole = logToUnityConsole;

                // Local units: Red Dismount
                randomizedPositions = RandomizeLaydown(redDismountLaydown);
                for (int i = 0; i < randomizedPositions.Count; i++)
                {
                    soaConfig.localPlatforms.Add(new RedDismountConfig(
                        randomizedPositions[i].first, // x
                        randomizedPositions[i].second, // y
                        randomizedPositions[i].third,  // z
                        -1, // id
                        "null", // initialWaypoint
                        (rand.NextDouble() <= probRedDismountWeaponized) // hasWeapon
                        ));
                }

                // Local units: Red Truck
                randomizedPositions = RandomizeLaydown(redTruckLaydown);
                for (int i = 0; i < randomizedPositions.Count; i++)
                {
                    soaConfig.localPlatforms.Add(new RedTruckConfig(
                        randomizedPositions[i].first, // x
                        randomizedPositions[i].second, // y
                        randomizedPositions[i].third,  // z
                        -1, // id
                        "null", // initialWaypoint
                        (rand.NextDouble() <= probRedDismountWeaponized) // hasWeapon
                        ));
                }

                // Local units: Neutral Dismount
                randomizedPositions = RandomizeLaydown(neutralDismountLaydown);
                for (int i = 0; i < randomizedPositions.Count; i++)
                {
                    soaConfig.localPlatforms.Add(new NeutralDismountConfig(
                        randomizedPositions[i].first, // x
                        randomizedPositions[i].second, // y
                        randomizedPositions[i].third,  // z
                        -1 // id
                        ));
                }

                // Local units: Neutral Truck
                randomizedPositions = RandomizeLaydown(neutralTruckLaydown);
                for (int i = 0; i < randomizedPositions.Count; i++)
                {
                    soaConfig.localPlatforms.Add(new NeutralTruckConfig(
                        randomizedPositions[i].first, // x
                        randomizedPositions[i].second, // y
                        randomizedPositions[i].third,  // z
                        -1 // id
                        ));
                }

                // Local units: Blue Police
                randomizedPositions = RandomizeLaydown(bluePoliceLaydown);
                for (int i = 0; i < randomizedPositions.Count; i++)
                {
                    soaConfig.localPlatforms.Add(new BluePoliceConfig(
                        randomizedPositions[i].first, // x
                        randomizedPositions[i].second, // y
                        randomizedPositions[i].third,  // z
                        -1 // id
                        ));
                }

                // Remote units: Heavy UAV
                randomizedPositions = RandomizeLaydown(heavyUAVLaydown);
                for (int i = 0; i < randomizedPositions.Count; i++)
                {
                    soaConfig.remotePlatforms.Add(new HeavyUAVConfig(
                        randomizedPositions[i].first, // x
                        randomizedPositions[i].second, // y
                        randomizedPositions[i].third,  // z
                        availableRemoteID++ // id
                        ));
                }

                // Remote units: Small UAV
                randomizedPositions = RandomizeLaydown(smallUAVLaydown);
                for (int i = 0; i < randomizedPositions.Count; i++)
                {
                    soaConfig.remotePlatforms.Add(new SmallUAVConfig(
                        randomizedPositions[i].first, // x
                        randomizedPositions[i].second, // y
                        randomizedPositions[i].third,  // z
                        availableRemoteID++ // id
                        ));
                }

                // Remote units: Balloon
                randomizedPositions = RandomizeLaydown(balloonLaydown);
                for (int i = 0; i < randomizedPositions.Count; i++)
                {
                    soaConfig.remotePlatforms.Add(new BalloonConfig(
                        randomizedPositions[i].first, // x
                        randomizedPositions[i].second, // y
                        randomizedPositions[i].third,  // z
                        availableRemoteID++ // id
                        ));
                }

                // Write SoaConfig contents to a config file
                SoaConfigXMLWriter.Write(soaConfig, soaConfigFileHeader + trial.ToString(toStringFormat));
            } // End current trial
        }
        #endregion

        #region Helper Functions
        /****************** MAIN FUNCTION ********************/
        private float RandN(float mean, float stdDev, float min, float max)
        {
            double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

            return (float)Math.Min(Math.Max(randNormal, min), max);
        }

        private bool IsLandCell(float x, float z)
        {
            // Convert world coordinate to cell coordinate
            PrimitivePair<int, int> cell = gridMath.WorldToGrid(new PrimitivePair<float, float>(x, z));

            // Check to see if that cell belongs to the list/set of land cells
            return landCells.Contains(cell);
        }

        private bool IsAnyCell(float x, float z)
        {
            // Convert world coordinate to cell coordinate
            PrimitivePair<int, int> cell = gridMath.WorldToGrid(new PrimitivePair<float, float>(x, z));

            // Check to see if that cell belongs to the list/set of land, water, or mountain cells
            return allCells.Contains(cell);
        }

        // Computes list of laydowns
        private List<PrimitiveTriple<float, float, float>> RandomizeLaydown(PlatformLaydown laydown)
        {
            // List to return
            List<PrimitiveTriple<float, float, float>> posList = new List<PrimitiveTriple<float, float, float>>();

            // First determine the # of units
            int numUnits = (int)Math.Round(RandN(laydown.numMean, laydown.numStdDev, (float)laydown.numMin, (float)laydown.numMax));

            // For each unit
            PrimitivePair<float, float> tempPos = new PrimitivePair<float, float>(0, 0);
            PrimitivePair<float, float> anchor;
            PrimitivePair<int, int> tempGrid;
            bool laydownFound;
            float tempAltitude;
            for (int i = 0; i < numUnits; i++)
            {
                // Laydown not valid by default
                laydownFound = false;

                // Keep trying points until we find one that satisfies grid constraints
                while (!laydownFound)
                {
                    // Randomly pick an anchor and convert coordinates to world
                    anchor = gridMath.GridToWorld(laydown.anchors[rand.Next(0, laydown.anchors.Count)]); // Note: rand.Next max value is exclusive

                    // Now independently pick X and Z deviations from that point
                    tempPos.first = anchor.first + RandN(0.0f, laydown.fromAnchorStdDev_km, 0.0f, laydown.fromAnchorMax_km);
                    tempPos.second = anchor.second + RandN(0.0f, laydown.fromAnchorStdDev_km, 0.0f, laydown.fromAnchorMax_km);

                    // Convert that temp position to grid
                    tempGrid = gridMath.WorldToGrid(tempPos);

                    // Check to see if the grid is within allowed
                    if (laydown.allowedCells.Contains(tempGrid))
                    {
                        laydownFound = true;
                    }
                }

                // Now pick an altitude randomly
                tempAltitude = RandN(laydown.altitudeMean_km, laydown.altitudeStdDev_km, laydown.altitudeMin_km, laydown.altitudeMax_km);

                // Save the 3D point
                posList.Add(new PrimitiveTriple<float, float, float>(tempPos.first, tempAltitude, tempPos.second));
            }

            // Return list of randomized positions
            return posList;
        }
        #endregion

        #region Main Function
        /****************** MAIN FUNCTION ********************/
        public static void Main()
        {
            MCTrialGenerator mcTrialGenerator = new MCTrialGenerator();
            mcTrialGenerator.GenerateConfigFiles();
        }
        #endregion
    }
}
