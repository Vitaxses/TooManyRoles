using TooManyRoles.Roles;
using static TooManyRoles.SettingsHandler;
using static TooManyRoles.Roles.RoleHelper;

namespace TooManyRoles.Objects;

public static class PercentageTypes {
    public static string[] percentages = new string[]{"0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"};
    public static string[] nothing = new string[]{"0", "1"};

    public static int GetPercentageForRole(Role role) {
        int selection = 0;
        switch (role) {
            case Sheriff.SheriffRole:
                selection = sheriffPercentage.getSelection();
            break;
        }
        return selection;
    }

}