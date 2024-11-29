using System.Linq;
using HarmonyLib;

namespace TooManyRoles.Settings;

public class HeaderSetting : CustomSetting {

    public HeaderSetting(int Id, string Name, MenuType menu)
     : base(Id, Name, menu, SettingType.Header, new string[]{""}, 0) {
        
        
    }

    public override void OnOptionCreated() {
        base.OnOptionCreated();
        Setting.Cast<ToggleOption>().TitleText.text = Name;
    }

}