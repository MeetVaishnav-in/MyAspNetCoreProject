using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class CustomEmailValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult("Email is required.");
        }

        string email = value.ToString();
        string pattern = @"^[\w]*[a-zA-Z][\w]*@(gmail\.com|utu\.ac\.in|hotmail\.com)$";

        Console.WriteLine($"Validating Email: {email}"); // Debugging output
        if (!Regex.IsMatch(email, pattern))
        {
            return new ValidationResult("Invalid email format.");
        }

        return ValidationResult.Success;
    }
}
