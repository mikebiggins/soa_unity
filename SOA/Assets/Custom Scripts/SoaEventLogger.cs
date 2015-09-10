﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace soa
{
    public class SoaEventLogger
    {
        private static System.DateTime epoch = new System.DateTime(1970, 1, 1);
        private XmlDocument xmlDoc;
        private XmlNode simulationNode;
        private XmlNode eventsNode;
        private string outputFile;
        private bool logToFile;
        private bool logToConsole;

        public SoaEventLogger(string outputFile, string configFile, bool logToFile, bool logToConsole)
        {
            // Save whether the logger is enabled
            this.logToFile = logToFile;
            this.logToConsole = logToConsole;

            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Only take action if logger is enabled
            if (logToFile)
            {
                // Save log output filename
                this.outputFile = outputFile;

                // Create a XML document to log data with
                xmlDoc = new XmlDocument();
                
                // Create "Log" node
                XmlNode logNode = xmlDoc.CreateElement("Log");
                xmlDoc.AppendChild(logNode);

                // Create "Simulation" node
                simulationNode = xmlDoc.CreateElement("Simulation");
                logNode.AppendChild(simulationNode);

                // Populate with attributes decribing the simulation
                AddAttribute(simulationNode, "configFile", configFile);
                AddAttribute(simulationNode, "startDateTime", now.ToString("MM\\/dd\\/yyyy hh\\:mm\\:ss.ffffff"));
                AddAttribute(simulationNode, "startTimeStamp", timeStamp);

                // Create "Events" node
                eventsNode = xmlDoc.CreateElement("Events");
                logNode.AppendChild(eventsNode);
            }
            if (logToConsole)
            {
                Debug.Log("SIMULATION (" + timeStamp + "): Loading config file " + configFile);
                Debug.Log("SIMULATION (" + timeStamp + "): Start date/time " + now.ToString("MM\\/dd\\/yyyy hh\\:mm\\:ss.ffffff"));
            }
        }
 
        /******************************************* HELPER FUNCTIONS *****************************************/
        private string CreateTimestamp(DateTime now)
        {
            return ((UInt64)(now.ToUniversalTime() - epoch).Ticks / 10000).ToString();
        }
     
        private void AddAttribute(XmlNode node, string attribute, string value)
        {
            XmlAttribute newAttribute = xmlDoc.CreateAttribute(attribute);
            newAttribute.Value = value;
            node.Attributes.Append(newAttribute);
        }

        /********************************************** TERMINATION ********************************************/
        public void TerminateLogging()
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);
            
            if (logToFile)
            {
                // Log current time
                AddAttribute(simulationNode, "stopDateTime", now.ToString("MM\\/dd\\/yyyy hh\\:mm\\:ss.ffffff"));
                AddAttribute(simulationNode, "stopTimeStamp", timeStamp);

                // Write contents to file
                xmlDoc.Save(outputFile);
            }
            if (logToConsole)
            {
                Debug.Log("SIMULATION (" + timeStamp + "): Terminate date/time " + now.ToString("MM\\/dd\\/yyyy hh\\:mm\\:ss.ffffff"));
            }
        }

        /******************************************* PUBLIC LOGGING ********************************************/
        public void LogSupplyDelivered(string deliverer, string destination)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            if (logToFile)
            {
                // Create a "SupplyDelivery" node
                XmlNode node = xmlDoc.CreateElement("SupplyDelivery");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "deliverer", deliverer);
                AddAttribute(node, "destination", destination);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Debug.Log("EVENT (" + timeStamp + "): Supply delivered by " + deliverer + " to " + destination);
            }
        }

        public void LogRedTruckCaptured(string capturer, string captured)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            if (logToFile)
            {
                // Create a "RedTruckCaptured" node
                XmlNode node = xmlDoc.CreateElement("RedTruckCaptured");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "capturer", capturer);
                AddAttribute(node, "captured", captured);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Debug.Log("EVENT (" + timeStamp + "): Red truck " + captured + " captured by " + capturer);
            }
        }

        public void LogRedDismountCaptured(string capturer, string captured)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            if (logToFile)
            {
                // Create a "RedDismountCaptured" node
                XmlNode node = xmlDoc.CreateElement("RedDismountCaptured");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "capturer", capturer);
                AddAttribute(node, "captured", captured);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Debug.Log("EVENT (" + timeStamp + "): Red dismount " + captured + " captured by " + capturer);
            }
        }

        public void LogCasualtyDelivery(string deliverer, string destination)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            if (logToFile)
            {
                // Create a "CasualtyDelivery" node
                XmlNode node = xmlDoc.CreateElement("CasualtyDelivery");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "deliverer", deliverer);
                AddAttribute(node, "destination", destination);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Debug.Log("EVENT (" + timeStamp + "): Casualty delivered by " + deliverer + " to " + destination);
            }
        }

        public void LogHeavyUAVLost(string victim, string killer)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            if (logToFile)
            {
                // Create a "HeavyUAVLost" node
                XmlNode node = xmlDoc.CreateElement("HeavyUAVLost");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "victim", victim);
                AddAttribute(node, "killer", killer);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Debug.Log("EVENT (" + timeStamp + "): Heavy UAV " + victim + " killed by " + killer);
            }
        }

        public void LogSmallUAVLost(string victim, string killer)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            if (logToFile)
            {
                // Create a "SmallUAVLost" node
                XmlNode node = xmlDoc.CreateElement("SmallUAVLost");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "victim", victim);
                AddAttribute(node, "killer", killer);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Debug.Log("EVENT (" + timeStamp + "): Small UAV " + victim + " killed by " + killer);
            }
        }

        public void LogCivilianInRedCustody(string capturer, string destination)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            if (logToFile)
            {
                // Create a "CivilianInRedCustody" node
                XmlNode node = xmlDoc.CreateElement("CivilianInRedCustody");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "capturer", capturer);
                AddAttribute(node, "destination", destination);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Debug.Log("EVENT (" + timeStamp + "): Civilian transported by " + capturer + " to custody of " + destination);
            }
        }
    }
}

