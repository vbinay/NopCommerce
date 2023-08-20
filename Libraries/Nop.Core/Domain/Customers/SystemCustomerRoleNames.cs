
namespace Nop.Core.Domain.Customers
{
    public static partial class SystemCustomerRoleNames
    {
        public static string Administrators { get { return "Administrators"; } }
        public static int AdministratorsId { get { return 1; } }    // NU-59
        
        public static string ForumModerators { get { return "ForumModerators"; } }
        public static int ForumModeratorsId { get { return 2; } }   // NU-59

        public static string Registered { get { return "Registered"; } }
        public static int RegisteredId { get { return 3; } }    // NU-59

        public static string Guests { get { return "Guests"; } }
        public static int GuestsId { get { return 4; } }    // NU-59

        public static string Vendors { get { return "Vendors"; } }
        public static int VendorId { get { return 5; } }    // NU-59

        #region NU-60
        public static string StoreAdministrators { get { return "StoreAdministrators"; } }
        public static int StoreAdministratorsId { get { return 7; } }

        public static string GlobalAdministrators { get { return "GlobalAdministrators"; } }
        public static int GlobalAdministratorsId { get { return 6; } }
        #endregion
    }
}