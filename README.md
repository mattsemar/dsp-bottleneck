# Bottleneck

This mod adds some information to the stats panel to help find production bottlenecks. It will show the top 5 (configurable) planets an item is made on
and also try to assess what your assemblers are stuck on (needing items, no power, stacking). It also adds some filter buttons for limiting the items shown to 
only the precursor (or dependent) items to narrow down the search for bottlenecks

![Example](https://github.com/mattsemar/dsp-bottleneck/blob/master/Examples/screenshot.png?raw=true)

## Config

* ProductionPlanetCount allows showing more "Produced on" planets in tooltip (max 15)

## Notes
This mod was originally planned as an enhancement to BetterStats by brokenmass which is why it borrows code from and depends on it.
The plan is to remove that requirement at some point in the future.

## Installation

For now installation is only supported through a mod manager, but, in general these mods must be installed first
* CommonAPI-CommonAPI
* xiaoye97-LDBTool

## Changelog

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
Bugs? Contact me on discord: mattersnot#1983 or create an issue in the github repository.

<div>Icons made by <a href="https://www.flaticon.com/authors/ddara" title="dDara">dDara</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>
