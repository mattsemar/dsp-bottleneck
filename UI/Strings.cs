using CommonAPI.Systems;

namespace Bottleneck.UI
{
    public static class Strings
    {
        public static void Init()
        {
            ProtoRegistry.RegisterString("clearFilterLabel", "Clear filter", "清除过滤器");
            ProtoRegistry.RegisterString("localSystemLabel", "Local System", "本地系统");

            ProtoRegistry.RegisterString("perMinLabel", "/min", "/分钟");
            ProtoRegistry.RegisterString("perSecLabel", "/sec", "/第二");

            ProtoRegistry.RegisterString("dispPerSecLabel", "Display /sec", "显示/秒");
            ProtoRegistry.RegisterString("filterLabel", "Filter", "筛选");
            
            ProtoRegistry.RegisterString("consumersLabel", "Consumers", "消费者数量");
            ProtoRegistry.RegisterString("producersLabel", "Producers", "生产者");
            ProtoRegistry.RegisterString("theoreticalMaxLabel", "Theoretical max", "理论最大值", "Maximum théorique");

            ProtoRegistry.RegisterString("prodDetailsLabel", "Production Details", "生产细节", "Détails de fabrication");
            ProtoRegistry.RegisterString("clickPrecursorText", " (click to show only precursor items)", "（单击以仅显示前体项目）");
            ProtoRegistry.RegisterString("controlClickLacking", "(Control click see only precursors that are lacking)\r\n", "（控制单击仅查看缺少的前体）\r\n");
            ProtoRegistry.RegisterString("producedOnLabel", "Produced on", "制作于");

            ProtoRegistry.RegisterString("conDetailsLabel", "Consumption Details", "消费明细", "Détails de la consommation");
            ProtoRegistry.RegisterString("clickConsumingText", " (click to show only consuming items)", "（点击只显示消耗品）");
            ProtoRegistry.RegisterString("consumedOnLabel", "Consumed on", "消费于");

            // text used in bottleneck message for an item
            ProtoRegistry.RegisterString("needLabel", "Need", "需要");
            ProtoRegistry.RegisterString("currentLabel", "current", "当前");
            ProtoRegistry.RegisterString("missingSprayLabel", "Missing spray", "缺少喷雾");
            ProtoRegistry.RegisterString("bottlenecksLabel", "Bottlenecks", "瓶颈");
            // text used in proliferator selection
            ProtoRegistry.RegisterString("proliferatorCalcDisabledLabel", "Proliferator Calculation Disabled", "增殖器计算已禁用");
            ProtoRegistry.RegisterString("proliferatorCalcDisabledHover", "Don't use Proliferator Points for calculation of Theoretical max values", "不要使用增殖器点来计算理论最大值");
            ProtoRegistry.RegisterString("proliferatorCalcEnabledLabel", "Proliferator Calculation Enabled", "扩散器计算已启用");

            ProtoRegistry.RegisterString("assemblerSelection", "Assembler Selection Mode", "汇编程序选择模式");
            ProtoRegistry.RegisterString("assemblerSelectionHover", "Max values calculated using currently selected mode for each assembler.", "使用每个汇编程序当前选择的模式计算的最大值。");

            ProtoRegistry.RegisterString("forceProductivity", "Force Productivity Mode", "强制生产力模式");
            ProtoRegistry.RegisterString("forceProductivityHover", "Max values calculated as if all all assemblers were set to 'Extra Products'.", "计算的最大值好像所有的组装程序都设置为“额外产品”。");

            ProtoRegistry.RegisterString("forceSpeed", "Force Speed Mode", "强制速度模式");
            ProtoRegistry.RegisterString("forceSpeedHover", "Max values calculated as if all all assemblers were set to 'Production Speedup'.", "计算的最大值好像所有的汇编程序都设置为“生产加速”。");
        }

        public static string NeedLabel => "needLabel".Translate(Localization.language);
        public static string CurrentLabel => "currentLabel".Translate(Localization.language);
        public static string StackingLabel => "产物堆积".Translate(Localization.language);
        public static string UnderPoweredLabel => "电力不足".Translate(Localization.language);
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