﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Encodings.Web;
using TNDStudios.Blogs.ViewModels;

namespace TNDStudios.Blogs.Helpers
{
    public static partial class HtmlHelpers
    {
        /// <summary>
        /// Get the Model in the proper formt from the IHtmlHelper context
        /// </summary>
        /// <typeparam name="T">The type to be returned</typeparam>
        /// <param name="helper">The IHtmlHelper in context</param>
        /// <returns>The view model in correct class format</returns>
        public static T GetModel<T>(IHtmlHelper helper) where T: BlogViewModelBase, new()
        {
            T returnModel = new T(); // The model that will be returned (Default to new but an error will be raised if needed)

            // Wrap in a try as the helper could be being used in the incorrect context
            try
            {
                returnModel = (T)helper.ViewContext.ViewData.Model; // Cast it

                // Populate any common items required in it
                PopulateModel(returnModel, helper);
            }
            catch (Exception ex)
            {
                // The helper is probably not being used in the right context so the model is wrong
                throw new CastObjectBlogException(ex);
            }

            return returnModel;
        }

        /// <summary>
        /// Get the Model in the proper format from the IHtmlHelper context
        /// alternative signature as BlogViewModelBase is an abstract so can't fall under type T
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static BlogViewModelBase GetModel(IHtmlHelper helper)
            => PopulateModel((BlogViewModelBase)helper.ViewContext.ViewData.Model, helper);

        /// <summary>
        /// Populate common attributes in to the model before returning it if needed
        /// </summary>
        /// <param name="model">The model to be populated</param>
        /// <param name="helper">The helper to use to populate the model</param>
        /// <returns></returns>
        public static BlogViewModelBase PopulateModel(BlogViewModelBase model, IHtmlHelper helper)
        {
            model.ControllerUrl = helper.ViewContext.RouteData.Values["Controller"].ToString(); // Get the Controller route attribute for the Url replacement
            return model; // Return the model
        }

        /// <summary>
        /// Standardised content fill function to provide IHtmlContent based on a 
        /// template and a set of replacement values
        /// </summary>
        /// <param name="part">The id of the template part</param>
        /// <param name="contentValues">A list of replacement values to process in the template</param>
        /// <param name="viewModel">The viewmodel to provide a link to the templates being used</param>
        /// <returns></returns>
        private static IHtmlContent ContentFill(
            BlogViewTemplatePart part, 
            List<BlogViewTemplateReplacement> contentValues, 
            BlogViewModelBase viewModel)
        {
            // Create the tag builders to return to the calling MVC page
            HtmlContentBuilder contentBuilder = new HtmlContentBuilder();

            // Append the processed Html to the content builder
            contentBuilder.AppendHtml(viewModel.Templates.Process(part, contentValues));

            // Return the builder
            return contentBuilder;
        }

        /// <summary>
        /// Get the string content from the IHtmlContent encoded
        /// </summary>
        /// <param name="content">The IHtmlContent package to be rendered</param>
        /// <returns>The rendered string</returns>
        public static String GetString(this IHtmlContent content)
        {
            /// Create an IO writer
            var writer = new StringWriter();

            // Write to a Html Encoder
            content.WriteTo(writer, HtmlEncoder.Default);

            // Get the string content before decoding
            String stringContent = writer.ToString();

            // Return the string to the caller
            return WebUtility.HtmlDecode(stringContent);
        }

        /// <summary>
        /// The translated attachment url (Will not be direct but 
        /// through a controller to relay the data)
        /// </summary>
        /// <param name="item">The blog item the file is attached to</param>
        /// <param name="file">The file to provide the url for</param>
        /// <param name="viewModel">The view model that containers the controller base url (incase there are multiple blogs)</param>
        /// <returns>The url for the file attachment</returns>
        private static String AttachmentUrl(IBlogItem item, BlogFile file, BlogViewModelBase viewModel)
            => viewModel.ControllerUrl;
    }
}
