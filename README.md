# Better Unity

A collection of Unity editor tools built to make your workflow faster, cleaner, and a little less painful. Made by [Lazy Cat](https://github.com/iam-lazycat).

> it works. sometimes. idk.

---

## Features

### Auto Save
Automatically saves your open scenes and assets on a configurable interval. Shows an in-editor notification when it saves so you actually know it happened. You can force-save manually too. Never lose work to a crash again (probably).

### Task List
A built-in to-do list inside the Unity editor. Create tasks, set priorities, add deadlines, attach scene objects, scripts, or links. Supports a Current list and a Backlog so you can pretend you'll get to things later.

### Bulk Rename
Rename multiple GameObjects or assets at once. Supports find & replace, prefix/suffix, number sequences, regex, and case changes. Live preview shows you exactly what will change before you commit.

### Align to Ground
Snaps selected objects to the terrain or ground mesh beneath them using raycasts. Supports rotation to surface normal, configurable sample points, and layer masking. Useful for placing props without doing it one by one.

### Transform Copy Paste
Adds copy/paste buttons directly to the Transform inspector. Copy position, rotation, or scale individually and paste them onto any other object. Small thing, saves a lot of time.

### Toolbar
Adds extra controls to Unity's main toolbar — scene switcher, time scale slider, FPS cap, bookmarks, and a screenshot button. Each item can be toggled on or off individually.

> **Note:** The Better Unity toolbar uses Unity's Overlay system. To enable it, right-click anywhere in the Scene view toolbar and select **Add Overlay**, or open the overlay menu via the **(⋯)** icon and ensure the Better Unity toolbar entry is checked.

### Folder Icons
Color-code or replace folder icons in the Project window. Right-click any folder → Folder Style to pick a preset color, custom texture, or overlay badge. Stored per-project so the whole team sees the same colors.

### Hierarchy
Makes the Hierarchy window actually useful. Adds zebra striping, tree lines, component icons, active toggles, and the ability to mark any object as a styled header row. Right-click or hover any object to set its style or leave a note.

---

## Requirements

- Unity **6.3** or newer
- No third-party dependencies — 100% Unity native

---

## Installation

1. Download or clone this repo
2. Copy the `BetterUnity` folder into your project's `Assets/Editor/` directory
3. Unity will compile it automatically — no setup required
4. Access everything via **Tools → Better Unity** or **Help → Better Unity**

---

## Changelog

#### v5.3.0
- Added Reddit and Instagram links to About window
- Toolbar: added FPS cap control
- Hierarchy: added dim inactive objects option
- Various stability fixes

#### v5.0.0
- Hierarchy module rewritten — tree lines now derived from transform depth, no hardcoded offsets
- Folder Icons: added overlay badge system with preset icons
- Task List: added attachments (objects, scripts, scenes, links)
- Settings cleanup across all modules

#### v4.2.0
- Align to Ground: added multi-sample raycasting and min normal dot threshold
- Transform Copy Paste: now supports multi-object editing
- Toolbar: scene switcher added

#### v4.0.0
- Folder Icons module added
- Task List: priorities and backlog list added
- Hierarchy: component icons and active toggle added

#### v3.1.0
- Bulk Rename: added regex mode and case conversion
- Auto Save: added force-save button and countdown display
- General settings window overhauled

#### v2.0.0
- Task List module added
- Hierarchy: zebra striping and header rows added
- First proper settings page under Project Settings

#### v1.0.0
- Initial release
- Auto Save, Bulk Rename, Transform Copy Paste, basic Toolbar

---

## Links

- GitHub: [github.com/iam-lazycat](https://github.com/iam-lazycat)
- X: [x.com/iam_lazycat](https://x.com/iam_lazycat)
- Discord: [iam_lazycat](https://discord.com/users/iam_lazycat)
- Instagram: [instagram.com/iam_lazycat](https://instagram.com/iam_lazycat)
- Reddit: [reddit.com/user/iam_lazycat](https://reddit.com/user/iam_lazycat)

---

*made by Lazy Cat — too lazy to write more, just try it out*
