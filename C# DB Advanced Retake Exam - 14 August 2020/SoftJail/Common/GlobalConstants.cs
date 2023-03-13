using System;
using System.Collections.Generic;
using System.Text;

namespace SoftJail.Common
{
    public static class GlobalConstants
    {
        //Prisoner
        public const int PrisonerFullNameMinLength = 3;

        public const int PrisonerFullNameMaxLength = 20;

        public const string PrisonerNickNameRegex = @"^(The\s)([A-Z][a-z]*)$";

        public const int PrisonerMinAgeValue = 18;

        public const int PrisonerMaxAgeValue = 65;

        //Officer
        public const int OfficerFullNameMinLength = 3;

        public const int OfficerFullNameMaxLength = 30;

        //Department
        public const int DepartmentNameMinLength = 3;

        public const int DepartmentNameMaxLength = 25;

        //Cell
        public const int CellNumberMinLength = 1;

        public const int CellNumberMaxLength = 1000;

        //Mail
        public const string MailAddressRegex = @"^([A-Za-z0-9\s]+?)(\sstr.)$";

        //Common
        public const string DecimalMinValue = "0";

        public const string DecimalMaxValue = "79228162514264337593543950335";
    }
}
