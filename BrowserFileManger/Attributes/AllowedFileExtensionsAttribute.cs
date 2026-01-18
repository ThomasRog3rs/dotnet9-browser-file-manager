using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace BrowserFileManger.Attributes;

public class AllowedFileExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _allowedFileExtensions;

    public AllowedFileExtensionsAttribute(string[] allowedFileExtensions)
    {
        _allowedFileExtensions = allowedFileExtensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        // Handle single file
        if (value is IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            return _allowedFileExtensions.Contains(fileExtension)
                ? ValidationResult.Success
                : new ValidationResult($"Allowed file types are: {string.Join(",", _allowedFileExtensions)}");
        }

        // Handle multiple files
        if (value is IEnumerable<IFormFile> files)
        {
            var fileList = files.ToList();
            if (!fileList.Any())
                return ValidationResult.Success;

            var invalidFiles = new List<string>();
            foreach (var f in fileList)
            {
                var fileExtension = Path.GetExtension(f.FileName).ToLower();
                if (!_allowedFileExtensions.Contains(fileExtension))
                {
                    invalidFiles.Add(f.FileName);
                }
            }

            if (invalidFiles.Any())
            {
                return new ValidationResult(
                    $"The following files have invalid extensions: {string.Join(", ", invalidFiles)}. " +
                    $"Allowed file types are: {string.Join(", ", _allowedFileExtensions)}");
            }

            return ValidationResult.Success;
        }

        return ValidationResult.Success;
    }
    
}