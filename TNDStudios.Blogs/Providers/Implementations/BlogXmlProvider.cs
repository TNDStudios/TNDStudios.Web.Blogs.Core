﻿using System;
using System.Collections.Generic;
using System.Linq;
using TNDStudios.Blogs.RequestResponse;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace TNDStudios.Blogs.Providers
{
    /// <summary>
    /// Provider for the blog using Xml files in the App_Data (Or other) folder
    /// </summary>
    public class BlogXmlProvider : BlogDataProviderBase, IBlogDataProvider
    {
        /// <summary>
        /// Constants for the filenames that need to be saved
        /// </summary>
        private const String indexXmlFilename = "index.xml";
        private const String blogItemXmlFilename = "{0}.xml";
        private const String blogItemFolder = "blogsitems";

        /// <summary>
        /// Load the item from disk
        /// </summary>
        /// <param name="request">The header of the item that is to be loaded</param>
        /// <returns>The blog item that was found</returns>
        public override IBlogItem Load(IBlogHeader request)
        {
            return base.Load(request);
        }

        /// <summary>
        /// Save a blog item
        /// </summary>
        /// <param name="item">The item to be saved</param>
        /// <returns>The item once it has been saved</returns>
        public override IBlogItem Save(IBlogItem item)
        {
            // The base class will save the item to the in-memory header
            // so we don't want to pass the content in to this. We only want to save 
            // the content to the file, so copy and update the item without the content
            // to the index
            IBlogItem headerRecord = item.Duplicate();
            headerRecord.Content = ""; // Remove the content from being saved to the header record
            IBlogItem response = base.Save(headerRecord);

            // Successfully saved?
            if (response != null && response.Header != null && response.Header.Id != "")
            {
                // Write the blog item to disk
                if (WriteBlogItem(response))
                {
                    // Try and save the header records to disk so any updates are cached there too
                    if (WriteBlogIndex())
                        return response;
                }
            }

            // Got to here so must have failed
            throw new CouldNotSaveBlogException();
        }

        /// <summary>
        /// Write the index to disk so it can be retrieved later
        /// </summary>
        private Boolean WriteBlogIndex()
            => Write<BlogIndex>(
                Path.Combine(
                    this.ConnectionString.Property("path"), indexXmlFilename
                    ),
                    this.items
                );

        /// <summary>
        /// Read the index from disk
        /// </summary>
        private BlogIndex ReadBlogIndex()
            => Read<BlogIndex>(
                Path.Combine(
                    this.ConnectionString.Property("path"), indexXmlFilename
                    )
                );

        /// <summary>
        /// Write a blog item to disk so it can be retrieved later
        /// </summary>
        /// <param name="blogItem">The Blog Item to be saved</param>
        private Boolean WriteBlogItem(IBlogItem blogItem)
            => Write<IBlogItem>(
                Path.Combine(
                    this.ConnectionString.Property("path"), blogItemFolder, String.Format(blogItemXmlFilename, blogItem.Header.Id)
                    ),
                    blogItem
                );

        /// <summary>
        /// Load the item from disk
        /// </summary>
        /// <param name="request">The header of the item we want to load from disk</param>
        /// <returns>The cast blog item</returns>
        private IBlogItem ReadBlogItem(IBlogHeader request)
            => Read<BlogItem>(
                Path.Combine(
                    this.ConnectionString.Property("path"), blogItemFolder, String.Format(blogItemXmlFilename, request.Id)
                    ));

        /// <summary>
        /// Write an item to disk with a given path from a given object type
        /// </summary>
        /// <typeparam name="T">The object type to be written to disk</typeparam>
        /// <param name="path">The path to write the item to</param>
        private Boolean Write<T>(String path, T toWrite) where T : IBlogBase
        {
            try
            {
                // Calculate the relative directory based on the path
                String combinedPath = Path.Combine(Configuration.Environment.WebRootPath, path);

                // Get the filename from the combined path
                String fileName = Path.GetFileName(combinedPath);

                // Get the directory alone from the combined path
                String pathAlone = (fileName != "") ? Path.GetDirectoryName(combinedPath) : combinedPath;

                // Check to make sure the directory exists
                if (!Directory.Exists(pathAlone))
                    Directory.CreateDirectory(pathAlone);

                // Write the Xml to disk
                File.WriteAllText(combinedPath, toWrite.ToXmlString());

                // Check if the file exists after the write
                return File.Exists(combinedPath);
            }
            catch (Exception ex)
            {
                // Throw that the file could not be saved
                throw BlogException.Passthrough(ex, new CouldNotSaveBlogException(ex));
            }
        }

        /// <summary>
        /// Read an item from disk with a given path from a given object type
        /// </summary>
        /// <typeparam name="T">The object type to be read from disk</typeparam>
        /// <param name="path">The path to read the item from</param>
        private T Read<T>(String path) where T : IBlogBase, new()
        {
            try
            {
                // Calculate the relative directory based on the path
                String combinedPath = Path.Combine(Configuration.Environment.WebRootPath, path);

                // Get the filename from the combined path
                String fileName = Path.GetFileName(combinedPath);

                // Get the directory alone from the combined path
                String pathAlone = (fileName != "") ? Path.GetDirectoryName(combinedPath) : combinedPath;
                
                // Write the Xml to disk
                String XmlString = File.ReadAllText(combinedPath);

                // Generate a new object
                T result = new T();

                // Populate the object by calling the extension method to cast the incoming string
                result.FromXmlString<T>(XmlString);

                // Send the result back
                return result;
            }
            catch (Exception ex)
            {
                // Throw that the file could not be saved
                throw BlogException.Passthrough(ex, new CouldNotLoadBlogException(ex));
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BlogXmlProvider() : base()
        {
        }

        /// <summary>
        /// Initialise call made by the factory
        /// </summary>
        public override void Initialise()
        {
            // Check to see if there is an a file containing the index to load to initialise the blog
            try
            {
                BlogIndex foundIndex = ReadBlogIndex();
                items = foundIndex;
                items.Initialised = true;
            }
            catch(Exception ex)
            {
                // Could not load error?
                if (ex.GetType() == typeof(CouldNotLoadBlogException))
                {
                    // File was not found so create a blank index file
                    if (ex.InnerException != null && ex.InnerException.GetType() == typeof(FileNotFoundException))
                    {
                        // Try and write the blank index
                        try
                        {
                            items = new BlogIndex(); // Generate the new index to be saved
                            if (WriteBlogIndex())
                                items.Initialised = true;
                        }
                        catch (Exception ex2)
                        {
                            throw BlogException.Passthrough(ex, new CouldNotSaveBlogException(ex2)); // Could not do save of the index
                        }
                    }
                }
                else
                    throw BlogException.Passthrough(ex, new CouldNotLoadBlogException(ex)); // Not a handled issue (such as no index so create one)
            }

            // No item index and not initialised then raise an error
            if (items == null || !items.Initialised)
                throw new NotInitialisedBlogException();
        }
    }
}
