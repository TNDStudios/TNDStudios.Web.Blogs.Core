﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using TNDStudios.Web.Blogs.Core.Controllers;
using TNDStudios.Web.Blogs.Core.RequestResponse;
using TNDStudios.Web.Blogs.Core.ViewModels;

namespace TNDStudios.Web.Blogs.Core.Helpers
{
    public static partial class HtmlHelpers
    {
        /// <summary>
        /// Html Helper to render the blog index without the need to define the display settings (so it uses the defaults)
        /// </summary>
        /// <param name="helper">The HtmlHelper reference to extend the function in to</param>
        /// <param name="blogId">The Id of the blog to render as defined by the param in the controller</param>
        /// <param name="searchParameters">The definition of what to search for</param>
        /// <returns>The Html String output for the helper</returns>
        public static IHtmlContent BlogWidget(this IHtmlHelper helper, String blogId,
            BlogListRequest searchParameters)
            => BlogWidget(helper, blogId, searchParameters, new BlogViewDisplaySettings());

        /// <summary>
        /// Html Helper to render the blog index
        /// </summary>
        /// <param name="helper">The HtmlHelper reference to extend the function in to</param>
        /// <param name="blogId">The Id of the blog to render as defined by the param in the controller</param>
        /// <param name="searchParameters">The definition of what to search for</param>
        /// <param name="viewSettings">How to render the widget (There is an alternative signature without this that uses the defaults)</param>
        /// <returns>The Html String output for the helper</returns>
        public static IHtmlContent BlogWidget(this IHtmlHelper helper, String blogId,
            BlogListRequest searchParameters, BlogViewDisplaySettings viewSettings)
        {
            // Create the tag builders to return to the calling MVC page
            HtmlContentBuilder contentBuilder = new HtmlContentBuilder();

            // Get the template to add to the view model
            KeyValuePair<String, IBlog> blogPair =
                Blogs.Items.Where(
                    blogCheck =>
                        blogCheck.Value.Parameters.Id == blogId
                    ).FirstOrDefault();

            // Did it find the blog?
            if (blogPair.Value != null &&
                blogPair.Value.Templates.ContainsKey(BlogControllerView.Widget))
            {
                /// Get the model from the helper context
                IndexViewModel viewModel = new IndexViewModel()
                {
                    Templates = new BlogViewTemplates() { },
                    SearchParameters = searchParameters,
                    DisplaySettings = viewSettings
                };

                // Populate the viewModel with any common things (such as URL's etc.)
                viewModel.Populate(helper, blogId);

                // Grab the widget templates (the key at least must be there)
                BlogViewTemplates templates =
                    blogPair.Value.Templates[BlogControllerView.Widget];

                // Check to see if there is any value to the templates
                if (templates != null && templates.Templates != null)
                {
                    // Assign the templates
                    viewModel.Templates.Templates =
                        blogPair.Value.Templates[BlogControllerView.Widget].Templates;

                    // Do the search to get the data
                    viewModel.Results = (List<IBlogHeader>)blogPair.Value.List(searchParameters);

                    // Stick the content all together in the table
                    contentBuilder
                        .AppendHtml(BlogWidgetIndexHeader(viewModel))
                        .AppendHtml(BlogWidgetIndexBody(viewModel))
                        .AppendHtml(BlogWidgetIndexFooter(viewModel));
                }
            }

            // Send the tag back
            return contentBuilder;
        }

        /// <summary>
        /// Build the table header tag
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private static IHtmlContent BlogWidgetIndexHeader(IndexViewModel viewModel)
            => ContentFill(BlogViewTemplatePart.Index_Header, new List<BlogViewTemplateReplacement>() { }, viewModel);

        /// <summary>
        /// Build the table footer tag
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private static IHtmlContent BlogWidgetIndexFooter(IndexViewModel viewModel)
            => ContentFill(BlogViewTemplatePart.Index_Footer, new List<BlogViewTemplateReplacement>() { }, viewModel);

        /// <summary>
        /// Build the table body tag
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private static IHtmlContent BlogWidgetIndexBody(IndexViewModel viewModel)
        {
            // Create a content builder just to make the looped items content
            HtmlContentBuilder itemsBuilder = new HtmlContentBuilder();

            // Build the clearfix insert classes from the model templates so we don't have to retrieve them each time
            String clearFixMedium = $" {viewModel.Templates.Get(BlogViewTemplatePart.Index_Clearfix_Medium).GetString()}";
            String clearFixLarge = $" {viewModel.Templates.Get(BlogViewTemplatePart.Index_Clearfix_Large).GetString()}";

            // Loop the results and create the row for each result in the itemsBuilder
            Int32 itemId = 0; // Counter to count the amount of items there are
            viewModel.Results
                .ForEach(blogHeader =>
                {
                    // Add the new item Html
                    itemId++;
                    itemsBuilder.AppendHtml(BlogItem(new BlogItem() { Header = (BlogHeader)blogHeader }, (BlogViewModelBase)viewModel));

                    // Built up template content classes to transpose in the clearfix template should it be needed
                    String clearfixHtml = "";
                    clearfixHtml += (itemId % viewModel.DisplaySettings.ViewPorts[BlogViewSize.Medium].Columns == 0) ? clearFixMedium : "";
                    clearfixHtml += (itemId % viewModel.DisplaySettings.ViewPorts[BlogViewSize.Large].Columns == 0) ? clearFixLarge : "";

                    // Do we have a clearfix to append?
                    if (clearfixHtml != "")
                    {
                        // .. yes we do, append the clearfix with the appropriate template and the class string that was built
                        itemsBuilder.AppendHtml(ContentFill(BlogViewTemplatePart.Index_Clearfix,
                            new List<BlogViewTemplateReplacement>()
                            {
                                    new BlogViewTemplateReplacement(BlogViewTemplateField.Index_BlogItem_ClearFix, clearfixHtml, false)
                            },
                            viewModel));
                    }
                }
                );

            // Call the standard content filler function
            return ContentFill(BlogViewTemplatePart.Index_Body,
                new List<BlogViewTemplateReplacement>()
                {
                    new BlogViewTemplateReplacement(BlogViewTemplateField.Index_Body_Items, itemsBuilder.GetString(), false)
                },
                viewModel);
        }
    }
}