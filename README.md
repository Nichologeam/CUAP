### Casualties: Unknown Archipelago

This mod is made for the V5PreTesting5 version of Casualties: Unknown, which can be downloaded from the game's [Itch page](https://orsonik.itch.io/scav-prototype). Make sure you download V5PreTesting5 and not V4.1.

# **Setup Guide**
1. Download BepInEx v5.4.23.3 from its [Github Page](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.3)
2. Extract the zip folder into the game's directory
    - If done correctly, a BepInEx folder should appear
3. Launch Casualties Unknown once to configure BepInEx
    - If done correctly, a Plugins folder should appear in the BepInEx folder
4. Download and install the client mod by extracting the CUAPClientModx.x.x.zip folder into the newly created Plugins folder
	- If done correctly, a CUAP folder should appear (not CUAPClientModx.x.x)
5. [If Hosting] Download [Archipelago 0.6.4](https://github.com/ArchipelagoMW/Archipelago/releases/tag/0.6.4) or newer
6. [If Hosting] Download the APWorld and place it in the custom_worlds folder of your Archipelago install
7. Download the .yaml file attached and fill out your desired settings, then place it in the Players folder of your Archipelago install
8. [If Hosting] Open Archipelago and select 'Generate'
9. [If Hosting] Upload the .zip file in the Output folder to [the Archipelago website](https://archipelago.gg/uploads) to host
	- Alternatively, you can host on your own PC by selecting 'Host' in the Archipelago Launcher
10. Launch the game normally and enter the room connection information in the top right
	- DeathLink is toggleable at any time using the game's console (see In-game Text Client below)
11. If anything goes wrong while playing, please let me know. This is still in development and bugs may happen
	- You can contact me in the AP After Dark Discord in the Casualties: Unkown thread in #future-game-design
	- or in the Orsoniks' studio Discord in the C:U Multiworld Randomizer (Archipelago) thread in #art

# **In-game Text Client**
The in-game debug console now has various new Archipelago related commands. You can open the in-game debug console with `.
- aptoggledeathlink [severity]: Enables or disables DeathLink. 'kill' kills Experiment. 'limbdamage' does a moderate amount of damage to a random limb.
- apchat [text]: Send a chat message to Archipelago. Replaces the old Talk command system
- aphint [item]: Alias for `apchat !hint`. Leave [item] empty for a hint status update.
- aphintlocation [location]: Alias for `apchat !hint_location`
- aprelease: Alias for `apchat !release`
- apcollect: Alias for `apchat !collect`
- apcheat: Alias for `apchat !getitem`
- apalias [name]: Alias for `apchat !alias`
- apreportbug: Opens a bug report in this GitHub repository. Optionally takes a screenshot as well.
