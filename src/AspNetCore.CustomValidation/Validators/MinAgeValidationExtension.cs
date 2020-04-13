﻿// <copyright file="MinAgeValidationExtension.cs" company="TanvirArjel">
// Copyright (c) TanvirArjel. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AspNetCore.CustomValidation.Validators
{
    public static class MinAgeValidationExtension
    {
        /// <summary>
        /// This extension method is used to validate <see cref="DateTime"/> input against dynamic values from database,
        /// configuration file or any external source.
        /// </summary>
        /// <param name="validationContext">The type to be extended.</param>
        /// <param name="propertyName">Name of the <see cref="DateTime"/> type property.</param>
        /// <param name="years">A positive <see cref="int"/> number.</param>
        /// <param name="months">A positive <see cref="int"/> number ranging from 0 to 11. </param>
        /// <param name="days">A positive <see cref="int"/> number ranging from 0 to 31.</param>
        /// <param name="errorMessage">A <see cref="string"/> content to override the default ErrorMessage.</param>
        /// <returns>Return <see cref="ValidationResult"/></returns>
        public static ValidationResult ValidateMinAge(
            this ValidationContext validationContext,
            string propertyName,
            int years,
            int months,
            int days,
            string errorMessage = null)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            PropertyInfo propertyInfo = validationContext.ObjectType.GetProperty(propertyName);

            if (propertyInfo == null)
            {
                throw new ArgumentException($"The object does not contain the property '{propertyName}'");
            }

            var propertyType = propertyInfo.PropertyType;

            if (propertyType != typeof(DateTime) && propertyType != typeof(DateTime?))
            {
                throw new ArgumentException($"Property '{propertyName}' must be {typeof(DateTime)} or {typeof(DateTime?)} type.");
            }

            var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance, null);

            if (propertyValue == null)
            {
                return ValidationResult.Success;
            }

            DateTime dateOfBirth = (DateTime)propertyValue;

            if (dateOfBirth > DateTime.Now)
            {
                return new ValidationResult($"{propertyName} can not be greater than today's date");
            }

            var dateNow = DateTime.Now;
            TimeSpan timeSpan = dateNow.Subtract(dateOfBirth);
            DateTime ageDateTime = DateTime.MinValue.Add(timeSpan);

            var minAgeDateTime = DateTime.MinValue.AddYears(years).AddMonths(months).AddDays(days);

            if (years > 0 || months > 0 || days > 0)
            {
                if (minAgeDateTime > ageDateTime)
                {
                    errorMessage = errorMessage ?? $"Minimum age should be at least {(years > 0 ? years + " years" : string.Empty)} {(months > 0 ? months + " months" : string.Empty)} {(days > 0 ? days + " days" : string.Empty)}.";

                    return new ValidationResult(errorMessage, new[] { propertyName });
                }
            }

            return ValidationResult.Success;
        }
    }
}
