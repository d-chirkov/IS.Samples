using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Site2
{
    public static class UserAccessControl
    {
        private static List<string> usersWithAccess = new List<string>();

        public static void AddAccessTo(params string[] userIds)
        {
            UserAccessControl.usersWithAccess.AddRange(userIds);
            UserAccessControl.usersWithAccess = UserAccessControl.usersWithAccess.Distinct().ToList();
        }

        public static bool HasAccess(string userId)
        {
            return UserAccessControl.usersWithAccess.Contains(userId);
        }
    }
}