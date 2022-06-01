# Bottleneck / 瓶颈

这个模组向统计面板添加了一些额外的信息，以帮助您找到生产瓶颈。 它将显示生产物品的前 5 个（数量可配置）行星，
并尝试检测您的生产设施因为什么导致生产瓶颈（需要物品， 电力不足， 物品堆叠）。
它还添加了一些过滤按钮，用于将显示的项目限制为制作于（或用于制作）哪些物品，以缩小对瓶颈的搜索范围

![Example](https://github.com/mattsemar/dsp-bottleneck/blob/master/Examples/screenshot_cn.png?raw=true)

## 带有增产剂计算的 BetterStats

这个插件包含了一个支持增产剂计算的 BetterStats 分支。要使用它，您必须禁用 BetterStats 插件。
分叉的 BetterStats 是完全可选的，安装 BetterStats 后，Bottleneck 插件应该可以正常工作，不会出现增产剂计算的增强功能。

_注意：如果 brokenmass 将[更改](https://github.com/DysonSphereMod/QOL/pull/125)合并到 BetterStats 中，那么这个分叉将消失_

对于可以喷涂增产剂的生产项目，在每个项目旁边添加了按钮，您可以在其中进行选择：

* 禁用 - 在计算项目的理论最大产量时不考虑 增产剂
* 生产设施当前选择 - 计算理论最大值时使用生产设施当前选择模式（更多产品或更高速度）
* 强制 生产加速 - 假设每个生产设施都处于 生产加速 模式，计算理论最大值
* 强制 额外产出 - 假设每个生产设施都处于 额外产出 模式下，计算理论最大值。仅适用于支持 额外产出 的配方

![Proliferator](https://github.com/mattsemar/dsp-bottleneck/blob/master/Examples/stats_buttons.png?raw=true)

## 配置

* 'ProductionPlanetCount' 允许在工具提示中显示更多 “生产于” 的行星（最多 15 个）
* 'Disable Bottleneck' 可让您禁用此 mod 的瓶颈功能并只关注统计数据
* 'Disable Proliferator Calculation' 将 增产剂 从理论最大值计算中完全移除
* 'Planet Filter' 当前置材料 / 消耗设施 过滤器处于激活状态时，本配置会从列表中删除非生产（或非消耗）行星
* 'System Filter' 当行星过滤器处于激活状态时，会在带有生产设施的系统列表中添加一个 “星系统” 项目  
* 'Include Second Level Items' 当前置材料 / 消耗设施 过滤器处于激活状态时，“包括二级项目” 还包括祖父母 / 孙子的前置材料 / 消耗设施  

## 笔记
这个模组最初计划是对 brokenmass 的 BetterStats 的改进。

行星消耗 / 生产仅在统计面板打开后计算一次。如果您在统计面板打开时将生产设施添加到您的生产系统中（也许您正在以非常高分辨率运行？）
那么您必须关闭并重新打开窗口来查看统计面板的更新

## 安装

目前仅支持通过模组管理器进行安装，但通常必须先安装这些模组
* CommonAPI-CommonAPI
* xiaoye97-LDBTool

## 联系
模组有 Bugs? 通过 discord 联系我: Semar#1983 或在 github 中创建issue

<div>模组图标由<a href="https://www.flaticon.com/authors/ddara" title="dDara">dDara</a>制作 来自 <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>
