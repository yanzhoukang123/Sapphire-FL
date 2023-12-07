using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Azure.UpdateConfig
{
    class Program
    {
        //private static readonly string _CompanyName = "Azure Biosystems";
        //private static readonly string _ApplicationName = "Sapphire";
        private static string _InstallationType = string.Empty;
        private static string _InstallationModule = string.Empty;
        private static string _ConfigXmlPath = string.Empty;
        private static string _SysSettingsXmlPath = string.Empty;
        private static string _ProtocolsXmlPath = string.Empty;
        private static bool _IsPhosphorOnly = false;
        static int Main(string[] args)
        {
            // check if input arguments were supplied:
            if (args.Length == 0)
            {
                System.Console.WriteLine("Usage: Azure.UpdateConfig <<InstallationOption> <InstallationModule> <ConfigPath> <SysSettingsPath>");
                return 1;
            }

            _InstallationType = args[0];    // Selected installation type (RGB/RGBNIR/NIR/NIR-Q/PHOSPHOR-IMAGING)
            _InstallationModule = args[1];  // Selected module (Chemi/Phosphor/Chemi+Phosphor/None)
            _ConfigXmlPath = args[2];       // Full path to config.xml file to be modify
            _SysSettingsXmlPath = args[3];  // Full path to SysSettings.xml file to be modify
            _ProtocolsXmlPath = args[4];    // Full path to Protocols.xml file to be modify

            // Phosphor ONLY
            if (_InstallationType.ToUpper() == "PHOSPHOR-IMAGING")
            {
                _IsPhosphorOnly = true;
            }

            if (string.IsNullOrEmpty(_InstallationModule.Trim()))
            {
                _InstallationModule = "None";
            }

            UpdateConfigXml();

            return 0;

        }

        /// <summary>
        /// Update config.xml file in ProgramData
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="filterPath"></param>
        /// <returns>returns true if the config file is modified</returns>
        static bool UpdateConfigXml()
        {
            bool bRetVal = false;

            if (!File.Exists(_ConfigXmlPath))
            {
                //Console.WriteLine("File doesn't exist");
                bRetVal = false;
            }
            else
            {
                if (!_IsPhosphorOnly)
                {
                    bRetVal = UpdateDyeTypes(_ConfigXmlPath);
                }
            }

            if (!File.Exists(_SysSettingsXmlPath))
            {
                //Console.WriteLine("File doesn't exist");
                bRetVal = false;
            }
            else
            {
                bRetVal = UpdateImagingTabs(_SysSettingsXmlPath);
                if (!_IsPhosphorOnly)
                {
                    bRetVal = AppendLaserTypes(_SysSettingsXmlPath);
                }
                UpdateChemiSettings(_SysSettingsXmlPath);
            }

            if (!File.Exists(_ProtocolsXmlPath))
            {
                //Console.WriteLine("File doesn't exist");
                bRetVal = false;
            }
            else
            {
                if (!_IsPhosphorOnly)
                {
                    bRetVal = UpdateProtocols(_ProtocolsXmlPath);
                }
            }

            return bRetVal;
        }

        static bool UpdateDyeTypes(string configPath)
        {
            bool bResult = false;
            bool bSaveDoc = false;

            XmlDocument xdoc = new XmlDocument();
            xdoc.PreserveWhitespace = true;
            xdoc.Load(configPath);

            XmlNodeList lightElements = xdoc.GetElementsByTagName("Dye");

            if (lightElements != null)
            {
                XmlNode commentNode = null;
                for (int i = lightElements.Count - 1; i >= 0; i--)
                {
                    if (lightElements[i].NodeType == XmlNodeType.Element)
                    {
                        string attrLaserName = lightElements[i].Attributes["Laser"].Value;

                        if (_InstallationType.ToUpper() == "RGB")
                        {
                            // No wavelength: 784
                            if (attrLaserName.Equals("LaserA"))
                            {
                                try
                                {
                                    commentNode = xdoc.CreateComment(lightElements[i].OuterXml);
                                    lightElements[i].ParentNode.ReplaceChild(commentNode, lightElements[i]);
                                    bSaveDoc = true;
                                }
                                catch
                                {
                                    bResult = false;
                                    bSaveDoc = false;
                                }
                            }
                        }
                        else if (_InstallationType.ToUpper() == "NIR")
                        {
                            // No wavelength: 488/520
                            if (attrLaserName.Equals("LaserD") || attrLaserName.Equals("LaserB"))
                            {
                                try
                                {
                                    commentNode = xdoc.CreateComment(lightElements[i].OuterXml);
                                    lightElements[i].ParentNode.ReplaceChild(commentNode, lightElements[i]);
                                    bSaveDoc = true;
                                }
                                catch
                                {
                                    bResult = false;
                                    bSaveDoc = false;
                                }
                            }
                        }
                        else if (_InstallationType.ToUpper() == "NIR-Q")
                        {
                            // No wavelength: 488
                            if (attrLaserName.Equals("LaserD"))
                            {
                                try
                                {
                                    commentNode = xdoc.CreateComment(lightElements[i].OuterXml);
                                    lightElements[i].ParentNode.ReplaceChild(commentNode, lightElements[i]);
                                    bSaveDoc = true;
                                }
                                catch
                                {
                                    bResult = false;
                                    bSaveDoc = false;
                                }
                            }
                        }
                        else if (_InstallationType.ToUpper() == "PHOSPHOR-IMAGING")
                        {
                            // No wavelength: 784/520
                            /*if (attrLaserName.Equals("LaserA") || attrLaserName.Equals("LaserB"))
                            {
                                try
                                {
                                    commentNode = xdoc.CreateComment(lightElements[i].OuterXml);
                                    lightElements[i].ParentNode.ReplaceChild(commentNode, lightElements[i]);
                                    bSaveDoc = true;
                                }
                                catch
                                {
                                    bResult = false;
                                    bSaveDoc = false;
                                }
                            }*/
                        }
                    }
                }

                // save changes to file 
                if (bSaveDoc)
                {
                    try
                    {
                        xdoc.Save(configPath);
                        bResult = true;
                    }
                    catch (Exception e)
                    {
                        bResult = false;
                        Console.WriteLine("Updating config.xml error: {0}", e.Message);
                    }
                }
            }

            return bResult;
        }

        static bool UpdateImagingTabs(string configPath)
        {
            bool bResult = false;
            bool bSaveDoc = false;
            bool bIsFluorescenceVisible = false;
            bool bIsChemiVisible = false;
            // Checking whether the imaging tabs exists on the SysSettings.xml
            bool bIsPhosphorExists = false;
            bool bIsFluorescenceExists = false;
            bool bIsChemiExists = false;
            bool bIsVisibleExists = false;

            // Phosphor ONLY
            if (_IsPhosphorOnly)
            {
                bIsFluorescenceVisible = false;
            }
            else
            {
                // RGB, NIR or NIR-Q selected
                if (_InstallationType.ToUpper() == "RGB" ||
                    _InstallationType.ToUpper() == "NIR" ||
                    _InstallationType.ToUpper() == "RGBNIR" ||
                    _InstallationType.ToUpper() == "NIR-Q")
                {
                    bIsFluorescenceVisible = true;
                }
            }

            if (_InstallationModule.Equals("None", StringComparison.InvariantCultureIgnoreCase))
            {
                bIsChemiVisible = false;
            }
            else if (_InstallationModule.Equals("Chemi", StringComparison.InvariantCultureIgnoreCase))
            {
                bIsChemiVisible = true;
            }

            // Update SysSettings.xml file
            //
            XmlDocument xdoc = new XmlDocument();

            try
            {
                xdoc.PreserveWhitespace = false;
                xdoc.Load(configPath);

                XmlNodeList imagingTabElements = xdoc.GetElementsByTagName("ImagingTab");

                if (imagingTabElements != null)
                {
                    if (_IsPhosphorOnly)
                    {
                        for (int i = imagingTabElements.Count - 1; i >= 0; i--)
                        {
                            string attrImagingType = imagingTabElements[i].Attributes["ImagingType"].Value;
                            if (attrImagingType.ToLower().Contains("phosphor"))
                            {
                                if (Convert.ToBoolean(imagingTabElements[i].Attributes["IsVisible"].Value) == false)
                                {
                                    imagingTabElements[i].Attributes["IsVisible"].Value = "True";
                                    bSaveDoc = true;
                                }
                                bIsPhosphorExists = true;
                            }
                            else
                            {
                                if (attrImagingType.ToLower().Contains("chemi") ||
                                    attrImagingType.ToLower().Contains("visible"))
                                {
                                    if (bIsChemiVisible)
                                    {
                                        imagingTabElements[i].Attributes["IsVisible"].Value = "True";       // Show imaging tab
                                        bSaveDoc = true;
                                    }
                                    else
                                    {
                                        imagingTabElements[i].Attributes["IsVisible"].Value = "False";      // Hide imaging tab
                                        bSaveDoc = true;
                                    }
                                }
                                else if (attrImagingType.ToLower().Contains("fluorescence"))
                                {
                                    imagingTabElements[i].ParentNode.RemoveChild(imagingTabElements[i]);    // Remove imaging tab
                                    bSaveDoc = true;
                                }
                            }
                        }

                        if (!bIsPhosphorExists)
                        {
                            XmlNodeList imagingTabsNode = xdoc.GetElementsByTagName("ImagingTabs");
                            //Create a new attrtibute.
                            var imagingTabElem = xdoc.CreateElement("ImagingTab");
                            //imagingTabsNode[0].AppendChild(imagingTabElem);
                            imagingTabsNode[0].InsertBefore(imagingTabElem, imagingTabsNode[0].FirstChild);
                            var xmlAttrib = xdoc.CreateAttribute("ImagingType");
                            xmlAttrib.Value = "PhosphorImaging";
                            imagingTabElem.Attributes.Append(xmlAttrib);
                            xmlAttrib = xdoc.CreateAttribute("IsVisible");
                            xmlAttrib.Value = "True";
                            imagingTabElem.Attributes.Append(xmlAttrib);
                            bSaveDoc = true;
                        }
                    }
                    else
                    {
                        for (int i = imagingTabElements.Count - 1; i >= 0; i--)
                        {
                            if (imagingTabElements[i].NodeType == XmlNodeType.Element)
                            {
                                string attrImagingType = imagingTabElements[i].Attributes["ImagingType"].Value;
                                if (attrImagingType.Equals("Fluorescence", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    bIsFluorescenceExists = true;
                                    if (bIsFluorescenceVisible)
                                    {
                                        if (Convert.ToBoolean(imagingTabElements[i].Attributes["IsVisible"].Value) == false)
                                        {
                                            imagingTabElements[i].Attributes["IsVisible"].Value = "True";
                                            bSaveDoc = true;
                                        }
                                    }
                                    else
                                    {
                                        if (Convert.ToBoolean(imagingTabElements[i].Attributes["IsVisible"].Value) == true)
                                        {
                                            imagingTabElements[i].Attributes["IsVisible"].Value = "False";
                                            bSaveDoc = true;
                                        }
                                    }
                                }
                                else if (attrImagingType.Equals("Chemiluminescence", StringComparison.InvariantCultureIgnoreCase) ||
                                        attrImagingType.Equals("Visible", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (attrImagingType.Equals("Visible", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        bIsVisibleExists = true;
                                        if (bIsChemiVisible)
                                        {
                                            if (Convert.ToBoolean(imagingTabElements[i].Attributes["IsVisible"].Value) == false)
                                            {
                                                imagingTabElements[i].Attributes["IsVisible"].Value = "True";
                                                bSaveDoc = true;
                                            }
                                        }
                                        else
                                        {
                                            if (Convert.ToBoolean(imagingTabElements[i].Attributes["IsVisible"].Value) == true)
                                            {
                                                imagingTabElements[i].Attributes["IsVisible"].Value = "False";
                                                bSaveDoc = true;
                                            }
                                        }
                                    }
                                    else if (attrImagingType.Equals("Chemiluminescence", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        bIsChemiExists = true;
                                        if (bIsChemiVisible)
                                        {
                                            if (Convert.ToBoolean(imagingTabElements[i].Attributes["IsVisible"].Value) == false)
                                            {
                                                imagingTabElements[i].Attributes["IsVisible"].Value = "True";
                                                bSaveDoc = true;
                                            }
                                        }
                                        else
                                        {
                                            if (Convert.ToBoolean(imagingTabElements[i].Attributes["IsVisible"].Value) == true)
                                            {
                                                imagingTabElements[i].Attributes["IsVisible"].Value = "False";
                                                bSaveDoc = true;
                                            }
                                        }
                                    }
                                }
                                else if (attrImagingType.Equals("PhosphorImaging", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (bIsFluorescenceVisible)
                                    {
                                        imagingTabElements[i].ParentNode.RemoveChild(imagingTabElements[i]);  // Remove imaging tab
                                    }
                                }
                            }
                        }

                        // Add imaging tab(s) if doesn't exist.
                        //
                        // Insert as first item (before first child)
                        if (!bIsFluorescenceExists && bIsFluorescenceVisible)
                        {
                            XmlNodeList imagingTabsNode = xdoc.GetElementsByTagName("ImagingTabs");
                            //Create a new attrtibute.
                            var imagingTabElem = xdoc.CreateElement("ImagingTab");
                            //imagingTabsNode[0].AppendChild(imagingTabElem);
                            imagingTabsNode[0].InsertBefore(imagingTabElem, imagingTabsNode[0].FirstChild);
                            var xmlAttrib = xdoc.CreateAttribute("ImagingType");
                            xmlAttrib.Value = "Fluorescence";
                            imagingTabElem.Attributes.Append(xmlAttrib);
                            xmlAttrib = xdoc.CreateAttribute("IsVisible");
                            xmlAttrib.Value = "True";
                            imagingTabElem.Attributes.Append(xmlAttrib);
                            bSaveDoc = true;
                        }
                        if (!bIsChemiExists && bIsChemiVisible)
                        {
                            XmlNodeList imagingTabsNode = xdoc.GetElementsByTagName("ImagingTabs");
                            //Create a new attrtibute.
                            var imagingTabElem = xdoc.CreateElement("ImagingTab");
                            imagingTabsNode[0].AppendChild(imagingTabElem);
                            var xmlAttrib = xdoc.CreateAttribute("ImagingType");
                            xmlAttrib.Value = "Chemiluminescence";
                            imagingTabElem.Attributes.Append(xmlAttrib);
                            xmlAttrib = xdoc.CreateAttribute("IsVisible");
                            xmlAttrib.Value = "True";
                            imagingTabElem.Attributes.Append(xmlAttrib);
                            bSaveDoc = true;
                        }
                        if (!bIsVisibleExists && bIsChemiVisible)
                        {
                            XmlNodeList imagingTabsNode = xdoc.GetElementsByTagName("ImagingTabs");
                            //Create a new attrtibute.
                            var imagingTabElem = xdoc.CreateElement("ImagingTab");
                            imagingTabsNode[0].AppendChild(imagingTabElem);
                            var xmlAttrib = xdoc.CreateAttribute("ImagingType");
                            xmlAttrib.Value = "Visible";
                            imagingTabElem.Attributes.Append(xmlAttrib);
                            xmlAttrib = xdoc.CreateAttribute("IsVisible");
                            xmlAttrib.Value = "True";
                            imagingTabElem.Attributes.Append(xmlAttrib);
                            bSaveDoc = true;
                        }
                    }   //closing: else
                }
            }
            catch (Exception)
            {
                bResult = false;
                bSaveDoc = false;
            }

            if (bSaveDoc)
            {
                try
                {
                    xdoc.Beautify();
                    xdoc.Save(configPath);
                    bResult = true;
                }
                catch (Exception e)
                {
                    bResult = false;
                    Console.WriteLine("Updating config.xml error: {0}", e.Message);
                }

            }

            return bResult;
        }

        static bool UpdateProtocols(string configPath)
        {
            bool bResult = false;
            bool bSaveDoc = false;

            if (_InstallationType.ToUpper() == "RGB" ||
                _InstallationType.ToUpper() == "NIR" ||
                _InstallationType.ToUpper() == "NIR-Q")
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.PreserveWhitespace = true;
                xdoc.Load(configPath);

                XmlNodeList protocolElements = xdoc.GetElementsByTagName("Protocol");

                if (protocolElements != null)
                {
                    XmlNodeList commentedNodes = null;
                    XmlNode commentNode = null;
                    for (int i = protocolElements.Count - 1; i >= 0 ; i--)
                    {
                        if (protocolElements[i].NodeType == XmlNodeType.Element)
                        {
                            string attrDisplayName = protocolElements[i].Attributes["DisplayName"].Value;

                            // Comment out "4 Channel Western" or " 4-Channel Fluorescence" protocol
                            if (attrDisplayName.IndexOf("4 Channel", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                attrDisplayName.IndexOf("4-Channel", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                try
                                {
                                    commentNode = xdoc.CreateComment(protocolElements[i].OuterXml);
                                    protocolElements[i].ParentNode.ReplaceChild(commentNode, protocolElements[i]);
                                    bSaveDoc = true;
                                    continue;
                                }
                                catch
                                {
                                    bResult = false;
                                    bSaveDoc = false;
                                }
                            }

                            if (_InstallationType.ToUpper() == "RGB")
                            {
                                // Comment out "NIR Western Blot" or "NIR Fluorescence" protocol
                                if (attrDisplayName.IndexOf("NIR Western Blot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    attrDisplayName.IndexOf("NIR Fluorescence", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    try
                                    {
                                        commentNode = xdoc.CreateComment(protocolElements[i].OuterXml);
                                        protocolElements[i].ParentNode.ReplaceChild(commentNode, protocolElements[i]);
                                        bSaveDoc = true;
                                    }
                                    catch
                                    {
                                        bResult = false;
                                        bSaveDoc = false;
                                    }
                                }
                            }
                            else if (_InstallationType.ToUpper() == "NIR" ||
                                _InstallationType.ToUpper() == "NIR-Q")
                            {
                                // Comment out "Visible Fluorescent Western" or "RGB Fluorescence" protocol (for NIR & NIR-Q)
                                if (attrDisplayName.IndexOf("Visible Fluorescent Western", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    attrDisplayName.IndexOf("RGB Fluorescence", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    try
                                    {
                                        commentNode = xdoc.CreateComment(protocolElements[i].OuterXml);
                                        protocolElements[i].ParentNode.ReplaceChild(commentNode, protocolElements[i]);
                                        bSaveDoc = true;
                                    }
                                    catch
                                    {
                                        bResult = false;
                                        bSaveDoc = false;
                                    }
                                }
                                else if (attrDisplayName.IndexOf("Densitometry Plate", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                    _InstallationType.ToUpper() == "NIR")
                                {
                                    // Comment out "Densitometry Plate" protocol (for NIR, 'Densitometry Plate' required LaserB/Green laser)
                                    try
                                    {
                                        commentNode = xdoc.CreateComment(protocolElements[i].OuterXml);
                                        protocolElements[i].ParentNode.ReplaceChild(commentNode, protocolElements[i]);
                                        bSaveDoc = true;
                                    }
                                    catch
                                    {
                                        bResult = false;
                                        bSaveDoc = false;
                                    }
                                }
                            }
                        }
                    }

                    if (_InstallationType.ToUpper() == "RGB")
                    {
                        // Uncomment RGB's default protocol
                        //
                        commentedNodes = xdoc.SelectNodes("//comment()");
                        //commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                        //               where comment.Value.Contains("Visible Fluorescent Western Blot")
                        //               select comment).FirstOrDefault();
                        //if (commentNode != null)
                        //{
                        //    XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                        //    XmlNode newNode = xdoc.ReadNode(nodeReader);
                        //    commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                        //    bSaveDoc = true;
                        //}
                        commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                                       where comment.Value.Contains("RGB Fluorescence")
                                       select comment).FirstOrDefault();
                        if (commentNode != null)
                        {
                            XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                            XmlNode newNode = xdoc.ReadNode(nodeReader);
                            commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                            bSaveDoc = true;
                        }
                        commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                                       where comment.Value.Contains("Densitometry Plate")
                                       select comment).FirstOrDefault();
                        if (commentNode != null)
                        {
                            XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                            XmlNode newNode = xdoc.ReadNode(nodeReader);
                            commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                            bSaveDoc = true;
                        }
                    }

                    if (_InstallationType.ToUpper() == "NIR" ||
                        _InstallationType.ToUpper() == "NIR-Q")
                    {
                        // Uncomment NIR's default protocol
                        //
                        commentedNodes = xdoc.SelectNodes("//comment()");
                        //commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                        //               where comment.Value.Contains("NIR Western Blot")
                        //               select comment).FirstOrDefault();
                        //if (commentNode != null)
                        //{
                        //    XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                        //    XmlNode newNode = xdoc.ReadNode(nodeReader);
                        //    commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                        //    bSaveDoc = true;
                        //}
                        commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                                       where comment.Value.Contains("NIR Fluorescence")
                                       select comment).FirstOrDefault();
                        if (commentNode != null)
                        {
                            XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                            XmlNode newNode = xdoc.ReadNode(nodeReader);
                            commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                            bSaveDoc = true;
                        }

                        if (_InstallationType.ToUpper() == "NIR-Q")
                        {
                            commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                                           where comment.Value.Contains("Densitometry Plate")
                                           select comment).FirstOrDefault();
                            if (commentNode != null)
                            {
                                XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                                XmlNode newNode = xdoc.ReadNode(nodeReader);
                                commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                                bSaveDoc = true;
                            }
                        }
                    }

                    // save changes to file 
                    if (bSaveDoc)
                    {
                        try
                        {
                            xdoc.Beautify();
                            xdoc.Save(configPath);
                            bResult = true;
                        }
                        catch (Exception e)
                        {
                            bResult = false;
                            Console.WriteLine("Updating config.xml error: {0}", e.Message);
                        }
                    }
                }
            }
            else
            {
                /*var doc = XElement.Load(configPath);
                var comments = doc.DescendantNodes().OfType<XComment>();

                if (comments != null)
                {
                    /*foreach (XComment comment in comments)
                    {
                        if (comment.Value.IndexOf("4 Channel Western", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            comment.Remove();
                            bSaveDoc = true;
                            break;
                       }
                    }
                }
                */

                XmlDocument xdoc = new XmlDocument();
                xdoc.PreserveWhitespace = true;
                xdoc.Load(configPath);

                XmlNodeList commentedNodes = xdoc.SelectNodes("//comment()");
                //var commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                //                   where comment.Value.Contains("4 Channel Western")
                //                   select comment).FirstOrDefault();
                //if (commentNode != null)
                //{
                //    XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                //    XmlNode newNode = xdoc.ReadNode(nodeReader);
                //    commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                //    bSaveDoc = true;
                //}
                var commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                                   where comment.Value.Contains("4-Channel Fluorescence")
                                   select comment).FirstOrDefault();
                if (commentNode != null)
                {
                    XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                    XmlNode newNode = xdoc.ReadNode(nodeReader);
                    commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                    bSaveDoc = true;
                }
                commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                               where comment.Value.Contains("RGB Fluorescence")
                               select comment).FirstOrDefault();
                if (commentNode != null)
                {
                    XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                    XmlNode newNode = xdoc.ReadNode(nodeReader);
                    commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                    bSaveDoc = true;
                }
                commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                               where comment.Value.Contains("NIR Fluorescence")
                               select comment).FirstOrDefault();
                if (commentNode != null)
                {
                    XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                    XmlNode newNode = xdoc.ReadNode(nodeReader);
                    commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                    bSaveDoc = true;
                }
                commentNode = (from comment in commentedNodes.Cast<XmlNode>()
                               where comment.Value.Contains("Densitometry Plate")
                               select comment).FirstOrDefault();
                if (commentNode != null)
                {
                    XmlReader nodeReader = XmlReader.Create(new StringReader(commentNode.Value));
                    XmlNode newNode = xdoc.ReadNode(nodeReader);
                    commentNode.ParentNode.ReplaceChild(newNode, commentNode);
                    bSaveDoc = true;
                }

                // save changes to file 
                if (bSaveDoc)
                {
                    try
                    {
                        /*StringBuilder sb = new StringBuilder();
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "  ",
                            NewLineChars = "\r\n",
                            NewLineHandling = NewLineHandling.Replace
                        };
                        using (XmlWriter writer = XmlWriter.Create(sb, settings))
                        {
                            xdoc.Save(writer);
                        }*/

                        xdoc.Save(configPath);
                        bResult = true;
                    }
                    catch (Exception e)
                    {
                        bResult = false;
                        Console.WriteLine("Updating config.xml error: {0}", e.Message);
                    }
                }
            }

            return bResult;
        }

        static bool AppendLaserTypes(string configPath)
        {
            bool bResult = false;

            try
            {
                XmlDocument xdoc = new XmlDocument();
                //xdoc.PreserveWhitespace = true; //this flag override auto indent settings
                xdoc.Load(configPath);

                XmlNode root = xdoc.DocumentElement;
                var nodeToFind = root.SelectSingleNode("LaserTypes");
                if (nodeToFind == null)
                {
                    //Add laser types section
                    //
                    //<LaserTypes >
                    //  <LaserType Laser = "LaserA" WaveLength = "784 / 832BP37"/>
                    //  <LaserType Laser = "LaserB" WaveLength = "520 / 565BP24"/>
                    //  <LaserType Laser = "LaserC" WaveLength = "658 / 710BP40"/>
                    //  <LaserType Laser = "LaserD" WaveLength = "488 / 518BP22"/>
                    //</LaserTypes >

                    XmlNodeList sampleTypesNode = xdoc.GetElementsByTagName("SampleTypes");

                    var laserTypesElem = xdoc.CreateElement("LaserTypes");
                    //root.AppendChild(laserTypesElem);
                    // Insert LaserTypes after SampleTypes
                    root.InsertAfter(laserTypesElem, sampleTypesNode.Item(0));

                    //Create a new attrtibute.
                    var laserElem = xdoc.CreateElement("LaserType");
                    laserTypesElem.AppendChild(laserElem);
                    var xmlAttrib = xdoc.CreateAttribute("Laser");
                    xmlAttrib.Value = "LaserA";
                    laserElem.Attributes.Append(xmlAttrib);
                    xmlAttrib = xdoc.CreateAttribute("WaveLength");
                    xmlAttrib.Value = "784 / 832BP37";
                    laserElem.Attributes.Append(xmlAttrib);
                    //Create a new attrtibute.
                    laserElem = xdoc.CreateElement("LaserType");
                    laserTypesElem.AppendChild(laserElem);
                    xmlAttrib = xdoc.CreateAttribute("Laser");
                    xmlAttrib.Value = "LaserB";
                    laserElem.Attributes.Append(xmlAttrib);
                    xmlAttrib = xdoc.CreateAttribute("WaveLength");
                    xmlAttrib.Value = "520 / 565BP24";
                    laserElem.Attributes.Append(xmlAttrib);
                    //Create a new attrtibute.
                    laserElem = xdoc.CreateElement("LaserType");
                    laserTypesElem.AppendChild(laserElem);
                    xmlAttrib = xdoc.CreateAttribute("Laser");
                    xmlAttrib.Value = "LaserC";
                    laserElem.Attributes.Append(xmlAttrib);
                    xmlAttrib = xdoc.CreateAttribute("WaveLength");
                    xmlAttrib.Value = "658 / 710BP40";
                    laserElem.Attributes.Append(xmlAttrib);
                    //Create a new attrtibute.
                    laserElem = xdoc.CreateElement("LaserType");
                    laserTypesElem.AppendChild(laserElem);
                    xmlAttrib = xdoc.CreateAttribute("Laser");
                    xmlAttrib.Value = "LaserD";
                    laserElem.Attributes.Append(xmlAttrib);
                    xmlAttrib = xdoc.CreateAttribute("WaveLength");
                    xmlAttrib.Value = "488 / 518BP22";
                    laserElem.Attributes.Append(xmlAttrib);

                    xdoc.Beautify();
                    xdoc.Save(configPath);

                    bResult = true;
                }
            }
            catch (Exception e)
            {
                bResult = false;
                Console.WriteLine("Updating SysSettings.xml error: {0}", e.Message);
            }

            return bResult;
        }

        /// <summary>
        /// Add the element: <InitialAutoExposureTime Value = "0.050" />
        /// if it doesn't exists.
        /// </summary>
        /// <param name="configPath"></param>
        static void UpdateChemiSettings(string configPath)
        {
            XmlDocument xdoc = new XmlDocument();
            //xdoc.PreserveWhitespace = true; //this flag override auto indent settings
            xdoc.Load(configPath);

            XmlNode root = xdoc.DocumentElement;
            bool bElemFound = false;
            var parentNode = root.SelectSingleNode("Chemi");
            if (parentNode != null)
            {
                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        if (node.Name.Equals("InitialAutoExposureTime", StringComparison.InvariantCultureIgnoreCase))
                        {
                            bElemFound = true;
                            break;
                        }
                    }
                }

                if (!bElemFound)
                {
                    var childElem = xdoc.CreateElement("InitialAutoExposureTime");
                    parentNode.AppendChild(childElem);
                    var xmlAttrib = xdoc.CreateAttribute("Value");
                    xmlAttrib.Value = "0.050";
                    childElem.Attributes.Append(xmlAttrib);
                    /// Add the node to the document.
                    parentNode.InsertBefore(childElem, parentNode.FirstChild);

                    xdoc.Beautify();
                    xdoc.Save(configPath);
                }
            }
        }

        /*public static void RemoveWithNextWhitespace(this XElement element)
        {
            IEnumerable<XText> textNodes
                = element.NodesAfterSelf()
                         .TakeWhile(node => node is XText).Cast<XText>();
            if (element.ElementsAfterSelf().Any())
            {
                // Easy case, remove following text nodes.
                textNodes.ToList().ForEach(node => node.Remove());
            }
            else
            {
                // Remove trailing whitespace.
                textNodes.TakeWhile(text => !text.Value.Contains("\n"))
                         .ToList().ForEach(text => text.Remove());
                // Fetch text node containing newline, if any.
                XText newLineTextNode
                    = element.NodesAfterSelf().OfType<XText>().FirstOrDefault();
                if (newLineTextNode != null)
                {
                    string value = newLineTextNode.Value;
                    if (value.Length > 1)
                    {
                        // Composite text node, trim until newline (inclusive).
                        newLineTextNode.AddAfterSelf(
                            new XText(value.Substring(value.IndexOf('\n') + 1)));
                    }
                    // Remove original node.
                    newLineTextNode.Remove();
                }
            }
            element.Remove();
        }*/

        /*public static XmlNodeList Scan(XmlNodeList nodeList)
        {
            List<XmlNode> toRemove = new List<XmlNode>();

            foreach (XmlNode xmlElement in nodeList)
            {
                string elementValue = xmlElement.OuterXml;
                if (elementValue.ToLower().Contains("rgb"))
                {
                    toRemove.Add(xmlElement);
                }
            }

            foreach (XmlNode xmlElement in toRemove)
            {
                XmlNode node = xmlElement.ParentNode;
                node.RemoveChild(xmlElement);
            }

            foreach (XmlNode xmlElement in nodeList)
            {
                string elementValue = xmlElement.OuterXml;
                if (string.IsNullOrWhiteSpace(elementValue))
                {
                    XmlNode node = xmlElement.ParentNode;
                    node.RemoveChild(xmlElement);
                }
            }

            return nodeList;
        }*/

        /*static string Beautify(this XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }*/

    }

    public static class XmlDocExtension
    {
        public static string ToIndentedString(this XmlDocument doc)
        {
            var stringWriter = new System.IO.StringWriter(new StringBuilder());
            var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented, IndentChar = ' ' };
            doc.Save(xmlTextWriter);
            return stringWriter.ToString();
        }

        public static string Beautify(this XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }

    }

}
