using System;
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

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if(file == null)
            return ValidationResult.Success;
        
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        return _allowedFileExtensions.Contains(fileExtension)
            ? ValidationResult.Success!
            : new ValidationResult($"Allowed file types are: {string.Join(",", _allowedFileExtensions)}");
    }
    
}