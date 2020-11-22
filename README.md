<a href="https://discord.gg/BvKsvS7"><img src="https://img.shields.io/discord/451599029499985920?color=%234e7bd8&label=Discord" alt="Pixel's Community"></a>
<a href="https://github.com/DGvagabond/SCP-008-X/releases"><img src="https://img.shields.io/github/v/release/DGvagabond/SCP-008-X?include_prereleases&label=Release" alt="Releases"></a>
<a href="https://github.com/DGvagabond/SCP-008-X/releases"><img src="https://img.shields.io/github/downloads/DGvagabond/SCP-008-X/total?label=Downloads" alt="Downloads"></a>
<a href="https://discord.gg/PyUkWTg"><img src="https://img.shields.io/discord/656673194693885975?color=%23aa0000&label=EXILED" alt="Support"></a>
<a href="https://github.com/DGvagabond/SCP-008-X"><img src="https://img.shields.io/github/license/DGvagabond/SCP-008-X?color=lightgrey&label=License" alt="License"></a>
# SCP-008-X
An EXILED 2.x plugin for SCP:SL that adds SCP-008 into the game. This is fundamentally for server hosts that want to add more a enganging SCP-049-2 experience for their players.
## How does it work?
It will give **SCP-049-2** the ability to infect it's targets on hit. The targets will receive a custom component that drains their health over time. In order to cure the infection, you must either use `SCP-500` for a guaranteed success or gamble with a `Medkit`'s 50% chance cure rate. Players that die while infected will spawn as SCP-049-2 as well.
# Config Options
| Name | Type | Description | Default |
| --- | --- | --- | --- |
| is_enabled | bool | Toggles the plugin | true |
| debug_mode | bool | Toggles debug messages to your console | false |
| summary_stats | bool | Toggles round summary stats | false |
| infection_chance | int | Percentage chance of infection | 25% |
| cure_chance | int | Percentage chance of being cured when using a medkit | 50% |
| aoe_infection | bool | Toggles infecting players near killed zombies | false |
| aoe_turned | bool | Toggles infecting players near recently turned zombies | false |
| aoe_chance | int | Percentage chance of players near recently turned zombies being infected | 50% |
| buff_doctor | bool | Enable instant revives for SCP-049 | false |
| zombie_health | int | Amount of health infected zombies spawn with | 300 |
| scp008_buff | int | Amount of AHP zombies spawn with and gain on each hit | 10 |
| max_ahp | int | Maximum amount of AHP zombies can reach | 100 |
| cassie_announcement | bool | Toggles the announcement when the round starts | true |
| announcement | string | Sets the CASSIE announcement when the round starts | SCP 0 0 8 containment breach detected . Allremaining |
| zombie_damage | int | Set how much damage SCP-049-2 deals on hit | 24 |
| suicide_broadcast | string | Text that is displayed to all instances of SCP-049-2 | `null` |
| infection_alert | string | A hint that is displayed to players after they're infected | `null` |
| spawn_hint | string | A hint that's displayed to SCP-049-2 on spawn | `null` |
| retain_inventory| bool | Allow players to keep their inventory as zombies. Items can NOT be used by them, this is purely for loot. | true |

# Commands
| Name | Arguments | Description |
| --- | --- | --- |
| infect | player | Infect `player` with SCP-008 |
| cure | player | Cure `player` of SCP-008 |

This plugin is still under development and I plan to add whatever is in high demand from the plugin's users, so feel free to submit your ideas!
If something is not working as intended or outright broken, please submit an issue ticket and I'll look into it as soon as possible!
