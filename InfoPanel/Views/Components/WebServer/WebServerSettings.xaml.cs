using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace InfoPanel.Views.Components.WebServer
{
    public sealed partial class WebServerSettings : Window
    {
        public WebServerSettings()
        {
            this.InitializeComponent();
            ComboBoxListenIp.Items.Add("127.0.0.1");
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    IPInterfaceProperties ipProps = ni.GetIPProperties();
                    foreach (IPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork 
                            && !addr.Address.ToString().StartsWith("169.254."))
                        {
                            ComboBoxListenIp.Items.Add(addr.Address.ToString());
                        }
                    }
                }
            }
        }
    }
}
