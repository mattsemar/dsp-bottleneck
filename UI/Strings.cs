using Bottleneck.Util;
using HarmonyLib;

namespace Bottleneck.UI
{
    public static class Strings
    {
        private static bool isZHCN = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameOption), "Apply")]
        public static void ApplyLanguageChange()
        {
            LoadStrings();
        }

        public static void LoadStrings()
        {
            isZHCN = false;
            try
            {
                isZHCN = IsZHCN();
            }
            catch (System.Exception e)
            {
                Log.Warn("Get Localization.isZHCN error!" + e);
            }

            RegisterString(ref ClearFilterLabel, "Clear filter", "清除筛选器");
            RegisterString(ref LocalSystemLabel, "Local System", "本地系统");

            // The Chinese display here is /min, so there is no need to translate here, /sec too.
            RegisterString(ref PerMinLabel, "/min", "/min");
            RegisterString(ref PerSecLabel, "/sec", "/sec");

            RegisterString(ref DispPerSecLabel, "Display /sec", "以 /秒 显示");
            RegisterString(ref FilterLabel, "Filter", "筛选");
            
            RegisterString(ref ConsumersLabel, "Consumers", "消耗设施");
            RegisterString(ref ProducersLabel, "Producers", "生产设施");
            RegisterString(ref TheoreticalMaxLabel, "Theoretical max", "理论最大值");

            RegisterString(ref ProdDetailsLabel, "Production Details", "生产详情");
            RegisterString(ref ClickPrecursorText, " (click to show only precursor items)", "（鼠标单击 仅展示所有前置材料）");
            RegisterString(ref ControlClickLacking, "(Control click see only precursors that are lacking)\r\n", "（按住Ctrl+鼠标单击 仅展示产量不足的前置材料）\r\n");
            RegisterString(ref ProducedOnLabel, "Produced on", "生产于");

            RegisterString(ref ConDetailsLabel, "Consumption Details", "消耗详情");
            RegisterString(ref ClickConsumingText, " (click to show only consuming items)", "（鼠标单击 仅展示用于制作的物品）");
            RegisterString(ref ConsumedOnLabel, "Consumed on", "消耗于");

            // text used in bottleneck message for an item
            RegisterString(ref NeedLabel, "Need", "需要");
            RegisterString(ref CurrentLabel, "current", "当前");
            RegisterString(ref StackingLabel, "Stacking", "产物堆积");
            RegisterString(ref UnderPoweredLabel, "Under Powered", "电力不足");
            RegisterString(ref MissingSprayLabel, "Missing spray", "缺少增产剂");
            RegisterString(ref BottlenecksLabel, "Bottlenecks", "瓶颈");
            // text used in proliferator selection
            RegisterString(ref ProliferatorCalculationDisabled, "Proliferator Calculation Disabled", "增产剂计算已禁用");
            RegisterString(ref ProliferatorCalculationDisabledHover, "Don't use Proliferator Points for calculation of Theoretical max values", "不使用增产点数计算理论最大值");
            RegisterString(ref ProliferatorCalculationEnabled, "Proliferator Calculation Enabled", "增产剂计算已启用");

            RegisterString(ref AssemblerSelectionMode, "Assembler Selection Mode", "生产设施当前选择 模式");
            RegisterString(ref AssemblerSelectionHover, "Max values calculated using currently selected mode for each assembler.", "使用每个生产设施 当前选择 的模式计算理论最大值");

            RegisterString(ref ForceProductivityMode, "Force Productivity Mode", "强制 额外产出 模式");
            RegisterString(ref ForceProductivityHover, "Max values calculated as if all all assemblers were set to 'Extra Products'.", "假设每个生产设施使用 额外产出 模式计算理论最大值");

            RegisterString(ref ForceSpeedMode, "Force Speed Mode", "强制 生产加速 模式");
            RegisterString(ref ForceSpeedModeHover, "Max values calculated as if all all assemblers were set to 'Production Speedup'.", "假设每个生产设施使用 生产加速 模式计算理论最大值");
            RegisterString(ref RecipePreText, "Recipe", "配方");
        }

        private static bool IsZHCN()
        {
            return Localization.isZHCN; // Separate so it won't break the whole part
        }

        private static void RegisterString(ref string result, string enTrans, string cnTrans)
        {
            string key = nameof(result);
            string translate = key.Translate();
            if (!string.Equals(key, translate))
            {   // if there is translation
                result = translate;
                return;
            }
            result = isZHCN ? cnTrans : enTrans;
        }

        public static string ClearFilterLabel;
        public static string LocalSystemLabel;
        public static string PerMinLabel;
        public static string PerSecLabel;
        public static string DispPerSecLabel;
        public static string FilterLabel;
        public static string ConsumersLabel;
        public static string ProducersLabel;
        public static string TheoreticalMaxLabel;
        public static string ProdDetailsLabel;
        public static string ClickPrecursorText;
        public static string ControlClickLacking;
        public static string ProducedOnLabel;
        public static string ConDetailsLabel;
        public static string ClickConsumingText;
        public static string ConsumedOnLabel;

        public static string NeedLabel;
        public static string CurrentLabel;
        public static string StackingLabel;
        public static string UnderPoweredLabel;
        public static string MissingSprayLabel;
        public static string BottlenecksLabel;
        public static string ProliferatorCalculationDisabled;
        public static string ProliferatorCalculationDisabledHover;
        public static string ProliferatorCalculationEnabled;
        public static string AssemblerSelectionMode;
        public static string AssemblerSelectionHover;
        public static string ForceProductivityMode;
        public static string ForceProductivityHover;
        public static string ForceSpeedMode;
        public static string ForceSpeedModeHover;
        public static string RecipePreText;
    }
}
