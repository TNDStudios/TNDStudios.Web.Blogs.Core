﻿using System;
using System.Collections.Generic;
using System.Linq;
using TNDStudios.Blogs.RequestResponse;

namespace TNDStudios.Blogs.Providers
{
    /// <summary>
    /// Provider for the blog using memory only
    /// </summary>
    public class BlogMemoryProvider : BlogDataProviderBase, IBlogDataProvider
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BlogMemoryProvider() : base()
        {
        }

        /// <summary>
        /// Initialise call made by the factory
        /// </summary>
        public override void Initialise()
        {
            // In-memory provider so always inialised
            items.Initialised = true;
        }
    }
}
