﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TNDStudios.Blogs
{
    /// <summary>
    /// Implementation of the blog index
    /// </summary>
    public class BlogIndex : BlogBase, IBlogIndex
    {
        /// <summary>
        /// List of the headers
        /// </summary>
        public List<IBlogItem> Headers { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public BlogIndex()
        {
            // Empty by default (some list functions assume it is instantiated)
            Headers = new List<IBlogItem>();
        }
    }
}
