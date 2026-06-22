namespace Movie_StructureCode.Contract.Abstractions.Shared
{
    public static class RoleConstants
    {
        public const string AdminSystem = "ADMIN-SYSTEM";
        public const string StaffSystemAccount = "STAFF-SYSTEM-ACCOUNT";
        public const string AdminTheater = "ADMIN-THEATER";
        public const string ManagementTheater = "MANAGEMENT-THEATER";
        public const string BoxOfficeStaff = "BOX-OFFICE-THEATER";
        public const string User = "USER";
        
        // Composed role lists for filters
        public const string AllAdminAndStaff = "ADMIN-SYSTEM,ADMIN-THEATER,MANAGEMENT-THEATER,BOX-OFFICE-THEATER";
        public const string StaffAndTicket = "BOX-OFFICE-THEATER,MANAGEMENT-THEATER";
        public const string AdminOnly = "ADMIN-SYSTEM,ADMIN-THEATER";

        public const string StaffAndRole = "ADMIN-SYSTEM,ADMIN-THEATER,MANAGEMENT-THEATER,STAFF-SYSTEM-ACCOUNT";
    }
}
