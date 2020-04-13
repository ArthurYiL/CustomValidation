﻿// <copyright file="FileTypeAttribute.cs" company="TanvirArjel">
// Copyright (c) TanvirArjel. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AspNetCore.CustomValidation.Extensions;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.CustomValidation.Attributes
{
    public sealed class FileTypeAttribute : ValidationAttribute
    {
        /// <summary>
        /// This constructor is used to pass a single <see cref="FileType"/> value which against the <see cref="IFormFile"/> will be validated.
        /// </summary>
        /// <param name="fileType">A single <see cref="FileType"/> value.</param>
        public FileTypeAttribute(FileType fileType)
        {
            FileTypes = new FileType[] { fileType };
            ErrorMessage = ErrorMessage ?? "{0} should be in {1} format.";
        }

        /// <summary>
        /// This constructor is used to pass an <see cref="Array"/> of <see cref="FileType"/> values which against the <see cref="IFormFile"/> will be validated.
        /// </summary>
        /// <param name="fileTypes">An <see cref="Array"/> of <see cref="FileType"/>.</param>
        public FileTypeAttribute(FileType[] fileTypes)
        {
            FileTypes = fileTypes;
            ErrorMessage = ErrorMessage ?? "{0} should be in {1} formats.";
        }

        public FileType[] FileTypes { get; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            PropertyInfo propertyInfo = validationContext.ObjectType.GetProperty(validationContext.MemberName);

            if (propertyInfo == null)
            {
                throw new ArgumentException($"The object does not contain any property with name '{validationContext.MemberName}'");
            }

            if (propertyInfo.PropertyType != typeof(IFormFile))
            {
                throw new ArgumentException($"The {nameof(FileAttribute)} is not valid on property type {propertyInfo.PropertyType}" +
                                            $"This Attribute is only valid on {typeof(IFormFile)}");
            }

            if (value != null)
            {
                IFormFile inputFile = (IFormFile)value;

                if (inputFile.Length > 0)
                {
                    if (FileTypes != null && FileTypes.Length > 0)
                    {
                        string[] validFileTypes = FileTypes.Select(ft => ft.ToDescriptionString().ToUpperInvariant()).ToArray();
                        validFileTypes = validFileTypes.SelectMany(vft => vft.Split(',')).ToArray();
                        if (!validFileTypes.Contains(inputFile.ContentType.ToUpperInvariant()))
                        {
                            string[] validFileTypeNames = FileTypes.Select(ft => ft.ToString("G")).ToArray();
                            string validFileTypeNamesString = string.Join(",", validFileTypeNames);
                            var fileTypeErrorMessage = GetFileTypeErrorMessage(ErrorMessage, validationContext.DisplayName, validFileTypeNamesString);
                            return new ValidationResult(fileTypeErrorMessage);
                        }
                    }
                }
                else
                {
                    return new ValidationResult("Selected file is empty.");
                }
            }

            return ValidationResult.Success;
        }

        private string GetFileTypeErrorMessage(string errorMessageString, string propertyName, string fileTypeNamesString)
        {
            return string.Format(CultureInfo.InvariantCulture, errorMessageString, propertyName, fileTypeNamesString);
        }
    }
}
