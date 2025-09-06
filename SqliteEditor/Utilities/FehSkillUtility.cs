using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SqliteEditor.Utilities;

public class FehSkillUtility
{
    public static bool EstimateInheritance(string skillName, string? skillType)
    {
        if (skillName.EndsWith("+")
            || skillName.StartsWith("魔器")
            || skillName.EndsWith("4")
            || skillName.EndsWith("3")
            || skillName.EndsWith("2")
            || skillName.EndsWith("1"))
        {
            return true;
        }
        else if (skillType != null && skillType.StartsWith("パッシブ"))
        {
            if (skillName.StartsWith("攻撃") |
                skillName.StartsWith("速さ") |
                skillName.StartsWith("守備") |
                skillName.StartsWith("魔防") |
                skillName.StartsWith("鬼神") |
                skillName.StartsWith("飛燕") |
                skillName.StartsWith("金剛") |
                skillName.StartsWith("明鏡") |
                skillName.StartsWith("攻速") |
                skillName.StartsWith("攻守") |
                skillName.StartsWith("攻魔") |
                skillName.StartsWith("速守") |
                skillName.StartsWith("速魔") |
                skillName.StartsWith("守魔")
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (skillType == "サポート")
        {
            if (skillName.EndsWith("・歩法"))
            {
                return true;
            }

            return false;
        }
        else
        {
            return false;
        }
    }
}
