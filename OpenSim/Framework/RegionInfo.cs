/*
* Copyright (c) Contributors, http://opensimulator.org/
* See CONTRIBUTORS.TXT for a full list of copyright holders.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the OpenSim Project nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
* 
*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using libsecondlife;
using Nini.Config;
using OpenSim.Framework.Console;

namespace OpenSim.Framework
{
    [Serializable]
    public class SimpleRegionInfo
    {
        public SimpleRegionInfo()
        {
        }

        public SimpleRegionInfo(uint regionLocX, uint regionLocY, IPEndPoint internalEndPoint, string externalUri)
        {
            m_regionLocX = regionLocX;
            m_regionLocY = regionLocY;

            m_internalEndPoint = internalEndPoint;
            m_externalHostName = externalUri;
        }

        public SimpleRegionInfo(uint regionLocX, uint regionLocY, string externalUri, uint port)
        {
            m_regionLocX = regionLocX;
            m_regionLocY = regionLocY;

            m_externalHostName = externalUri;

            m_internalEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), (int) port);
        }

        public SimpleRegionInfo(RegionInfo ConvertFrom)
        {
            m_regionLocX = ConvertFrom.RegionLocX;
            m_regionLocY = ConvertFrom.RegionLocY;
            m_internalEndPoint = ConvertFrom.InternalEndPoint;
            m_externalHostName = ConvertFrom.ExternalHostName;
            m_remotingPort = ConvertFrom.RemotingPort;
            m_allow_alternate_ports = ConvertFrom.m_allow_alternate_ports;
            RemotingAddress = ConvertFrom.RemotingAddress;
            RegionID = LLUUID.Zero;
        }

        public LLUUID RegionID = LLUUID.Zero;

        public uint m_remotingPort;

        public uint RemotingPort
        {
            get { return m_remotingPort; }
            set { m_remotingPort = value; }
        }
        public bool m_allow_alternate_ports;

        public string RemotingAddress;

        public IPEndPoint ExternalEndPoint
        {
            get
            {
                // Old one defaults to IPv6
                //return new IPEndPoint( Dns.GetHostAddresses( m_externalHostName )[0], m_internalEndPoint.Port );

                IPAddress ia = null;
                // If it is already an IP, don't resolve it - just return directly
                if (IPAddress.TryParse(m_externalHostName, out ia))
                    return new IPEndPoint(ia, m_internalEndPoint.Port);

                // Reset for next check
                ia = null;

                // New method favors IPv4
                foreach (IPAddress Adr in Dns.GetHostAddresses(m_externalHostName))
                {
                    if (ia == null)
                        ia = Adr;

                    if (Adr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ia = Adr;
                        break;
                    }
                }

                return new IPEndPoint(ia, m_internalEndPoint.Port);
            }

            set { m_externalHostName = value.ToString(); }
        }

        protected string m_externalHostName;

        public string ExternalHostName
        {
            get { return m_externalHostName; }
            set { m_externalHostName = value; }
        }

        protected bool Allow_Alternate_Ports;

        protected IPEndPoint m_internalEndPoint;

        public IPEndPoint InternalEndPoint
        {
            get { return m_internalEndPoint; }
            set { m_internalEndPoint = value; }
        }

        protected uint? m_regionLocX;

        public uint RegionLocX
        {
            get { return m_regionLocX.Value; }
            set { m_regionLocX = value; }
        }

        protected uint? m_regionLocY;

        public uint RegionLocY
        {
            get { return m_regionLocY.Value; }
            set { m_regionLocY = value; }
        }

        public ulong RegionHandle
        {
            get { return Util.UIntsToLong((RegionLocX*256), (RegionLocY*256)); }
        }
    }

    public class RegionInfo : SimpleRegionInfo
    {
        public string RegionName = String.Empty;

        public string DataStore = String.Empty;
        public bool isSandbox = false;
        public bool commFailTF = false;

//        public bool m_allow_alternate_ports;

        public LLUUID MasterAvatarAssignedUUID = LLUUID.Zero;
        public LLUUID CovenantID = LLUUID.Zero;
        public string MasterAvatarFirstName = String.Empty;
        public string MasterAvatarLastName = String.Empty;
        public string MasterAvatarSandboxPassword = String.Empty;

        // Apparently, we're applying the same estatesettings regardless of whether it's local or remote.
        private EstateSettings m_estateSettings;

        public EstateSettings EstateSettings
        {
            get
            {
                if (m_estateSettings == null)
                {
                    m_estateSettings = new EstateSettings();
                }

                return m_estateSettings;
            }
        }

        public ConfigurationMember configMember;

        public RegionInfo(string description, string filename, bool skipConsoleConfig)
        {
            configMember =
                new ConfigurationMember(filename, description, loadConfigurationOptions, handleIncomingConfiguration, !skipConsoleConfig);
            configMember.performConfigurationRetrieve();
        }

        public RegionInfo(string description, XmlNode xmlNode, bool skipConsoleConfig)
        {
            configMember =
                new ConfigurationMember(xmlNode, description, loadConfigurationOptions, handleIncomingConfiguration, !skipConsoleConfig);
            configMember.performConfigurationRetrieve();
        }

        public RegionInfo(uint regionLocX, uint regionLocY, IPEndPoint internalEndPoint, string externalUri) :
            base(regionLocX, regionLocY, internalEndPoint, externalUri)
        {
        }

        public RegionInfo()
        {
        }

        public RegionInfo(SearializableRegionInfo ConvertFrom)
        {
            m_regionLocX = ConvertFrom.RegionLocX;
            m_regionLocY = ConvertFrom.RegionLocY;
            m_internalEndPoint = ConvertFrom.InternalEndPoint;
            m_externalHostName = ConvertFrom.ExternalHostName;
            m_remotingPort = ConvertFrom.RemotingPort;
            m_allow_alternate_ports = ConvertFrom.m_allow_alternate_ports;
            RemotingAddress = ConvertFrom.RemotingAddress;
            RegionID = LLUUID.Zero;
        }

        public RegionInfo(SimpleRegionInfo ConvertFrom)
        {
            m_regionLocX = ConvertFrom.RegionLocX;
            m_regionLocY = ConvertFrom.RegionLocY;
            m_internalEndPoint = ConvertFrom.InternalEndPoint;
            m_externalHostName = ConvertFrom.ExternalHostName;
            m_remotingPort = ConvertFrom.RemotingPort;
            m_allow_alternate_ports = ConvertFrom.m_allow_alternate_ports;
            RemotingAddress = ConvertFrom.RemotingAddress;
            RegionID = LLUUID.Zero;
        }

        //not in use, should swap to nini though.
        public void LoadFromNiniSource(IConfigSource source)
        {
            LoadFromNiniSource(source, "RegionInfo");
        }

        //not in use, should swap to nini though.
        public void LoadFromNiniSource(IConfigSource source, string sectionName)
        {
            string errorMessage = String.Empty;
            RegionID = new LLUUID(source.Configs[sectionName].GetString("Region_ID", LLUUID.Random().ToString()));
            RegionName = source.Configs[sectionName].GetString("sim_name", "OpenSim Test");
            m_regionLocX = Convert.ToUInt32(source.Configs[sectionName].GetString("sim_location_x", "1000"));
            m_regionLocY = Convert.ToUInt32(source.Configs[sectionName].GetString("sim_location_y", "1000"));
            // this.DataStore = source.Configs[sectionName].GetString("datastore", "OpenSim.db");

            string ipAddress = source.Configs[sectionName].GetString("internal_ip_address", "0.0.0.0");
            IPAddress ipAddressResult;
            if (IPAddress.TryParse(ipAddress, out ipAddressResult))
            {
                m_internalEndPoint = new IPEndPoint(ipAddressResult, 0);
            }
            else
            {
                errorMessage = "needs an IP Address (IPAddress)";
            }
            m_internalEndPoint.Port =
                source.Configs[sectionName].GetInt("internal_ip_port", (int) NetworkServersInfo.DefaultHttpListenerPort);

            string externalHost = source.Configs[sectionName].GetString("external_host_name", "127.0.0.1");
            if (externalHost != "SYSTEMIP")
            {
                m_externalHostName = externalHost;
            }
            else
            {
                m_externalHostName = Util.GetLocalHost().ToString();
            }

            MasterAvatarFirstName = source.Configs[sectionName].GetString("master_avatar_first", "Test");
            MasterAvatarLastName = source.Configs[sectionName].GetString("master_avatar_last", "User");
            MasterAvatarSandboxPassword = source.Configs[sectionName].GetString("master_avatar_pass", "test");

            if (errorMessage != String.Empty)
            {
                // a error 
            }
        }

        public void loadConfigurationOptions()
        {
            configMember.addConfigurationOption("sim_UUID", ConfigurationOption.ConfigurationTypes.TYPE_LLUUID,
                                                "UUID of Region (Default is recommended, random UUID)",
                                                LLUUID.Random().ToString(), true);
            configMember.addConfigurationOption("sim_name", ConfigurationOption.ConfigurationTypes.TYPE_STRING_NOT_EMPTY,
                                                "Region Name", "OpenSim Test", false);
            configMember.addConfigurationOption("sim_location_x", ConfigurationOption.ConfigurationTypes.TYPE_UINT32,
                                                "Grid Location (X Axis)", "1000", false);
            configMember.addConfigurationOption("sim_location_y", ConfigurationOption.ConfigurationTypes.TYPE_UINT32,
                                                "Grid Location (Y Axis)", "1000", false);
            //configMember.addConfigurationOption("datastore", ConfigurationOption.ConfigurationTypes.TYPE_STRING_NOT_EMPTY, "Filename for local storage", "OpenSim.db", false);
            configMember.addConfigurationOption("internal_ip_address",
                                                ConfigurationOption.ConfigurationTypes.TYPE_IP_ADDRESS,
                                                "Internal IP Address for incoming UDP client connections", "0.0.0.0",
                                                false);
            configMember.addConfigurationOption("internal_ip_port", ConfigurationOption.ConfigurationTypes.TYPE_INT32,
                                                "Internal IP Port for incoming UDP client connections",
                                                NetworkServersInfo.DefaultHttpListenerPort.ToString(), false);
            configMember.addConfigurationOption("allow_alternate_ports", ConfigurationOption.ConfigurationTypes.TYPE_BOOLEAN,
                                                "Allow sim to find alternate UDP ports when ports are in use?",
                                                "false", true);
            configMember.addConfigurationOption("external_host_name",
                                                ConfigurationOption.ConfigurationTypes.TYPE_STRING_NOT_EMPTY,
                                                "External Host Name", "127.0.0.1", false);
            configMember.addConfigurationOption("master_avatar_uuid", ConfigurationOption.ConfigurationTypes.TYPE_LLUUID,
                                                "Master Avatar UUID", LLUUID.Zero.ToString(), true);
            configMember.addConfigurationOption("estate_covanant_uuid",
                                                ConfigurationOption.ConfigurationTypes.TYPE_LLUUID, "Estate Covenant",
                                                LLUUID.Zero.ToString(), true);
            configMember.addConfigurationOption("master_avatar_first",
                                                ConfigurationOption.ConfigurationTypes.TYPE_STRING_NOT_EMPTY,
                                                "First Name of Master Avatar", "Test", false,
                                                (ConfigurationOption.ConfigurationOptionShouldBeAsked)
                                                shouldMasterAvatarDetailsBeAsked);
            configMember.addConfigurationOption("master_avatar_last",
                                                ConfigurationOption.ConfigurationTypes.TYPE_STRING_NOT_EMPTY,
                                                "Last Name of Master Avatar", "User", false,
                                                (ConfigurationOption.ConfigurationOptionShouldBeAsked)
                                                shouldMasterAvatarDetailsBeAsked);
            configMember.addConfigurationOption("master_avatar_pass", ConfigurationOption.ConfigurationTypes.TYPE_STRING,
                                                "(Sandbox Mode Only)Password for Master Avatar account", "test", false,
                                                (ConfigurationOption.ConfigurationOptionShouldBeAsked)
                                                shouldMasterAvatarDetailsBeAsked);
        }

        public bool shouldMasterAvatarDetailsBeAsked(string configuration_key)
        {
            if (MasterAvatarAssignedUUID.Equals(null) || MasterAvatarAssignedUUID.ToString() == LLUUID.Zero.ToString())
            {
                return true;
            }
            return false;
        }

        public bool handleIncomingConfiguration(string configuration_key, object configuration_result)
        {
            switch (configuration_key)
            {
                case "sim_UUID":
                    RegionID = (LLUUID) configuration_result;
                    break;
                case "sim_name":
                    RegionName = (string) configuration_result;
                    break;
                case "sim_location_x":
                    m_regionLocX = (uint) configuration_result;
                    break;
                case "sim_location_y":
                    m_regionLocY = (uint) configuration_result;
                    break;
                case "datastore":
                    DataStore = (string) configuration_result;
                    break;
                case "internal_ip_address":
                    IPAddress address = (IPAddress) configuration_result;
                    m_internalEndPoint = new IPEndPoint(address, 0);
                    break;
                case "internal_ip_port":
                    m_internalEndPoint.Port = (int)configuration_result;
                    break;
                case "allow_alternate_ports":
                    m_allow_alternate_ports = (bool)configuration_result;
                    break;
                case "external_host_name":
                    if ((string) configuration_result != "SYSTEMIP")
                    {
                        m_externalHostName = (string) configuration_result;
                    }
                    else
                    {
                        m_externalHostName = Util.GetLocalHost().ToString();
                    }
                    break;
                case "master_avatar_uuid":
                    MasterAvatarAssignedUUID = (LLUUID) configuration_result;
                    break;
                case "estate_covanant_uuid":
                    CovenantID = (LLUUID) configuration_result;
                    break;

                case "master_avatar_first":
                    MasterAvatarFirstName = (string) configuration_result;
                    break;
                case "master_avatar_last":
                    MasterAvatarLastName = (string) configuration_result;
                    break;
                case "master_avatar_pass":
                    string tempMD5Passwd = (string) configuration_result;
                    MasterAvatarSandboxPassword = Util.Md5Hash(Util.Md5Hash(tempMD5Passwd) + ":" + String.Empty);
                    break;
            }

            return true;
        }
        public void SaveEstatecovenantUUID(LLUUID notecard) 
        {
            configMember.forceSetConfigurationOption("estate_covanant_uuid", notecard.ToString());

        }
    }
}