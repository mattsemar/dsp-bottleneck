using CommonAPI.Systems;

namespace Bottleneck.UI
{
    public static class Strings
    {
        public static void Init()
        {
            ProtoRegistry.RegisterString("clearFilterLabel", "Clear filter", "清除筛选器");
            ProtoRegistry.RegisterString("localSystemLabel", "Local System", "本地系统");

            // The Chinese display here is /min, so there is no need to translate here, /sec too.
            ProtoRegistry.RegisterString("perMinLabel", "/min", "/min");
            ProtoRegistry.RegisterString("perSecLabel", "/sec", "/sec");

            ProtoRegistry.RegisterString("dispPerSecLabel", "Display /sec", "以 /秒 显示");
            ProtoRegistry.RegisterString("filterLabel", "Filter", "筛选");
            
            ProtoRegistry.RegisterString("consumersLabel", "Consumers", "消耗设施");
            ProtoRegistry.RegisterString("producersLabel", "Producers", "生产设施");
            ProtoRegistry.RegisterString("theoreticalMaxLabel", "Theoretical max", "理论最大值", "Maximum théorique");

            ProtoRegistry.RegisterString("prodDetailsLabel", "Production Details", "生产详情", "Détails de fabrication");
            ProtoRegistry.RegisterString("clickPrecursorText", " (click to show only precursor items)", "（鼠标单击 仅展示所有前置材料）");
            ProtoRegistry.RegisterString("controlClickLacking", "(Control click see only precursors that are lacking)\r\n", "（按住Ctrl+鼠标单击 仅展示产量不足的前置材料）\r\n");
            ProtoRegistry.RegisterString("producedOnLabel", "Produced on", "生产于");

            ProtoRegistry.RegisterString("conDetailsLabel", "Consumption Details", "消耗详情", "Détails de la consommation");
            ProtoRegistry.RegisterString("clickConsumingText", " (click to show only consuming items)", "（鼠标单击 仅展示用于制作的物品）");
            ProtoRegistry.RegisterString("consumedOnLabel", "Consumed on", "消耗于");

            // text used in bottleneck message for an item
            ProtoRegistry.RegisterString("needLabel", "Need", "需要");
            ProtoRegistry.RegisterString("currentLabel", "current", "当前");
            ProtoRegistry.RegisterString("stackingLabel", "Stacking", "产物堆积");
            ProtoRegistry.RegisterString("underPoweredLabel", "UnderPowered", "电力不足");
            ProtoRegistry.RegisterString("missingSprayLabel", "Missing spray", "缺少增产剂");
            ProtoRegistry.RegisterString("bottlenecksLabel", "Bottlenecks", "瓶颈");
            // text used in proliferator selection
            ProtoRegistry.RegisterString("proliferatorCalcDisabledLabel", "Proliferator Calculation Disabled", "增产剂计算已禁用");
            ProtoRegistry.RegisterString("proliferatorCalcDisabledHover", "Don't use Proliferator Points for calculation of Theoretical max values", "不使用增产点数计算理论最大值");
            ProtoRegistry.RegisterString("proliferatorCalcEnabledLabel", "Proliferator Calculation Enabled", "增产剂计算已启用");

            ProtoRegistry.RegisterString("assemblerSelection", "Assembler Selection Mode", "生产设施当前选择 模式");
            ProtoRegistry.RegisterString("assemblerSelectionHover", "Max values calculated using currently selected mode for each assembler.", "使用每个生产设施 当前选择 的模式计算理论最大值");

            ProtoRegistry.RegisterString("forceProductivity", "Force Productivity Mode", "强制 额外产出 模式");
            ProtoRegistry.RegisterString("forceProductivityHover", "Max values calculated as if all all assemblers were set to 'Extra Products'.", "假设每个生产设施使用 额外产出 模式计算理论最大值");

            ProtoRegistry.RegisterString("forceSpeed", "Force Speed Mode", "强制 生产加速 模式");
            ProtoRegistry.RegisterString("forceSpeedHover", "Max values calculated as if all all assemblers were set to 'Production Speedup'.", "假设每个生产设施使用 生产加速 模式计算理论最大值");
        }

        public static string NeedLabel => "needLabel".Translate(Localization.language);
        public static string CurrentLabel => "currentLabel".Translate(Localization.language);
        public static string StackingLabel => "stackingLabel".Translate(Localization.language);
        public static string UnderPoweredLabel => "underPoweredLabel".Translate(Localization.language);
        public static string MissingSprayLabel => "missingSprayLabel".Translate(Localization.language);
        public static string BottlenecksLabel => "bottlenecksLabel".Translate(Localization.language);
        public static string ProliferatorCalculationDisabled => "proliferatorCalcDisabledLabel".Translate(Localization.language);
        public static string ProliferatorCalculationDisabledHover => "proliferatorCalcDisabledHover".Translate(Localization.language);
        public static string ProliferatorCalculationEnabled => "proliferatorCalcEnabledLabel".Translate(Localization.language);
        public static string AssemblerSelectionMode => "assemblerSelection".Translate(Localization.language);
        public static string AssemblerSelectionHover => "assemblerSelectionHover".Translate(Localization.language);
        public static string ForceProductivityMode => "forceProductivity".Translate(Localization.language);
        public static string ForceProductivityHover => "forceProductivityHover".Translate(Localization.language);
        public static string ForceSpeedMode => "forceSpeed".Translate(Localization.language);
        public static string ForceSpeedModeHover => "forceSpeedHover".Translate(Localization.language);

    }
}
