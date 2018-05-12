﻿using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TNDStudios.Blogs.ViewModels
{
    /// <summary>
    /// Enumeration for the content parts for each of the display templates
    /// </summary>
    [DefaultValue(Unknown)]
    public enum BlogViewTemplatePart : Int32
    {
        Unknown = 0, // When the item cannot be found

        [XmlEnum(Name = "header")]
        [Description("header")]
        [EnumMember(Value = "header")]
        Index_Header = 101,

        [XmlEnum(Name = "body")]
        [Description("body")]
        [EnumMember(Value = "body")]
        Index_Body = 102,

        [XmlEnum(Name = "footer")]
        [Description("footer")]
        [EnumMember(Value = "footer")]
        Index_Footer = 103,

        [XmlEnum(Name = "indexclearfix")]
        [Description("indexclearfix")]
        [EnumMember(Value = "indexclearfix")]
        Index_Clearfix = 105,

        [XmlEnum(Name = "indexclearfix-medium")]
        [Description("indexclearfix-medium")]
        [EnumMember(Value = "indexclearfix-medium")]
        Index_Clearfix_Medium = 106,

        [XmlEnum(Name = "indexclearfix-large")]
        [Description("indexclearfix-large")]
        [EnumMember(Value = "indexclearfix-large")]
        Index_Clearfix_Large = 107,

        [XmlEnum(Name = "item")]
        [Description("item")]
        [EnumMember(Value = "item")]
        Blog_Item = 201,

        [XmlEnum(Name = "edititem")]
        [Description("edititem")]
        [EnumMember(Value = "edititem")]
        Blog_EditItem = 202
    }
}