### Casualties: Unknown Archipelago

This version of the mod is made for the V7.0.1 version of Casualties: Unknown, which is version of the game's [Steam demo](https://store.steampowered.com/app/4576490/Casualties_Unknown/).
This version of the mod is no longer receiving updates. For the Itch.io version, check out [its branch](https://github.com/Nichologeam/CUAP/tree/Itch.io).

# **Installing The Mod**
1. Download BepInEx v5.4.23.5 from its [Github Page](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5)
2. Extract the zip folder into the game's directory
    - If done correctly, a BepInEx folder should appear
3. Launch Casualties Unknown once to configure BepInEx
    - If done correctly, a Plugins folder should appear in the BepInEx folder
4. Download and install the client mod by extracting the CUAPClientModx.x.x.zip folder into the newly created Plugins folder
	- If done correctly, a CUAP folder should appear (not CUAPClientModx.x.x)

# **Creating Your .yaml Settings**
There are two ways to edit your .yaml file, and what you pick comes down to convenience.
### **Through Archipelago**
1. Download [Archipelago 0.6.7](https://github.com/ArchipelagoMW/Archipelago/releases/tag/0.6.8) or newer
2. Download the APWorld and place it into the custom_worlds folder of your Archipelago install, restarting the launcher if it was open
     - If you don't know where this is, the Archipelago Launcher comes with a `Browse Files` option
     - Fresh installs of Archipelago also create a desktop shortcut to the Archipelago folder
3. In the Archipelago Launcher, open the `Options Creator`, and select `Casualties: Unknown` on the left side
4. Fill out your desired options. You can hover over the option names for descriptions on what each of them does
5. Once finished, select `Export` in the top right corner. Send that file to the world host, or check the [Archipelago Website](https://archipelago.gg/tutorial/Archipelago/setup_en#hosting-an-archipelago-server) for instructions on how to host yourself

### **Through a Text Editor**
This method is more hands-on, but you don't have to download anything more than the .yaml itself
1. Download the .yaml file from the latest release
2. Open it in the text editor of your choice
3. Fill out your desired settings. The text after the `#` beneath each option describes what it does
4. Once finished, save the file and send it to the world host. You need the Archipelago Launcher to host yourself. Check the [Archipelago Website](https://archipelago.gg/tutorial/Archipelago/setup_en) for more information

# **Connecting To The Multiworld**
1. Get the world's connection information. If you are hosting on archipelago.gg, this is directly above the playerlist, in the format of `/connect archipelago.gg:xxxxx`
2. Open Casualties: Unknown. An Archipelago GUI should show up on the right side
3. Enter the connection info as shown:
      - The top textbox is the server IP, which is that `archipelago.gg:xxxxx` on the connection page. If the server is hosted on your own PC, this is `localhost`
      - The middle textbox is your slot name. This is the name inside your .yaml file
      - The bottom textbox is the server's password
4. Upon connecting, the game's console (assigned to ~ on QWERTY keyboards) can be used as a text client. See the section below for more info
5. If anything goes wrong while playing, please let me know. You can contact me...
	- In the AP After Dark Discord in the Casualties: Unknown thread in #future-game-design
	- In the Target Planet Discord in the C:U Multiworld Randomizer (Archipelago) thread in #art
    - In the Casualties Unknown Modding Community Discord in the C:U Multiworld Randomizer (Archipelago) thread in #public-creations

# **In-game Text Client**
The in-game debug console now has various new Archipelago-related commands. You can open the in-game debug console with ~
- apdeathlink [severity]: Enables or disables DeathLink. 'kill' kills Experiment. 'limbdamage' does a moderate amount of damage to a random limb
- apchat [text]: Send a chat message to Archipelago
- aphint [item]: Alias for `apchat !hint`. Leave [item] empty for a hint status update
- aphintlocation [location]: Alias for `apchat !hint_location`
- aprelease: Alias for `apchat !release`
- apcollect: Alias for `apchat !collect`
- apcheat: Alias for `apchat !getitem`
- apalias [name]: Alias for `apchat !alias`
- aplang [file]: Change the mod's langauge to a different one. See the next section for more information
- apgift [inventorySlot] [Player]: Gift an item to another player. Must be enabled in the YAML for both players
### **Debug Commands**
- apreportbug: Opens a bug report in this GitHub repository. Optionally takes a screenshot as well
- apresetantispam: Clears local records of sent checks, allowing them to be sent again. Use this if a certain location isn't sending
- apsetskill: Forces Skillsanity's received skills to be a certain value. Use this if your skills desync from what you've actually received
- apfixquests: Forces Moodlesanity's questboard to refresh which checks have been sent. Use this if a quest didn't send properly

# **Custom Translations**
This mod supports custom translations! Inside the lang folder of both this repo and the client mod, there is an included EN.json file that anyone can edit to support any lanuage they want. Simply made a copy, change the text, and run `aplang` ingame! A few things to note:
- Text inside \<angled brackets\> is to be left alone. Changing the text inside the brackets can cause the mod to break or behave oddly.
- Some text will be sent to the Archipelago server, such as when a Deathlink is sent.
