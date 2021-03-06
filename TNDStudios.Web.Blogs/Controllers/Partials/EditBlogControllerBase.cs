﻿using Microsoft.AspNetCore.Mvc;
using System;
using TNDStudios.Web.Blogs.Core.Attributes;
using TNDStudios.Web.Blogs.Core.ViewModels;

namespace TNDStudios.Web.Blogs.Core.Controllers
{
    public abstract partial class BlogControllerBase : Controller
    {
        /// <summary>
        /// Route for editing a blog item
        /// </summary>
        /// <returns>The default view</returns>
        [HttpGet]
        [BlogSecurity(permission: BlogPermission.Admin)]
        [Route("[controller]/item/{id}/edit")]
        public virtual IActionResult EditBlog(String id)
            => EditBlogCommon(id);

        /// <summary>
        /// Route for saving the data for a blog item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [BlogSecurity(permission: BlogPermission.Admin)]
        [Route("[controller]/item/{id}/edit")]
        public virtual IActionResult SaveBlogEdit(EditItemViewModel model)
        {
            // Get the blog that is for this controller instance
            if (Current != null)
            {
                // Get the item that needs to be saved
                IBlogItem blogItem = (model.Id == "") ? new BlogItem() : Current.Get(new BlogHeader() { Id = Current.Parameters.Provider.DecodeId(model.Id) });

                // Blog item valid?
                if (blogItem != null)
                {
                    // Update the properties of the blog item from the incoming model
                    blogItem.Copy(model);

                    // (Re)Save the blog item back to the blog handler
                    blogItem = Current.Save(blogItem);
                }
                else
                    throw new ItemNotFoundBlogException("Item with id '{id}' not found");

            }

            // Call the common view handler
            return EditBlogCommon(model.Id);
        }

        /// <summary>
        /// Common call between the verbs to return the blog edit model
        /// </summary>
        /// <returns></returns>
        private IActionResult EditBlogCommon(String id)
        {
            // Get the blog that is for this controller instance
            if (Current != null)
            {
                // Generate the view model to pass
                EditViewModel viewModel = new EditViewModel()
                {
                    Templates = Current.Templates.ContainsKey(BlogControllerView.Edit) ?
                    Current.Templates[BlogControllerView.Edit] : new BlogViewTemplates(),
                };

                viewModel.Item = Current.Get(
                    new BlogHeader()
                    {
                        Id = Current.Parameters.Provider.DecodeId(id)
                    });

                // Pass the view model
                return View(this.ViewLocation("edit"), viewModel);
            }
            else
                return View(this.ViewLocation("edit"), new EditViewModel());
        }
    }
}
