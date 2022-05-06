using System;

namespace API.extensions
{
    public static class DateTimeExtensions
    {
        public static int CalcAge(this DateTime DateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;

            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

    }
}