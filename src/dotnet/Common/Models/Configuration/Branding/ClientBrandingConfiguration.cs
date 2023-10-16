﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Configuration.Branding
{
    /// <summary>
    /// Provides configuration for client branding.
    /// </summary>
    public class ClientBrandingConfiguration
    {
        /// <summary>
        /// The display name of the client.
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// The URL of the client's favicon.
        /// </summary>
        public string FavIconUrl { get; set; }
        /// <summary>
        /// The URL of the client's logo.
        /// </summary>
        public string LogoUrl { get; set; }
        /// <summary>
        /// The text to display next to the logo in the nav pane. Leave blank to not display any text.
        /// </summary>
        public string LogoText { get; set; }
        /// <summary>
        /// The primary color of the client in hex format.
        /// </summary>
        public string PrimaryColor { get; set; }
        /// <summary>
        /// The secondary color of the client in hex format.
        /// </summary>
        public string SecondaryColor { get; set; }
        /// <summary>
        /// The accent color of the client in hex format.
        /// </summary>
        public string AccentColor { get; set; }
        /// <summary>
        /// The background color of the client in hex format.
        /// </summary>
        public string BackgroundColor { get; set; }
    }
}
