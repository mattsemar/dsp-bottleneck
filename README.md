# Bottleneck

This mod adds some information to the stats panel to help find production bottlenecks. It will show the top 5 (configurable) planets an item is made on
and also try to assess what your assemblers are stuck on (needing items, no power, stacking). It also adds some filter buttons for limiting the items shown to 
only the precursor (or dependent) items to narrow down the search for bottlenecks

![Example](https://github.com/mattsemar/dsp-bottleneck/blob/master/Examples/screenshot.png?raw=true)

## BetterStats with Proliferator

This plugin contains a fork of BetterStats with support for Proliferator. To use it,
you'll have to disable the actual BetterStats plugin, unfortunately. The forked BetterStats
is completely optional, the Bottleneck plugin should work just fine when BetterStats is installed, the proliferator enhancements just won't be present.
_Note: If brokenmass [merges the changes](https://github.com/DysonSphereMod/QOL/pull/125) into BetterStats then this fork will go away_

For production items that can be proliferated, buttons are added next to each item where you can choose between:

* Disable - don't consider Profilerator when determining Theoretical max production for the item  
* Assembler setting - Use the assemblers current setting (more products or more speed) when calculating theoretical max
* Force speed - Calculate theoretical max assuming every assembler is in Production Speedup mode
* Force productivity - Calculate theoretical max assuming every assembler is in Extra Products mode. Only available for recipes that support extra products

![Proliferator](https://github.com/mattsemar/dsp-bottleneck/blob/master/Examples/stats_buttons.png?raw=true)

## Config

* ProductionPlanetCount allows showing more "Produced on" planets in tooltip (max 15)
* 'Disable Bottleneck' lets you disable the Bottleneck functionality of this mod and just focus on stats
* 'Disable Proliferator Calculation' removes Proliferator from Theoretical max calculations completely

## Notes
This mod was originally planned as an enhancement to BetterStats by brokenmass.

Planetary consumption/production is only calculated one time after the statistics window is opened. If you add machines to your factory while the stats window is
open (maybe you're running at a very high resolution?) then you'll have to close and re-open the window to see those values update to reflect the change

## Installation

For now installation is only supported through a mod manager, but, in general these mods must be installed first
* CommonAPI-CommonAPI
* xiaoye97-LDBTool

## Changelog

### v1.0.7
Update: update to sync with latest changes in game. 

### v1.0.6
Bugfix: fix labs not detecting stacking condition 

### v1.0.5
Bugfix: fix detection of non-productive assembler recipe default mode. Assemblers for antimatter treated as if they supported productivity mode 

### v1.0.4
Bugfix: fix initialization issue with enhanced stats version

### v1.0.3
Bugfix: resolve issue with initialization of Proliferator info when using BetterStats official was enabled

### v1.0.2
* Update: combined stats collection with bottleneck calculations
* Update: added 'Disable Bottleneck' config to allow only BetterStats functionality to be used. Removes precursors, made on, etc.
* Update: added detection for unsprayed items in bottleneck calculation

### v1.0.1
Bugfix: handle modded items that are created after this plugin is initted

### v1.0.0
* Update: removed dependency on BetterStats. Now when that plugin is not installed a local fork of it will be used instead 
* Update: Account for usage of proliferator in local BetterStats fork
* Update: Detect stacking for Ray Receivers generating critical photons

### v0.1.4
Update: Sync with update in game code that removed the 'outputing' field on assemblers and labs 

### v0.1.3
Bugfix: Changed low power check to be less sensitive. Should give fewer false positives 

### v0.1.2
Bugfix: Fixed issue where stats for modded items would not show up properly 

### v0.1.1
Bugfix: reduced the frequency of computation for planetary production/consumption to address some reported UI lag (thanks to thedoc31 for report) 

### v0.1.0
Feature: added support for filtering only precursors that are needed (hold control when clicking the precursor filter)
Feature: added popup warning when a planet with too little power are detected (one popup per planet per game), use config property to disable
Increased ProductionPlanetCount config property max to 35. 

### v0.0.4
Bugfix: fixed issue where multiple clear filter buttons would be added to stats window.  
Bugfix: updated logic for determining whether assemblers/labs are currently stacking to match game better  

### v0.0.3
Bugfix: fixed exception in stats view when no tech is currently being researched. (Thanks Valoneu for report)

### v0.0.2
Stopgap bugfix, ended up being useless

### v0.0.1
Initial version

## Contact
Bugs? Contact me on discord: Semar#1983 or create an issue in the github repository.

<div>Icons made by <a href="https://www.flaticon.com/authors/ddara" title="dDara">dDara</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>
